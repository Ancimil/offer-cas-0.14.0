#region imports
using AssecoCurrencyConvertion;
using MicroserviceCommon.Models;
using MicroserviceCommon.Services;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Offer.Domain.AggregatesModel.ApplicationAggregate;
using Offer.Domain.AggregatesModel.ApplicationAggregate.Interfaces;
using Offer.Domain.AggregatesModel.ApplicationAggregate.ProductModel;
using Offer.Domain.Calculations;
using Offer.Domain.Exceptions;
using Offer.Domain.Services;
using PriceCalculation.Calculations;
using PriceCalculation.Exceptions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using OfferApplication = Offer.Domain.AggregatesModel.ApplicationAggregate.Application;
#endregion

namespace Offer.Domain.Utils
{
    public class ArrangementRequestFactory
    {
        private readonly IConfigurationService _configurationService;
        private readonly IProductService _productService;
        private readonly IArrangementService _arrangementService;
        private readonly ICampaignService _campaignService;
        private readonly ILogger<ArrangementRequestFactory> _logger;
        private readonly OfferPriceCalculation _priceCalculator;
        private readonly CalculatorProvider _calculatorProvider;
        private readonly IProductSnapshotRepository _productSnapshotRepository;

        public ArrangementRequestFactory(IConfigurationService configurationService,
            IProductService productService,
            IArrangementService arrangementService,
            ICampaignService campaignService,
            ILogger<ArrangementRequestFactory> logger,
            OfferPriceCalculation priceCalculator,
            CalculatorProvider calculatorProvider,
            IProductSnapshotRepository productSnapshotRepository)
        {
            _configurationService = configurationService ?? throw new ArgumentNullException(nameof(configurationService));
            _productService = productService ?? throw new ArgumentNullException(nameof(productService));
            _arrangementService = arrangementService ?? throw new ArgumentNullException(nameof(arrangementService));
            _campaignService = campaignService ?? throw new ArgumentNullException(nameof(campaignService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _calculatorProvider = calculatorProvider ?? throw new ArgumentNullException(nameof(calculatorProvider));
            _priceCalculator = priceCalculator ?? throw new ArgumentNullException(nameof(priceCalculator));
            _productSnapshotRepository = productSnapshotRepository ?? throw new ArgumentNullException(nameof(productSnapshotRepository));
        }

        public async Task<List<ArrangementRequest>> AddToApplication(OfferApplication application, string productCode,
            ArrangementRequestInitializationParameters parameters)
        {
            application.ArrangementRequests = application.ArrangementRequests ?? new List<ArrangementRequest>();
            // Get product data
            var productData = await _productService.GetProductData(productCode, "documentation", application.CustomerNumber);
            if (!ShouldAddProduct(application, productData))
            {
                _logger.LogInformation("Product not added because it doesn't satisfies all conditions");
                return null;
            }
            var response = new List<ArrangementRequest>();
            // Bootstrap arr request
            var arrangementRequest = await BootstrapArrangementRequest(parameters, productData, application);

            application.ArrangementRequests.Add(arrangementRequest);
            response.Add(arrangementRequest);

            // Resolve bundle
            if (!(arrangementRequest is AbstractArrangementRequest))
            {
                var resolved = await ResolveBundleComponents(application, productData);
                if (resolved != null)
                {
                    response.AddRange(resolved);
                }
            }
            PerformCalculation(application, arrangementRequest);

            return response;
        }

        public async Task<List<ArrangementRequest>> AddBundleComponentToApplication(OfferApplication application, string productCode,
            BundledProductInfo bundleInfo, string parentProductCode,
            ArrangementRequestInitializationParameters parameters = null)
        {
            application.ArrangementRequests = application.ArrangementRequests ?? new List<ArrangementRequest>();
            // Get product data
            ProductSnapshot productData;
            try
            {
                productData = await _productService.GetProductData(productCode, "documentation", application.CustomerNumber);
            }
            catch
            {
                productData = null;
            }
            if (!ShouldAddProduct(application, productData))
            {
                _logger.LogInformation("Bundled product not added because it doesn't satisfies all conditions");
                return null;
            }
            if (bundleInfo.ProductKind != ProductKinds.AbstractProduct && productData.Kind != bundleInfo.ProductKind)
            {
                var e = new Exception("Product Kinds on Bundle Info and on Product Data are different.");
                _logger.LogError(e, "Error occurred while adding bundle component " + bundleInfo.ProductCode +
                    " from parent product " + parentProductCode + " to application");
                throw e;
            }
            var response = new List<ArrangementRequest>();
            // Bootstrap arr request
            var arrangementRequest = await BootstrapArrangementRequest(parameters, productData, application);
            arrangementRequest.ProductName = arrangementRequest.IsAbstractOrigin ?
                arrangementRequest.ProductName : (bundleInfo.ProductName ?? arrangementRequest.ProductName);
            arrangementRequest.ParentProductCode = parentProductCode;
            arrangementRequest.BundleInfo = bundleInfo;
            arrangementRequest.ProductCode = productData.ProductCode;
            application.ArrangementRequests.Add(arrangementRequest);
            arrangementRequest.IsOptional = IsOptional(bundleInfo);
            response.Add(arrangementRequest);

            // Resolve bundle
            if (!(arrangementRequest is AbstractArrangementRequest))
            {
                // TODO Reconsider snapshoting unresolved nested bundles instead of ignoring them (how they wil be solved afterwards?)
                var resolveNestedBundles = (await _configurationService.GetEffective("offer/bundled-products/resolve-nested", "false"))
                    .Equals("true");
                if (resolveNestedBundles)
                {
                    var resolved = await ResolveBundleComponents(application, productData);
                    if (resolved != null)
                    {
                        response.AddRange(resolved);
                    }
                }
            }
            PerformCalculation(application, arrangementRequest);
            return response;
        }

        public bool IsOptional(BundledProductInfo bundledProductInfo)
        {
            return bundledProductInfo == null || bundledProductInfo.MinimalNumberOfInstances == 0;
        }

        private bool ShouldAddProduct(OfferApplication application, ProductSnapshot productData)
        {
            if (productData == null)
            {
                return false;
            }
            // Jel treba ovo standalone da bude?. Proveriti
            if (!productData.IsSingleton)
            {
                return true;
            }

            return !application.ArrangementRequests.Any(r => r.ProductCode.Equals(productData.ProductCode));
        }

        public async Task<ArrangementRequest> BootstrapArrangementRequest(ArrangementRequestInitializationParameters parameters,
            ProductSnapshot productData, OfferApplication application)
        {
            #region Create initial
            ArrangementKind? arrangementKind = OfferUtility.GetArrangmentKindByProductKind(productData.Kind);
            parameters = GetInitializationParametersFromProduct(productData, parameters) ?? new ArrangementRequestInitializationParameters();
            var arrangementRequest = GetForProductKind(parameters, productData);
            arrangementRequest.ArrangementRequestId = GetNextRequestIdForApplication(application);
            arrangementRequest.Application = application;
            arrangementRequest.ProductSnapshot = productData;
            arrangementRequest.ArrangementKind = arrangementKind;
            arrangementRequest.ProductName = arrangementRequest.ProductName ?? productData.Name;
            arrangementRequest.CalculationDate = arrangementRequest.CalculationDate ?? DateTime.UtcNow;
            arrangementRequest.ProductCode = parameters.ProductCode ?? productData.ProductCode;
            arrangementRequest.Conditions = parameters.Conditions;
            arrangementRequest.Options = parameters.ProductOptions;
            arrangementRequest.IsAbstractOrigin = parameters.IsAbstractOrigin ?? false;
            arrangementRequest.RepaymentType = parameters.RepaymentType ?? null;
            arrangementRequest.InstallmentScheduleDayOfMonth = parameters.InstallmentScheduleDayOfMonth;
            #endregion
            arrangementRequest.ProductSnapshotDb = await _productSnapshotRepository.PostProductSnapshot(productData);

            if (arrangementRequest is FinanceServiceArrangementRequest ara)
            {
                #region Resolve for Finance service
                arrangementRequest = AddCollateralRequirements(ara);
                var conversionMethod = _configurationService.GetEffective("offer/fee-currency-conversion-method", "Buy to middle").Result;
                var domesticCurrency = _configurationService.GetEffective("domestic-currency", "RSD").Result;

                if (domesticCurrency != null)
                {
                    if (domesticCurrency == parameters.Currency)
                    {
                        ara.AmountInDomesticCurrency = parameters.Amount ?? 0;
                    }
                    else if (ara.Amount != 0)
                    {
                        var financial = (FinanceServiceArrangementRequest)arrangementRequest;
                        CurrencyConverter currencyConverter = new CurrencyConverter();
                        financial.AmountInDomesticCurrency = currencyConverter.CurrencyConvert(financial.Amount, financial.Currency,
                            domesticCurrency, DateTime.Today.ToString("o", CultureInfo.InvariantCulture), conversionMethod);
                    }
                }
                #endregion
            }


            if (arrangementRequest is OverdraftFacilityRequest)
            {
                #region Resolve accounts for overdraft
                var arrangementList = await _arrangementService.GetArrangements(parameters.CustomerNumber);

                var arr = arrangementList?.FirstOrDefault();
                if (arr != null)
                {
                    List<ArrangementAccountInfo> arrangementAccounts = new List<ArrangementAccountInfo>();
                    JArray accountList = (JArray)arr["accounts"];
                    var primaryAccounts = accountList.Where(x => x["role-kind"].ToString().Equals("primary-account")).ToList();
                    foreach (var account in primaryAccounts)
                    {
                        var accountNumber = account["account-number"].ToString();
                        ArrangementAccountInfo newAccount = new ArrangementAccountInfo
                        {
                            AccountNumber = accountNumber,
                            RoleKind = ArrangementAccountRoleKind.SettlementAccount
                        };
                        arrangementAccounts.Add(newAccount);
                    }
                    arrangementRequest.Accounts = arrangementAccounts;
                }
                #endregion
            }

            if (!string.IsNullOrEmpty(parameters.CustomerNumber))
            {
                #region Resolve campaigns
                var leadList = await _campaignService.GetCampaigns(parameters.CustomerNumber);
                var productCampaign = leadList?.Leads?.Where(l => l.ProductCode == arrangementRequest.ProductCode).FirstOrDefault();
                arrangementRequest.ProductSnapshot.Campaign = productCampaign;
                arrangementRequest.Campaign = productCampaign;
                #endregion
            }

            if (!parameters.MaturityDate.HasValue && arrangementRequest.IsFinanceService() && parameters.Term != null)
            {
                FinanceServiceArrangementRequest fsr = arrangementRequest as FinanceServiceArrangementRequest;
                fsr.MaturityDate = Utility.GetEndDateFromPeriod(parameters.Term);
            }
            return arrangementRequest;
        }

        public int GetNextRequestIdForApplication(OfferApplication application)
        {
            if (application.ArrangementRequests == null || application.ArrangementRequests.Count() == 0)
            {
                return 1;
            }
            return application.ArrangementRequests.Max(r => r.ArrangementRequestId) + 1;
        }

        public ArrangementRequestInitializationParameters GetInitializationParametersFromProduct(ProductSnapshot productData,
            ArrangementRequestInitializationParameters parameters)
        {
            if (productData == null)
            {
                return null;
            }
            parameters = parameters ?? new ArrangementRequestInitializationParameters();
            parameters.Amount = parameters.Amount ?? productData.DefaultParameters?.Amount ?? ((productData.MinimalAmount?.Amount ?? 0) + (productData.MaximalAmount?.Amount ?? 2)) / 2;
            parameters.Term = parameters.Term ?? productData.DefaultParameters?.Term ?? productData.MaximalTerm;
            parameters.Currency = parameters.Currency ?? productData.PrimaryCurrency ?? productData.MinimalAmount?.Code ?? productData.AllowedCurrencies?.FirstOrDefault();
            parameters.IsRefinancing = parameters.IsRefinancing ?? productData.DefaultParameters?.IsRefinancing ?? (string.IsNullOrEmpty(productData.Refinancing) ? false : productData.Refinancing.Equals("always"));

            parameters.DownpaymentPercentage = parameters.DownpaymentPercentage ?? productData.DefaultParameters?.DownpaymentPercentage ?? (productData.MinimalDownpaymentPercentage == 0 ? 20 : productData.MinimalDownpaymentPercentage);
            parameters.InvoiceAmount = parameters.InvoiceAmount ?? productData.DefaultParameters?.InvoiceAmount ?? (parameters.Amount / (1 - parameters.DownpaymentPercentage / 100));
            parameters.DownpaymentAmount = parameters.InvoiceAmount == 0 ? 0 : parameters.InvoiceAmount - parameters.Amount;

            parameters.ProductCode = parameters.ProductCode ?? productData.ProductCode;
            parameters.RevolvingPercentage = parameters.RevolvingPercentage ?? productData.DefaultParameters?.RevolvingPercentage ?? (string.IsNullOrEmpty(productData.AvailableRevolvingPercentage) ? (decimal?)null : decimal.Parse(productData.AvailableRevolvingPercentage.Split(",").First()));

            return parameters;
        }

        public ArrangementRequest GetForProductKind(ArrangementRequestInitializationParameters parameters, ProductSnapshot productData = null)
        {
            return GetForArrangementKind(parameters, OfferUtility.GetArrangmentKindByProductKind(productData.Kind), productData);
        }

        public async Task<List<ArrangementRequest>> ResolveBundleComponents(OfferApplication application, ProductSnapshot productData)
        {
            if (productData.BundledProducts == null || productData.BundledProducts.Count() == 0)
            {
                return null;
            }
            application.OriginatesBundle = true;
            var response = new List<ArrangementRequest>();
            var createIfOptional = (await _configurationService
                .GetEffective("offer/bundled-products/create-if-optional", "true")).Equals("true");
            foreach (var bundled in productData.BundledProducts)
            {
                var instancesToCreate = bundled.MinimalNumberOfInstances > 0 ? bundled.MinimalNumberOfInstances :
                    (createIfOptional ? 1 : 0);
                if (bundled.ProductKind == ProductKinds.AbstractProduct)
                {
                    var resolved = await AddAbstractProduct(application, bundled, instancesToCreate);
                    if (resolved != null)
                    {
                        response.AddRange(resolved);
                    }
                }
                else
                {
                    for (int i = 0; i < instancesToCreate; i++)
                    {
                        var resolved = await AddBundleComponentToApplication(application, bundled.ProductCode, bundled, productData.ProductCode);
                        if (resolved != null)
                        {
                            if (createIfOptional && bundled.MinimalNumberOfInstances == 0)
                            {
                                foreach (var item in resolved)
                                {
                                    item.Enabled = false;
                                }
                            }
                            response.AddRange(resolved);
                        }
                    }
                    if (instancesToCreate == 0)
                    {
                        var resolved = await AddBundleComponentToApplication(application, bundled.ProductCode, bundled, productData.ProductCode);
                        foreach (var item in resolved)
                        {
                            item.Enabled = false;
                        }
                        if (resolved != null)
                        {
                            response.AddRange(resolved);
                        }
                    }
                }
            }
            return response;
        }

        public ArrangementRequest GetForArrangementKind(ArrangementRequestInitializationParameters parameters,
            ArrangementKind? arrangementKind, ProductSnapshot productData = null)
        {
            var arrangementRequest = new ArrangementRequest();
            switch (arrangementKind)
            {
                case ArrangementKind.TermLoan:
                    var termLoanRequest = new TermLoanRequest
                    {
                        Amount = parameters.Amount ?? 0,
                        Currency = parameters.Currency,
                        Term = parameters.Term,
                        Annuity = parameters.Annuity ?? 0,
                        DownpaymentAmount = parameters.DownpaymentAmount ?? 0,
                        Napr = parameters.InterestRate ?? 0,
                        RepaymentPeriod = parameters.RepaymentPeriod,
                        RepaymentPeriodStartDate = parameters.RepaymentPeriodStartDate,
                        GracePeriod = parameters.GracePeriod,
                        GracePeriodStartDate = parameters.GracePeriodStartDate,
                        DrawdownPeriod = parameters.DrawdownPeriod,
                        DrawdownPeriodStartDate = parameters.DrawdownPeriodStartDate,
                        MaturityDate = parameters.MaturityDate,
                        DownpaymentPercentage = parameters.DownpaymentPercentage,
                        InvoiceAmount = parameters.InvoiceAmount ?? 0,
                        IsRefinancing = parameters.IsRefinancing ?? false,
                        RepaymentType = parameters.RepaymentType,
                        InstallmentScheduleDayOfMonth = parameters.InstallmentScheduleDayOfMonth
                    };
                    arrangementRequest = termLoanRequest;
                    break;
                case ArrangementKind.CardAccessArrangement:
                    arrangementRequest = new CardAccessArrangementRequest();
                    break;
                case ArrangementKind.CreditCardFacility:
                    var creditCardFacilityRequest = new CreditCardFacilityRequest
                    {
                        Amount = parameters.Amount ?? 0,
                        Currency = parameters.Currency,
                        Napr = parameters.InterestRate ?? 0,
                        Term = Utility.GetMonthsFromPeriod(parameters.Term).ToString(),
                        MaturityDate = parameters.MaturityDate,
                        RevolvingPercentage = parameters.RevolvingPercentage ?? 0,
                        MinimalRepaymentPercentage = productData?.MinimalRepaymentPercentage ?? 0,
                        MinimalRepaymentAmount = new Currency
                        {
                            Amount = productData?.MinimalRepaymentAmount?.Amount ?? 0,
                            Code = productData?.MinimalRepaymentAmount?.Code ?? "EUR"
                        },
                        RepaymentType = parameters.RepaymentType,
                        InstallmentScheduleDayOfMonth = parameters.InstallmentScheduleDayOfMonth
                    };
                    arrangementRequest = creditCardFacilityRequest;
                    break;
                case ArrangementKind.CreditFacility:
                    var creditFacilityRequest = new CreditFacilityRequest
                    {
                        Amount = parameters.Amount ?? 0,
                        Currency = parameters.Currency,
                        Napr = parameters.InterestRate ?? 0,
                        MaturityDate = parameters.MaturityDate,
                        MinimalRepaymentAmount = new Currency
                        {
                            Amount = productData?.MinimalRepaymentAmount?.Amount ?? 0,
                            Code = productData?.MinimalRepaymentAmount?.Code ?? "EUR"
                        }
                    };
                    arrangementRequest = creditFacilityRequest;
                    break;
                case ArrangementKind.CurrentAccount:
                    var currentAccountRequest = new CurrentAccountRequest
                    {
                        Currency = parameters.Currency,
                        Napr = parameters.InterestRate ?? 0
                    };
                    arrangementRequest = currentAccountRequest;
                    break;
                case ArrangementKind.DemandDeposit:
                    var demandDepositRequest = new DemandDepositRequest
                    {
                        Currency = parameters.Currency,
                        Napr = parameters.InterestRate ?? 0
                    };
                    arrangementRequest = demandDepositRequest;
                    break;
                case ArrangementKind.ElectronicAccessArrangement:
                    arrangementRequest = new ElectronicAccessArrangementRequest();
                    break;
                case ArrangementKind.OtherProductArrangement:
                    arrangementRequest = new OtherProductArrangementRequest();
                    break;
                case ArrangementKind.OverdraftFacility:
                    var overdraftFacilityRequest = new OverdraftFacilityRequest
                    {
                        Amount = parameters.Amount ?? 0,
                        Currency = parameters.Currency,
                        Term = Utility.GetMonthsFromPeriod(parameters.Term).ToString(),
                        Napr = parameters.InterestRate ?? 0,
                        MaturityDate = parameters.MaturityDate,
                        RepaymentType = parameters.RepaymentType,
                        InstallmentScheduleDayOfMonth = parameters.InstallmentScheduleDayOfMonth
                    };
                    arrangementRequest = overdraftFacilityRequest;
                    break;
                case ArrangementKind.SecuritiesArrangement:
                    arrangementRequest = new SecuritiesArrangementRequest();
                    break;
                case ArrangementKind.TermDeposit:
                    var termDepositRequest = new TermDepositRequest
                    {
                        Amount = parameters.Amount ?? 0,
                        Currency = parameters.Currency,
                        Term = parameters.Term,
                        Napr = parameters.InterestRate ?? 0,
                        MaturityDate = parameters.MaturityDate
                    };
                    arrangementRequest = termDepositRequest;
                    break;
                case ArrangementKind.Abstract:
                    arrangementRequest = new AbstractArrangementRequest
                    {
                        IsAbstractOrigin = true
                    };
                    break;
                case ArrangementKind.CreditLine:
                    var creditLineRequest = new CreditLineRequest
                    {
                        Amount = parameters.Amount ?? 0,
                        Currency = parameters.Currency,
                        Term = parameters.Term
                    };
                    arrangementRequest = creditLineRequest;
                    break;
                default:
                    arrangementRequest = new ArrangementRequest();
                    break;
            }
            arrangementRequest.Conditions = parameters.Conditions;
            arrangementRequest.Periods = parameters.ScheduledPeriods;
            arrangementRequest.CalculationDate = parameters.CalculationDate;
            return arrangementRequest;
        }

        private FinanceServiceArrangementRequest AddCollateralRequirements(FinanceServiceArrangementRequest request)
        {
            if (request.ProductSnapshot.AvailableCollateralModelsData.Count > 0)
            {
                ProductCollateralModel selectedModel = null;
                if (!String.IsNullOrEmpty(request.ProductSnapshot.DefaultCollateralModel))
                {
                    selectedModel = request.ProductSnapshot.AvailableCollateralModelsData.Find(c => c.Code == request.ProductSnapshot.DefaultCollateralModel);
                }
                if (selectedModel == null)
                {
                    selectedModel = request.ProductSnapshot.AvailableCollateralModelsData[0];
                }

                request.CollateralModel = selectedModel.Code;
                if (request.CollateralRequirements == null)
                {
                    request.CollateralRequirements = new List<CollateralRequirement>();
                }
                foreach (var requirement in selectedModel.CollateralRequirements)
                {
                    var collateralRequirement = new CollateralRequirement
                    {
                        // Application = request.Application,
                        ArrangementRequestId = request.ArrangementRequestId,
                        CollateralRequirementId = (request.CollateralRequirements.Count > 0) ?
                            request.CollateralRequirements.Max(x => x.CollateralRequirementId) + 1 : 1,
                        CollateralArrangementCode = requirement.CollateralArrangementCode,
                        FromModel = true,
                        MinimalCoverage = requirement.MinimalCoverage,
                        MinimalCoverageInLoanCurrency = request.Amount * (requirement.MinimalCoverage / 100),
                        ActualCoverage = 0
                    };
                    request.CollateralRequirements.Add(collateralRequirement);
                }
            }
            return request;
        }

        private void PerformCalculation(OfferApplication application, ArrangementRequest request)
        {
            if (request is FinanceServiceArrangementRequest finR)
            {
                try
                {
                    request = _calculatorProvider.Calculate(finR, application);
                    //var conversionMethod = _configurationService.GetEffective("offer/fee-currency-conversion-method", "Buy to middle").Result;
                    //finR.CalculateOffer(application, _priceCalculator, conversionMethod);
                }
                catch (MaxNumberOfIterationsException e)
                {
                    _logger.LogError(e, "Maximanl number of iterations exceeded while calculating price");
                    throw e;
                }
                catch (Exception exp)
                {
                    _logger.LogError(exp, "An unknown error occured while performing offer calculation");
                    throw new InvalidCalculationException();
                }
            }
        }

        private async Task<List<ArrangementRequest>> AddAbstractProduct(OfferApplication application,
            BundledProductInfo bundleInfo, int numberOfInstances)
        {
            // If abstract product contains product which is singleton and is already added earlier before,
            // it will be skipped but not some other product will be added instead of it in order to fulfill number of instances

            // Get product data
            var productData = await _productService.GetProductData(bundleInfo.ProductCode, "documentation", application.CustomerNumber);

            if (productData.Kind != ProductKinds.AbstractProduct)
            {
                return null;
            }
            var response = new List<ArrangementRequest>();
            var variants = productData.Variants?.Split(",") ?? new string[0];

            var instancesToCreate = Math.Min(numberOfInstances, variants.Count());
            var parameters = new ArrangementRequestInitializationParameters
            {
                IsAbstractOrigin = true
            };
            int i;
            for (i = 0; i < instancesToCreate; i++)
            {
                var resolved = await AddBundleComponentToApplication(application, variants[i], bundleInfo, bundleInfo.ProductCode, parameters);
                if (resolved != null)
                {
                    response.AddRange(resolved);
                }
            }
            if (variants.Count() > i)
            {
                for (; i < variants.Count(); i++)
                {
                    var resolved = await AddBundleComponentToApplication(application, variants[i], bundleInfo, bundleInfo.ProductCode, parameters);
                    if (resolved != null)
                    {
                        foreach (var item in resolved)
                        {
                            item.Enabled = false;
                        }
                    }
                    if (resolved != null)
                    {
                        response.AddRange(resolved);
                    }
                }
            }
            return response;
        }
    }
}
