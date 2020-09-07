using MediatR;
using MicroserviceCommon.ApiUtil;
using MicroserviceCommon.Application.Commands;
using MicroserviceCommon.Infrastructure.Idempotency;
using MicroserviceCommon.Models;
using MicroserviceCommon.Models.Product;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Offer.API.Extensions;
using Offer.Domain.AggregatesModel.ApplicationAggregate;
using Offer.Domain.AggregatesModel.ApplicationAggregate.ProductModel;
using PriceCalculation.Models.Lifecycle;
using PriceCalculation.Models.Product;
using PriceCalculation.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Offer.API.Application.Commands
{
    #region ProductData class
    public partial class ProductData
    {
        public string ProductCode { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public ProductKinds Kind { get; set; }
        public ProductStatus Status { get; set; }
        public string MarketFeatures { get; set; }
        public string BenefitsInfo { get; set; }
        public string FamilyName { get; set; }
        public string PrimaryMarketSegmentName { get; set; }
        public bool IsPackage { get; set; }
        public string ImageUrl { get; set; }
        public string CoverImageUrl { get; set; }
        public string ApplicationProcessDescription { get; set; }
        public string CampaignCode { get; set; }
        public DateTime AvailabilityStart { get; set; }
        public string ProposalValidityPeriod { get; set; }
        public string OfferValidityPeriod { get; set; }
        public string PrimaryCurrency { get; set; }
        public List<string> AllowedCurrencies { get; set; }
        public Currency MaximalAmount { get; set; }
        public Currency MinimalAmount { get; set; }
        public bool IsRecommended { get; set; }
        public bool IsPreApproved { get; set; }
        public int InUse { get; set; }
        public bool IsStandalone { get; set; }
        public bool IsSingleton { get; set; }
        public int PreviouslyUsed { get; set; }
        public Dictionary<string, bool> BundleDefaults { get; set; }
        public int ActiveRequests { get; set; }
        public string AccountTypeMapping { get; set; }
        public string SyncTimestamp { get; set; }
        public string TargetSegments { get; set; }
        public string TargetCustomerResidency { get; set; }
        public string TargetCustomerKind { get; set; }
        public string ChannelAvailability { get; set; }
        public string AvailableCollateralModels { get; set; }
        public string DefaultCollateralModel { get; set; }
        public string AvailableDiscountValues { get; set; }
        public bool SupportsAlternativeOffer { get; set; }
        public DefaultParameters DefaultParameters { get; set; }
        public List<SchedulingPeriod> Periods { get; set; }

        public List<OptionGroup> OptionGroups { get; set; }
        public string LoanPurposes { get; set; }
        public string Refinancing { get; set; }
        public string AvailableRevolvingPercentage { get; set; }
        public bool DisbursementInfoEntry { get; set; }
        public string Variants { get; set; }

        public List<BundledProductInfo> BundledProducts { get; set; }
        public string RelatedProducts { get; set; }
        public List<ProductDocumentation> RequiredDocumentation { get; set; }
        public List<ProductCollateralModel> AvailableCollateralModelsData { get; set; }
        

        //public CardKinds CardKind { get; set; }
        public string CardBrand { get; set; }
        public Currency DailyLimit { get; set; }
        public int DailyTransactionsLimit { get; set; }
        // public List<ProductAccessServiceChannel> CardAccessProductChannels { get; set; }
        public string MinimalTerm { get; set; }
        public string MaximalTerm { get; set; }
        public Currency MinimalRepaymentAmount { get; set; }
        public decimal MinimalRepaymentPercentage { get; set; }
        public string FinancingKind { get; set; }
        public string ChannelCode { get; set; }
        public string ChannelName { get; set; }
        public bool IsDigital { get; set; }
        public bool IsNotification { get; set; }
        public double MinimalDownpaymentPercentage { get; set; }

        public bool HasGracePeriod { get; set; }
        public string MinimalGracePeriod { get; set; }
        public string MaximalGracePeriod { get; set; }

        public bool HasDrawdownPeriod { get; set; }
        public string MinimalDrawdownPeriod { get; set; }
        public string MaximalDrawdownPeriod { get; set; }

        public ProductConditions Conditions { get; set; }
        // public List<ProductData> RelatedProducts { get; set; }
        // public List<CollateralModel> AvailableCollateralModelsData { get; set; }
        public bool IsRevolving { get; set; }
        public CreditLineProducts CreditLineProducts { get; set; }
        public int MinimumDaysForFirstInstallment { get; set; }
        public string DueDayOptions { get; set; }
    }
    #endregion

    public class GetProductDataCommandHandler : IRequestHandler<GetProductDataCommand, ProductData>
    {

        private readonly IMediator _mediator;
        private readonly ApiEndPoints _apiEndPoints;
        private readonly ILogger<GetProductDataCommand> _logger;
        private readonly IPriceCalculationService _priceCalculation;
        private readonly IHttpClientFactory _httpClientFactory;

        public GetProductDataCommandHandler(IMediator mediator,
            ApiEndPoints apiEndPoints,
            ILogger<GetProductDataCommand> logger,
            IPriceCalculationService priceCalculation,
            IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _apiEndPoints = apiEndPoints ?? throw new ArgumentNullException(nameof(apiEndPoints));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _priceCalculation = priceCalculation ?? throw new ArgumentNullException(nameof(priceCalculation));
        }

        public async Task<ProductData> Handle(GetProductDataCommand message, CancellationToken cancellationToken)
        {
            using (var httpClient = _httpClientFactory.CreateClient())
            {
                httpClient.AddDefaultJsonHeaders();
                try
                {
                    string checkCustomerId = string.IsNullOrEmpty(message.CustomerId) ? "" : "&customer-id=" + message.CustomerId;
                    using (HttpResponseMessage response = await httpClient.GetAsync(_apiEndPoints.GetServiceUrl("product") + "products/" + message.ProductCode + "?include=available-collateral-models-data,bundled-products" + checkCustomerId))
                    {
                        var res = await response.Content.ReadAsStringAsync();
                        if (string.IsNullOrEmpty(res))
                        {
                            _logger.LogError("Could not fetch product data for product: {productCode}. Response is empty", message.ProductCode);
                            throw new Exception("Could not fetch Product");
                        }
                        var productData = (ProductData)CaseUtil.ConvertFromJsonToObject(res, typeof(ProductData));
                        if (string.IsNullOrEmpty(productData.ProductCode))
                        {
                            return null;
                        }
                        var conditions = await GetProductConditions(message.ProductCode);
                        productData.Conditions = await _priceCalculation.ReadVariationDefinitions(conditions);
                        if (productData.IsPackage)
                        {
                            productData.BundleDefaults = await GetProductBundleDefaults(productData.ProductCode);
                        }
                        if (message.IncludeArray.Contains("documentation"))
                        {
                            var getProductDocs = new IdentifiedCommand<GetProductDocumentationCommand, List<ProductDocumentation>>(
                                new GetProductDocumentationCommand { ProductCode = message.ProductCode, CustomerId = message.CustomerId }, new Guid());
                            productData.RequiredDocumentation = await _mediator.Send(getProductDocs);
                        }
                        return productData;
                    }
                }
                catch (HttpRequestException e)
                {
                    _logger.LogError(e, "Could not fetch product data for product: {productCode}", message.ProductCode);
                    throw e;
                }
            }
        }

        public async Task<Dictionary<string, bool>> GetProductBundleDefaults(string productCode)
        {
            using (var httpClient = _httpClientFactory.CreateClient())
            {
                httpClient.AddDefaultJsonHeaders();

                _logger.LogDebug("Product endpoint is: {productApiUrl}products/{{productCode}}/bundle-defaults", _apiEndPoints.GetServiceUrl("product"), productCode);

                try
                {
                    using (HttpResponseMessage response = await httpClient.GetAsync(_apiEndPoints.GetServiceUrl("product") + "products/" + productCode + "/bundle-defaults"))
                    {
                        var res = await response.Content.ReadAsStringAsync();
                        if (res.Equals(""))
                        {
                            _logger.LogError("Could not fetch bundle defaults for product {productCode}", productCode);
                            throw new Exception("Could not fetch Product bundle defaults");
                        }
                        Dictionary<string, bool> returnProducts = new Dictionary<string, bool>();
                        JArray array = (JArray)JsonConvert.DeserializeObject(res, typeof(JArray));
                        foreach (JToken token in array)
                        {
                            returnProducts.Add(token["product-code"].ToString(), token.Value<bool>("enabled"));
                        }
                        return returnProducts;
                    }
                }
                catch (HttpRequestException e)
                {
                    _logger.LogError(e, "Could not fetch product bundle defaults for product {productCode}", productCode);
                    throw e;
                }
            }
        }
        public async Task<ProductConditions> GetProductConditions(string productCode)
        {
            using (var httpClient = _httpClientFactory.CreateClient())
            {
                httpClient.AddDefaultJsonHeaders();

                _logger.LogDebug("Product endpoint is: {productApiUrl}products/{{productCode}}/conditions", _apiEndPoints.GetServiceUrl("product"), productCode);

                try
                {
                    using (HttpResponseMessage response = await httpClient.GetAsync(_apiEndPoints.GetServiceUrl("product") + "products/" + productCode + "/conditions"))
                    {
                        var res = await response.Content.ReadAsStringAsync();
                        if (res.Equals(""))
                        {
                            _logger.LogError("Could not fetch product conditions for product {productCode}", productCode);
                            throw new Exception("Could not fetch Product conditions");
                        }
                        return (ProductConditions)CaseUtil.ConvertFromJsonToObject(res, typeof(ProductConditions));
                    }
                }
                catch (HttpRequestException e)
                {
                    _logger.LogError(e, "Could not fetch product conditions for product {productCode}", productCode);
                    throw e;
                }
            }
        }
    }

    public class GetProductDataIdentifiedCommandHandler : IdentifiedCommandHandler<GetProductDataCommand, ProductData>
    {
        public GetProductDataIdentifiedCommandHandler(IMediator mediator, IRequestManager requestManager) : base(mediator, requestManager)
        {
        }

        protected override ProductData CreateResultForDuplicateRequest()
        {
            return null;
        }
    }
}
