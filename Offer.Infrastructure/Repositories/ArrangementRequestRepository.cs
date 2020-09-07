using Offer.Domain.AggregatesModel.ApplicationAggregate;
using System;
using System.Collections.Generic;
using MicroserviceCommon.Domain.SeedWork;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using AssecoCurrencyConvertion;
using System.Globalization;
using MicroserviceCommon.Services;
using Offer.Domain.Utils;
using Offer.Domain.AggregatesModel.ApplicationAggregate.AlternativeOfferModel;
using System.Threading.Tasks;
using AutoMapper;
using Offer.Domain.Calculations;
using MicroserviceCommon.Extensions.Broker;
using Asseco.EventBus.Abstractions;
using Microsoft.Extensions.Logging;
using Offer.Domain.AggregatesModel.ApplicationAggregate.ArrangementRequestModel;
using PriceCalculation.Calculations;
using Newtonsoft.Json.Linq;
using Offer.Domain.AggregatesModel.ApplicationAggregate.Interfaces;

namespace Offer.Infrastructure.Repositories
{
    public class ArrangementRequestRepository : IArrangementRequestRepository
    {
        private readonly OfferDBContext _context;
        private readonly IConfigurationService _configurationService;
        private readonly ArrangementRequestFactory _requestFactory;
        //private readonly IAuditClient _auditClient;
        private readonly IApplicationRepository _applicationRepository;
        private readonly ApplicationDocumentsResolver _documentsResolver;
        private readonly OfferPriceCalculation _priceCalculator;
        private readonly MessageEventFactory _messageEventFactory;
        private readonly IEventBus _bus;
        private readonly ILogger<ArrangementRequestRepository> _logger;
        private readonly IProductSnapshotRepository _productSnapshotRepository;

        public ArrangementRequestRepository(OfferDBContext context, IConfigurationService configurationService,
            ApplicationDocumentsResolver documentsResolver, OfferPriceCalculation priceCalculator,
            MessageEventFactory messageEventFactory, IEventBus bus, ILogger<ArrangementRequestRepository> logger,
            IApplicationRepository applicationRepository,
            ArrangementRequestFactory requestFactory,
            IProductSnapshotRepository productSnapshotRepository)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _configurationService = configurationService ?? throw new ArgumentNullException(nameof(configurationService));
            _documentsResolver = documentsResolver ?? throw new ArgumentNullException(nameof(documentsResolver));
            _priceCalculator = priceCalculator ?? throw new ArgumentNullException(nameof(priceCalculator));
            _messageEventFactory = messageEventFactory ?? throw new ArgumentNullException(nameof(messageEventFactory));
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _requestFactory = requestFactory ?? throw new ArgumentNullException(nameof(requestFactory));
            //_auditClient = auditClient ?? throw new ArgumentNullException(nameof(auditClient));
            _applicationRepository = applicationRepository ?? throw new ArgumentNullException(nameof(applicationRepository));
            _productSnapshotRepository = productSnapshotRepository ?? throw new ArgumentNullException(nameof(productSnapshotRepository));
        }

        public IUnitOfWork UnitOfWork
        {
            get
            {
                return _context;
            }
        }
        public ArrangementRequest GetArrangementRequest(long applicationNumber, int arrangementRequestId, string include, string trim)
        {
            string[] inclusions = string.IsNullOrEmpty(include) ? new string[] { } : include.Split(',');

            var application = _applicationRepository.GetAsync(applicationNumber).Result;
            if (application == null)
            {
                return null;
            }

            var queryable = _context.ArrangementRequests.Include(x => x.ProductSnapshotDb)
                .Where(d => d.ApplicationId == applicationNumber
                         && d.ArrangementRequestId == arrangementRequestId).AsQueryable();

            if (inclusions.Contains("collateral-requirements"))
            {
                queryable = queryable.Include(x => (x as FinanceServiceArrangementRequest).CollateralRequirements);
            }

            ArrangementRequest request = queryable.FirstOrDefault();
            if (request == null)
            {
                return null;
            }
            if (request is TermLoanRequest termLoan)
            {
                LoadDisbursementInfoAmount(termLoan);
            }
            return request;
        }


        private IQueryable<ArrangementRequest> GetArrangementRequestsQuery(long applicationId, bool includePotential = false)
        {
            var application = _applicationRepository.GetAsync(applicationId).Result;
            if (application == null)
            {
                return null;
            }
            IQueryable<ArrangementRequest> arrangementRequestsQuery = _context.ArrangementRequests.Include(ar => (ar as ArrangementRequest).ProductSnapshotDb).Where(
                r => r.ApplicationId == applicationId &&
                (includePotential ? true : (r.Enabled ?? false))
            );
            return arrangementRequestsQuery;
        }

        public ArrangementRequest GetArrangementRequest(long applicationNumber, int arrangementRequestId)
        {
            return GetArrangementRequest(applicationNumber, arrangementRequestId, "product-snapshot,installment-plan", null);
        }

        public List<ArrangementRequest> GetArrangementRequests(long applicationNumber, string include, string trim,
            bool includePotential = false)
        {
            var application = _applicationRepository.GetAsync(applicationNumber).Result;
            if (application == null)
            {
                return null;
            }
            string[] inclusions = string.IsNullOrEmpty(include) ? new string[] { } : include.Split(',');
            var queryArrRequests = GetArrangementRequestsQuery(applicationNumber, includePotential);
            if (inclusions.Contains("collateral-requirements"))
            {
                queryArrRequests = queryArrRequests.Include(x => (x as FinanceServiceArrangementRequest).CollateralRequirements);
            }
            List<ArrangementRequest> arrRequests = queryArrRequests.AsNoTracking().ToList();
            if (!inclusions.Contains("installment-plan"))
            {
                arrRequests.ForEach(x => x.InstallmentPlan = null);
            }
            if (!inclusions.Contains("product-snapshot"))
            {
                arrRequests.ForEach(x => x.ProductSnapshot = null);
            }
            foreach (ArrangementRequest request in arrRequests)
            {
                if (request is TermLoanRequest termLoan)
                {
                    LoadDisbursementInfoAmount(termLoan);
                }
            }
            return arrRequests;
        }

        public bool AddArrangementRequest(ArrangementRequest request)
        {
            _context.Add(request);
            return true;
        }


        public void LoadDisbursementInfoAmount(TermLoanRequest termLoanRequest)
        {
            if (termLoanRequest.DisbursementsInfo != null && termLoanRequest.DisbursementsInfo.Count > 0)
            {
                decimal totalInvoiceAmount = 0;
                CurrencyConverter currencyConverter = new CurrencyConverter();
                var conversionMethod = _configurationService.GetEffective("offer/fee-currency-conversion-method", "Buy to middle").Result;
                var domesticCurrency = _configurationService.GetEffective("domestic-currency", "RSD").Result;
                foreach (var loanPurpose in termLoanRequest.DisbursementsInfo)
                {
                    var amount = currencyConverter.CurrencyConvert(loanPurpose.Amount.Amount, loanPurpose.Amount.Code,
                               domesticCurrency, DateTime.Today.ToString("o", CultureInfo.InvariantCulture), conversionMethod);
                    totalInvoiceAmount += amount;
                }
                termLoanRequest.TotalDisbursementAmount = totalInvoiceAmount;
            }
        }

        public bool? DeleteArrangementRequest(long applicationNumber, int arrangementRequestId)
        {

            var application = _applicationRepository.GetAsync(applicationNumber).Result;
            if (application == null)
            {
                return null;
            }
            var arrangementRequestQuery = _context.ArrangementRequests.Include(ar => ar.ProductSnapshotDb).Where(a => a.ApplicationId.Equals(applicationNumber) &&
                                                     a.ArrangementRequestId.Equals(arrangementRequestId)).AsQueryable();
            var arrangementRequest = arrangementRequestQuery.FirstOrDefault();
            if (arrangementRequest == null)
            {
                return false;
            }
            arrangementRequest.Enabled = false;
            _context.SaveChanges();
            var app = _context.Applications
                .Include(a => a.Documents)
                .Include(a => a.InvolvedParties)
                .Include(a => a.ArrangementRequests).ThenInclude(ar => (ar as ArrangementRequest).ProductSnapshotDb)
                .FirstOrDefault(a => a.ApplicationId == applicationNumber);
            var x = _documentsResolver.ResolveDocuments(app).Result;
            _context.SaveChanges();
            return _documentsResolver.CreateApplicationDocumentsFolders().Result;
        }

        public async Task<ArrangementRequest> UpdateArrangementRequest(ArrangementRequest arrangementRequest)
        {
            if (arrangementRequest != null)
            {
                if (long.TryParse(arrangementRequest.ApplicationNumber, out long appNumberLong))
                {
                    var request = GetArrangementRequest(appNumberLong, arrangementRequest.ArrangementRequestId, null, null);
                    if (request == null)
                    {
                        return null;
                    }
                    if (arrangementRequest is FinanceServiceArrangementRequest finArrRequest)
                    {
                        var app = _context.Applications.First(a => a.ApplicationNumber.Equals(arrangementRequest.ApplicationNumber));
                        if (app.ProductCode.Equals(arrangementRequest.ProductCode))
                        {
                            app.LoanToValue = finArrRequest.LoanToValue;
                            app.MaximalAmount = finArrRequest.MaximalAmount;
                            app.MaximalAnnuity = finArrRequest.MaximalAnnuity;
                            app.AmountLimitBreached = finArrRequest.Amount > arrangementRequest.ProductSnapshot.MaximalAmount.Amount ? true : false;
                        }
                    }
                    arrangementRequest.ProductSnapshotDb = await _productSnapshotRepository.PostProductSnapshot(arrangementRequest.ProductSnapshot);
                    _context.Entry(request).CurrentValues.SetValues(arrangementRequest);
                    _context.SaveChanges();
                    return arrangementRequest;
                }
            }
            return null;
        }

        public CollateralRequirement AddCollateralRequirement(CollateralRequirement req)
        {
            var applicationNumber = long.Parse(req.ApplicationNumber);
            var arrangementRequest = GetArrangementRequest(applicationNumber, req.ArrangementRequestId);
            if (arrangementRequest == null)
            {
                return null;
            }
            if (arrangementRequest is FinanceServiceArrangementRequest ara)
            {
                if (ara.CollateralRequirements == null)
                {
                    ara.CollateralRequirements = new List<CollateralRequirement>();
                }
                ara.CollateralRequirements.Add(req);
                var result = _context.SaveChanges();
                if (result > 0)
                {
                    var app = _context.Applications
                        .Include(a => a.ArrangementRequests)
                        .Include(a => a.InvolvedParties)
                        .Include(a => a.Documents)
                        .FirstOrDefault(a => a.ApplicationId == applicationNumber);
                    var x = _documentsResolver.ResolveDocuments(app).Result;
                    result = _context.SaveChanges();
                    var y = _documentsResolver.CreateApplicationDocumentsFolders().Result;
                    return (result > 0) ? req : null;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        public List<CollateralRequirement> GetCollateralRequirementsForArrangementRequest(long applicationNumber, int arrangementRequestId)
        {
            var application = _applicationRepository.GetAsync(applicationNumber).Result;
            if (application == null)
            {
                return null;
            }
            var query = _context.CollateralRequirements.Where(x => x.ApplicationId.Equals(applicationNumber) && x.ArrangementRequestId.Equals(arrangementRequestId));
            return query.ToList();
        }

        public CollateralRequirement GetCollateralRequirementById(long applicationNumber, int arrangementRequestId, long collateralRequirementId)
        {
            var application = _applicationRepository.GetAsync(applicationNumber).Result;
            if (application == null)
            {
                return null;
            }
            var query = _context.CollateralRequirements.Where(x => x.CollateralRequirementId.Equals(collateralRequirementId)
                                                           && x.ApplicationId.Equals(applicationNumber)
                                                           && x.ArrangementRequestId.Equals(arrangementRequestId));
            return query.FirstOrDefault();
        }

        public CollateralRequirement UpdateCollateralRequirement(CollateralRequirement requirement)
        {
            _logger.LogInformation("Updating collateral requirement {CollateralRequirementId} for application {ApplicationNumber}",
                requirement.CollateralRequirementId, requirement.ApplicationNumber);
            _logger.LogInformation("Current actual coverage for collateral requirement {CollateralRequirementId} for application {ApplicationNumber}" +
                "is {ActualCoverage}", requirement.CollateralRequirementId, requirement.ApplicationNumber, requirement.ActualCoverage);
            requirement.ActualCoverage = requirement.SecuredDealLinks.Sum(deal => deal.PledgedValueInLoanCurrency); ;
            _logger.LogInformation("New actual coverage for collateral requirement {CollateralRequirementId} for application {ApplicationNumber}" +
                "is {ActualCoverage}", requirement.CollateralRequirementId, requirement.ApplicationNumber, requirement.ActualCoverage);
            return requirement;
        }

        public async Task<bool?> DeleteCollateralRequirement(long applicationNumber, int arrangementRequestId, long collateralRequirementId)
        {
            var collateralRequirement = GetCollateralRequirementById(applicationNumber, arrangementRequestId, collateralRequirementId);
            if (collateralRequirement == null)
            {
                return null;
            }
            if (collateralRequirement != null && !collateralRequirement.FromModel)
            {
                var arrangementRequest = GetArrangementRequest(applicationNumber, arrangementRequestId, "collateral-requirements", null);
                if (arrangementRequest is FinanceServiceArrangementRequest finRequest)
                {
                    finRequest.CollateralRequirements.Remove(collateralRequirement);
                }
                collateralRequirement.ActualCoverage = collateralRequirement.SecuredDealLinks.Sum(deal => deal.PledgedValueInLoanCurrency);
                _context.SaveChanges();

                // try
                // {
                //     await _auditClient.WriteLogEntry(AuditLogEntryAction.Delete, AuditLogEntryStatus.Success, "collateral", applicationNumber.ToString(), "Collateral deleted", collateralRequirement);
                // }
                // catch (Exception ex)
                // {
                //     _logger.LogError(ex, "Audit error in ArrangementRequestRepository");
                // }

                var app = _context.Applications
                        .Include(a => a.ArrangementRequests)
                        .Include(a => a.InvolvedParties)
                        .Include(a => a.Documents)
                        .FirstOrDefault(a => a.ApplicationId == applicationNumber);
                var x = _documentsResolver.ResolveDocuments(app).Result;
                _context.SaveChanges();
                var y = _documentsResolver.CreateApplicationDocumentsFolders().Result;
                return true;
            }
            return false;
        }

        public async Task<ArrangementRequest> SetApprovedLimits(long applicationNumber, int arrangementRequestId,
            ApprovedLimits command, Application application)
        {
            var arrRequest = GetArrangementRequest(applicationNumber, arrangementRequestId);
            if (arrRequest == null)
            {
                return null;
            }

            //set requested parameters
            if (arrRequest.ApprovedLimits is null)
            {
                RequestedValues requestedValues = new RequestedValues();

                if (arrRequest is TermLoanRequest termLoan)
                {
                    requestedValues.Annuity = termLoan.Annuity;

                    if (command.InvoiceAmount != null)
                    {
                        requestedValues.DownpaymentAmount = termLoan.DownpaymentAmount;
                        requestedValues.DownpaymentPercentage = termLoan.DownpaymentPercentage.Value;
                        requestedValues.InvoiceAmount = termLoan.InvoiceAmount;
                    }

                }
                if (arrRequest is FinanceServiceArrangementRequest fsaData)
                {
                    requestedValues.Amount = fsaData.Amount;
                    requestedValues.Term = Utility.GetMonthsFromPeriod(fsaData.Term).ToString();
                    requestedValues.Napr = fsaData.Napr;
                }

                arrRequest.RequestedValues = requestedValues;
            }

            //set initial parameteres
            if (arrRequest is TermLoanRequest tlr)
            {
                tlr.Annuity = command.Annuity;
                if (command.InvoiceAmount != null)
                {
                    tlr.InvoiceAmount = command.InvoiceAmount.Value;
                    tlr.DownpaymentPercentage = command.DownpaymentPercentage.Value;
                    tlr.DownpaymentAmount = command.DownpaymentAmount.Value;
                }
            }

            if (arrRequest is FinanceServiceArrangementRequest fsa)
            {

                fsa.Amount = command.Amount;
                fsa.Term = Utility.GetMonthsFromPeriod(command.Term).ToString();
            }

            arrRequest.ApprovedLimits = command;
            var conversionMethod = _configurationService.GetEffective("offer/fee-currency-conversion-method", "Buy to middle").Result;
            arrRequest.CalculateOffer(application, _priceCalculator, conversionMethod);
            await _context.SaveEntitiesAsync();
            return arrRequest;

        }

        public async Task<ArrangementRequest> SetAcceptedValues(long applicationNumber, int arrangementRequestId,
            AcceptedValues command, Application application)
        {
            var arrRequest = _context.ArrangementRequests.Include(ar => ar.ProductSnapshotDb).Where(p => p.ApplicationId == applicationNumber &&
                p.ArrangementRequestId == arrangementRequestId).FirstOrDefault();
            if (arrRequest == null)
            {
                throw new KeyNotFoundException("Requested arrangement does not exist for provided application.");
            }

            AcceptedValues acceptedValues = new AcceptedValues();

            if (arrRequest is TermLoanRequest tlr)
            {
                acceptedValues.Annuity = command.Annuity;
                tlr.Annuity = command.Annuity;
                if (command.DownpaymentAmount != null)
                {
                    tlr.DownpaymentAmount = command.DownpaymentAmount.Value;
                    tlr.DownpaymentPercentage = command.DownpaymentPercentage.Value;
                    tlr.InvoiceAmount = command.InvoiceAmount.Value;
                }
            }
            if (arrRequest is FinanceServiceArrangementRequest fsa)
            {
                acceptedValues.Amount = command.Amount;
                acceptedValues.Term = Utility.GetMonthsFromPeriod(command.Term).ToString();
                acceptedValues.Napr = command.Napr;
                if (command.DownpaymentAmount != null)
                {
                    acceptedValues.DownpaymentAmount = command.DownpaymentAmount.Value;
                    acceptedValues.DownpaymentPercentage = command.DownpaymentPercentage.Value;
                    acceptedValues.InvoiceAmount = command.InvoiceAmount.Value;
                }
                fsa.Amount = command.Amount;
                fsa.Term = Utility.GetMonthsFromPeriod(command.Term).ToString();
            }

            arrRequest.AcceptedValues = acceptedValues;

            var conversionMethod = _configurationService.GetEffective("offer/fee-currency-conversion-method", "Buy to middle").Result;
            arrRequest.CalculateOffer(application, _priceCalculator, conversionMethod);

            var res = await _context.SaveEntitiesAsync();

            var messageObj = _messageEventFactory.CreateBuilder("offer", "accepted-values-setted").Build();
            _bus.Publish(messageObj);

            return arrRequest;

        }

        public List<CollateralRequirementValidation> ValidateCollateralRequirement(long applicationNumber, int arrangementRequestId)
        {
            List<CollateralRequirementValidation> collValidationList = new List<CollateralRequirementValidation>();
            ArrangementRequest arrRequest = GetArrangementRequest(applicationNumber, arrangementRequestId, "collateral-requirements", null);
            if (arrRequest == null || !(arrRequest is FinanceServiceArrangementRequest))
            {
                return null;
            }
            var collReq = ((FinanceServiceArrangementRequest)arrRequest).CollateralRequirements;

            foreach (var item in collReq)
            {
                CollateralRequirementValidation collValidation = new CollateralRequirementValidation
                {
                    ApplicationNumber = applicationNumber,
                    ArrangementRequestId = arrangementRequestId,
                    CollateralArrangementCode = item.CollateralArrangementCode,
                    CollateralRequirementId = item.CollateralRequirementId
                };
                if (item.SecuredDealLinks == null)
                {
                    collValidation.ValidationResult = CollateralValidationResult.NotFilled;

                }
                else
                {
                    if (item.MinimalCoverage > 0)
                    {
                        if (item.MinimalCoverageInLoanCurrency > item.ActualCoverage)
                        {
                            collValidation.ValidationResult = CollateralValidationResult.BelowMinimalCoverage;
                        }
                        else
                        {
                            collValidation.ValidationResult = CollateralValidationResult.Filled;
                        }
                    }
                    else
                    {
                        collValidation.ValidationResult = CollateralValidationResult.Filled;
                    }
                }
                collValidationList.Add(collValidation);
            }


            return collValidationList;
        }

        public List<ArrangementRequest> GetBundledRequests(ArrangementRequest arrangementRequest, bool includeSingletons = false)
        {
            var application = _applicationRepository.GetAsync(arrangementRequest.ApplicationId).Result;
            if (application == null)
            {
                return null;
            }
            var bundledRequests = GetArrangementRequestsQuery(arrangementRequest.ApplicationId, true)
                .Where(r =>
                    r.ArrangementRequestId != arrangementRequest.ArrangementRequestId &&
                    r.ParentProductCode == arrangementRequest.ProductCode &&
                    r.ProductSnapshot.IsSingleton == includeSingletons)
                .ToList();

            return bundledRequests;
        }

        public async Task<bool?> DeleteArrangementRequests(long applicationNumber, List<ArrangementRequest> arrangementRequests)
        {
            var application = _applicationRepository.GetAsync(applicationNumber).Result;
            if (application == null)
            {
                return null;
            }
            var disabledIds = arrangementRequests.Select(r => r.ArrangementRequestId).ToList();
            var requestsToDisable = GetArrangementRequestsQuery(applicationNumber, true)
                .Where(r =>
                    disabledIds.Contains(r.ArrangementRequestId))
                .ToList();
            requestsToDisable.ForEach(r =>
            {
                r.Enabled = false;
            });
            _context.ArrangementRequests.UpdateRange(requestsToDisable);

            await _context.SaveChangesAsync();
            var app = _context.Applications
                .Include(a => a.Documents)
                .Include(a => a.InvolvedParties)
                .Include(a => a.ArrangementRequests)
                .FirstOrDefault(a => a.ApplicationId == applicationNumber);
            var x = _documentsResolver.ResolveDocuments(app).Result;
            await _context.SaveChangesAsync();
            return await _documentsResolver.CreateApplicationDocumentsFolders();
        }

        public List<ArrangementRequestValidation> ValidateArrangementRequests(long applicationId)
        {
            var application = _applicationRepository.GetAsync(applicationId).Result;
            if (application == null)
            {
                return null;
            }
            var arrangementRequests = GetArrangementRequestsQuery(applicationId, true).ToList();
            if (arrangementRequests.Count == 0)
            {
                return new List<ArrangementRequestValidation>();
            }
            var bundledProducts = GetBundledComponentsForApplication(applicationId)
                .ToDictionary(x => x.ProductCode, y => y);

            return arrangementRequests.Where(r => !string.IsNullOrEmpty(r.ParentProductCode))
                .GroupBy(k => k.ProductCode)
                .Select(k =>
                {
                    if (!bundledProducts.TryGetValue(k.Key, out BundleComponentInfo bundledInfo))
                    {
                        bundledInfo = bundledProducts[k.Select(p => p.ParentProductCode).FirstOrDefault()];
                    }
                    var parentCode = k.Select(p => p.ParentProductCode).FirstOrDefault();
                    var parentCount = string.IsNullOrEmpty(parentCode) ? 0 :
                            arrangementRequests.Count(pc =>
                                !string.IsNullOrEmpty(pc.ParentProductCode) &&
                                pc.ParentProductCode.Equals(parentCode) &&
                                (pc.Enabled ?? false));
                    var count = k.Where(r => r.Enabled ?? false).Count();
                    var maxCount = Math.Max(count, parentCount);
                    return new ArrangementRequestValidation
                    {
                        ProductCode = k.Key,
                        ProductName = k.Select(p => p.ProductName).FirstOrDefault(),
                        ProductSnapshot = k.Select(p => p.ProductSnapshot).FirstOrDefault(),
                        Count = count,
                        ParentCount = parentCount,
                        MinimalNumberOfInstances = bundledInfo.MinimalNumberOfInstances,
                        MaximalNumberOfInstances = bundledInfo.MaximalNumberOfInstances,
                        IsValid = maxCount >= bundledInfo.MinimalNumberOfInstances &&
                            maxCount <= bundledInfo.MaximalNumberOfInstances,
                        Kind = k.Select(p => p.ArrangementKind).FirstOrDefault() ?? ArrangementKind.Abstract,
                        IsAbstractOrigin = k.Any(a => a.IsAbstractOrigin),
                        Enabled = k.Select(p => p.Enabled ?? false).FirstOrDefault(),
                        ArrangementRequests = k.ToList()
                    };
                })
                .ToList();
        }

        public async Task<List<ArrangementRequest>> GetArrangementRequestsByProductCode(long applicationNumber, string productCode, string include = null, string trim = null)
        {
            var application = _applicationRepository.GetAsync(applicationNumber).Result;
            if (application == null)
            {
                return null;
            }
            string[] inclusions = string.IsNullOrEmpty(include) ? new string[] { } : include.Split(',');

            var queryable = _context.ArrangementRequests.Include(ar => ar.ProductSnapshotDb)
                .Where(d => d.ApplicationId == applicationNumber && d.ProductCode == productCode).AsQueryable();
            if (inclusions.Contains("collateral-requirements"))
            {
                queryable = queryable.Include(x => (x as FinanceServiceArrangementRequest).CollateralRequirements);
            }
            if (inclusions.Contains("product-snapshot"))
            {
                queryable = queryable.Include(x => x.ProductSnapshot);
            }
            return await queryable.ToListAsync();
        }

        public List<BundleComponentInfo> GetBundledComponentsForApplication(long applicationNumber)
        {
            var application = _applicationRepository.GetAsync(applicationNumber).Result;
            if (application == null)
            {
                return null;
            }
            var arrangementRequests = GetArrangementRequestsQuery(applicationNumber, true)
                .ToList();
            return arrangementRequests
                .Where(r => r.ProductSnapshot.BundledProducts != null)
                .SelectMany(r => r.ProductSnapshot.BundledProducts)
                .GroupBy(b => b.ProductCode)
                .Select(p => new BundleComponentInfo
                {
                    ProductCode = p.Key,
                    ProductName = p.Select(n => n.ProductName).FirstOrDefault(),
                    MinimalNumberOfInstances = p.Max(m => m.MinimalNumberOfInstances),
                    MaximalNumberOfInstances = p.Min(m => m.MaximalNumberOfInstances),
                    Kind = Helpers.GetArrangmentKindByProductKind(p.Select(n => n.ProductKind).FirstOrDefault()).Value
                })
                .ToList();
        }

        public async Task<bool> UpdateArrangementRequestsAvailability(Application application, List<ArrangementRequestsAvailability> arrangementRequestUpserts)
        {
            var enabledIds = arrangementRequestUpserts.Where(u => u.Enabled).Select(u => u.ArrangementRequestId).ToList();
            var disabledIds = arrangementRequestUpserts.Where(u => !u.Enabled).Select(u => u.ArrangementRequestId).ToList();
            var requestsToEnable = _context.ArrangementRequests.Include(ar => ar.ProductSnapshotDb)
                .Where(r =>
                    r.ApplicationId == application.ApplicationId &&
                    enabledIds.Contains(r.ArrangementRequestId))
                .ToList();
            requestsToEnable.ForEach(r =>
            {
                r.Enabled = true;
            });
            _context.ArrangementRequests.UpdateRange(requestsToEnable);
            var requestsToDisable = _context.ArrangementRequests
                .Where(r =>
                    r.ApplicationId == application.ApplicationId &&
                    disabledIds.Contains(r.ArrangementRequestId))
                .ToList();
            requestsToDisable.ForEach(r =>
            {
                r.Enabled = false;
            });
            _context.ArrangementRequests.UpdateRange(requestsToDisable);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AddArrangementRequestsToApplication(Application application, List<ArrangementRequest> arrangementRequests)
        {
                try
                {
                    foreach (var item in arrangementRequests)
                    {
                        var parameters = Mapper.Map<ArrangementRequest, ArrangementRequestInitializationParameters>(item);
                        var requests = await _requestFactory.AddToApplication(application, item.ProductCode, parameters);
                    }

                    await _context.SaveChangesAsync();
                    await _documentsResolver.ResolveDocuments(application);
                    await _context.SaveChangesAsync();
                    return await _documentsResolver.CreateApplicationDocumentsFolders();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "AddArrangementRequestsToApplication error");
                    throw ex;
                }
    
        }

        public List<ArrangementRequest> GetAvailableProducts(long applicationId)
        {
            return null;
        }

        public async Task<bool?> SetArragementRequestAvailability(long applicationId, int arrangementRequestId, bool enabled)
        {
            ArrangementRequest arrRequest = GetArrangementRequest(applicationId, arrangementRequestId, null, null);
            if (arrRequest == null)
            {
                return null;
            }
            arrRequest.Enabled = enabled;
            return true;
        }

        public async Task<bool?> SetCreditLineUsers(long applicationNumber, int arrangementRequestId,
           CreditLineLimits command, Application application)
        {
            var arrRequest = _context.ArrangementRequests.Include(ar => ar.ProductSnapshotDb).Where(p => p.ApplicationId == applicationNumber &&
                p.ArrangementRequestId == arrangementRequestId).FirstOrDefault();
            if (arrRequest == null)
            {
                return null;
            }

            if (arrRequest is CreditLineRequest tlr)
            {
                if (tlr.CreditLineLimits == null)
                {
                    tlr.CreditLineLimits = new CreditLineLimits();

                }
                tlr.CreditLineLimits.AllowedRevolvingLineUsers = command.AllowedRevolvingLineUsers;
            }

            await _context.SaveEntitiesAsync();


            return true;

        }

        public async Task<bool?> SetCreditLineProductCodes(long applicationNumber, int arrangementRequestId,
          CreditLineLimits command, Application application)
        {
            var arrRequest = _context.ArrangementRequests.Include(ar => ar.ProductSnapshotDb).Where(p => p.ApplicationId == applicationNumber &&
                p.ArrangementRequestId == arrangementRequestId).FirstOrDefault();
            if (arrRequest == null)
            {
                return null;
            }

            if (arrRequest is CreditLineRequest tlr)
            {
                CreditLineLimits cll = new CreditLineLimits
                {
                    AllowedRevolvingLineUsers = tlr.CreditLineLimits.AllowedRevolvingLineUsers,
                    ProductCodes = command.ProductCodes
                };

                tlr.CreditLineLimits = new CreditLineLimits();
                tlr.CreditLineLimits = cll;
            }

            await _context.SaveEntitiesAsync();
            return true;
            // return arrRequest;

        }

        public async Task<bool?> SetCreditLineProductKinds(long applicationNumber, int arrangementRequestId,
        CreditLineLimits command, Application application)
        {
            var arrRequest = _context.ArrangementRequests.Include(ar => ar.ProductSnapshotDb).Where(p => p.ApplicationId == applicationNumber &&
                p.ArrangementRequestId == arrangementRequestId).FirstOrDefault();
            if (arrRequest == null)
            {
                return null;
            }

            if (arrRequest is CreditLineRequest tlr)
            {

                CreditLineLimits cll = new CreditLineLimits
                {
                    AllowedRevolvingLineUsers = tlr.CreditLineLimits.AllowedRevolvingLineUsers,
                    ProductKinds = command.ProductKinds
                };

                tlr.CreditLineLimits = new CreditLineLimits();
                tlr.CreditLineLimits = cll;

            }

            await _context.SaveEntitiesAsync();

            return true;

        }

        public IDictionary<string, IDictionary<string, JToken>> GetExtendedArrangementData(long applicationNumber, int arrangementRequestId)
        {
            var arrangementRequest = GetArrangementRequest(applicationNumber, arrangementRequestId);

            if (arrangementRequest == null)
            {
                return null;
            }

            if (arrangementRequest.Extended == null)
            {
                return new Dictionary<string, IDictionary<string, JToken>>();
            }

            return arrangementRequest.Extended;
        }

        public IDictionary<string, JToken> GetExtendedArrangementDataSection(long applicationNumber, int arrangementRequestId, string sectionName)
        {
            var arrangementRequest = GetArrangementRequest(applicationNumber, arrangementRequestId);

            if (arrangementRequest == null)
            {
                return null;
            }

            if (arrangementRequest.Extended == null || !arrangementRequest.Extended.ContainsKey(sectionName))
            {
                return new Dictionary<string, JToken>();
            }
            return arrangementRequest.Extended[sectionName];
        }

        public async Task<bool?> DeleteExtendedDataSection(long applicationNumber, int arrangementRequestId, string sectionName)
        {
            var arrangementRequest = GetArrangementRequest(applicationNumber, arrangementRequestId);
            if (arrangementRequest == null)
            {
                return null;
            }
            if (arrangementRequest.Extended == null || !arrangementRequest.Extended.ContainsKey(sectionName))
            {
                return null;
            }
            arrangementRequest.Extended.Remove(sectionName);
            await _context.SaveEntitiesAsync();
            return true;
        }
    }
}
