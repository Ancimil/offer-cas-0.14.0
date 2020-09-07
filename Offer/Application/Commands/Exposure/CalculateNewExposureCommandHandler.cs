using MediatR;
using MicroserviceCommon.Application.Commands;
using MicroserviceCommon.Infrastructure.Idempotency;
using Offer.Domain.AggregatesModel.ApplicationAggregate;
using OfferApplication = Offer.Domain.AggregatesModel.ApplicationAggregate.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AssecoCurrencyConvertion;
using System.Globalization;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using MicroserviceCommon.ApiUtil;
using MicroserviceCommon.API.ApiUtils;
using Offer.Domain.AggregatesModel.ExposureModel;
using MicroserviceCommon.Services;
using Asseco.EventBus.Abstractions;
using MicroserviceCommon.Extensions.Broker;
using MicroserviceCommon.Models;
using System.Threading;
using MicroserviceCommon.Extensions;

using PriceCalculation.Calculations;
using Offer.API.Extensions;
using AuditClient;
using AuditClient.Model;

namespace Offer.API.Application.Commands
{
    public class CalculateNewExposureCommandHandler : IRequestHandler<CalculateNewExposureCommand, CommandStatus<Currency>>
    {
        private readonly IApplicationRepository _applicationRepository;
        private readonly IConfigurationService _configurationService;
        private readonly IMasterPartyDataService _masterPartyDataService;
        private readonly ApiEndPoints _apiEndPoints;
        private readonly IEventBus _eventBus;
        private readonly ILogger<InitiateOnlineOfferCommand> _logger;
        private readonly MessageEventFactory _messageEventFactory;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IAuditClient _auditClient;

        public CalculateNewExposureCommandHandler(
            IApplicationRepository applicationRepository,
            IEventBus eventBus,
            ILogger<InitiateOnlineOfferCommand> logger,
            ApiEndPoints apiEndPoints,
            MessageEventFactory messageEventFactory,
            IMasterPartyDataService masterPartyDataService,
            IConfigurationService configurationService,
            IHttpClientFactory httpClientFactory,
            IAuditClient auditClient)
        {
            this._applicationRepository = applicationRepository;
            _configurationService = configurationService ?? throw new ArgumentNullException(nameof(configurationService));
            _masterPartyDataService = masterPartyDataService ?? throw new ArgumentNullException(nameof(masterPartyDataService));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _apiEndPoints = apiEndPoints ?? throw new ArgumentNullException(nameof(apiEndPoints));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _messageEventFactory = messageEventFactory;
            _httpClientFactory = httpClientFactory;
            _auditClient = auditClient ?? throw new ArgumentNullException(nameof(auditClient));
        }

        public async Task<CommandStatus<Currency>> Handle(CalculateNewExposureCommand message, CancellationToken cancellationToken)
        {
            var application = await _applicationRepository.GetAsync(message.ApplicationNumber,
                "involved-parties,exposure-info,arrangement-requests");
            if (application == null)
            {
                _logger.LogError("Application {applicationNumber} not found for updating new exposure.", message.ApplicationNumber);
                new CommandStatus<Currency> { CommandResult = StandardCommandResult.NOT_FOUND };
            }
            List<string> listOfCalcParties = new List<string>();
            var customerNumber = application.CustomerNumber;
            var customer = application.InvolvedParties.Find(p => p.CustomerNumber == application.CustomerNumber);

            var exposureInfo = new ExposureInfo();
            if (application.ExposureInfo != null)
            {
                exposureInfo = application.ExposureInfo;
            }
            // exposure in current application
            exposureInfo.NewExposureInCurrentApplication = new ExposureList
            {
                Exposures = new List<Exposure>()
            };

            await CalculateExposuresForApplication(application, exposureInfo.NewExposureInCurrentApplication);

            // Convert amounts to TargetCurrency
            var totalCategory = _applicationRepository.CalculateExposureInTargetCurrency(exposureInfo.NewExposureInCurrentApplication);

            // exposure in other applications on Offer
            exposureInfo.NewExposureInOtherApplications = new ExposureList
            {
                Exposures = new List<Exposure>()
            };
            var activeAppStatuses = _configurationService.GetEffective("offer/active-statuses", "draft,active,approved,accepted").Result;
            var activeRoles = _configurationService.GetEffective("offer/exposure/active-roles", "customer,guarantor,co-debtor").Result;
            var statusesList = EnumUtils.GetEnumPropertiesForListString<ApplicationStatus>(activeAppStatuses);
            var rolesList = EnumUtils.GetEnumPropertiesForListString<PartyRole>(activeRoles);
            var otherApplications = new List<OfferApplication>();

            if (string.IsNullOrEmpty(customerNumber))
            {
                //prospect
                var email = ((IndividualParty)application.InvolvedParties.Where(x => x.PartyRole == PartyRole.Customer).FirstOrDefault()).EmailAddress;
                otherApplications = _applicationRepository.CheckExistingOffersForProspect(application.Initiator, email, statusesList, rolesList);
            }
            else
            {
                //existing customer
                await GetAppsForExposureRelatedGroups(customerNumber, customer, otherApplications, statusesList, rolesList, listOfCalcParties);
            }
            otherApplications = otherApplications
            .Where(x => !x.ApplicationNumber.Equals(application.ApplicationNumber))
            .ToList(); //exclude current application
            foreach (var app in otherApplications)
            {
                await CalculateExposuresForApplication(app, exposureInfo.NewExposureInOtherApplications);
            }

            // Exposure in other applications on other services
            try
            {
                var externalPotentialExposure = await GetExposureOnExternalApplications(application.CustomerNumber);
                exposureInfo.NewExposureInOtherApplications.Exposures.AddRange(externalPotentialExposure.Exposures);
                exposureInfo.NewExposureInOtherApplications.TotalApprovedAmountInTargetCurrency += externalPotentialExposure.TotalApprovedAmountInTargetCurrency;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An error occurred while calculating exposure in external sources");
            }

            // Convert amounts to TargetCurrency
            totalCategory = _applicationRepository.CalculateExposureInTargetCurrency(exposureInfo.NewExposureInOtherApplications);
            _applicationRepository.CalculateExposureInTargetCurrency(exposureInfo.CurrentExposure);
            _applicationRepository.CalculateExposureInTargetCurrency(exposureInfo.CreditBureauExposure);

            var targetCurrency = _configurationService.GetEffective("offer/exposure/target-currency", "EUR").Result;

            var totalCurrentExposure = exposureInfo.CurrentExposure?.TotalApprovedAmountInTargetCurrency ?? 0;
            var totalCurrentAppExposure = exposureInfo.NewExposureInCurrentApplication?.TotalApprovedAmountInTargetCurrency ?? 0;
            var totalOtherAppsExposure = exposureInfo.NewExposureInOtherApplications?.TotalApprovedAmountInTargetCurrency ?? 0;
            var totalCreditBureauExposure = exposureInfo.CreditBureauExposure?.TotalApprovedAmountInTargetCurrency ?? 0;

            var totalCurrentDebtExposure = exposureInfo.CurrentExposure?.TotalOutstandingAmountInTargetCurrency ?? 0;
            var totalCurrentDebtAppExposure = exposureInfo.NewExposureInCurrentApplication?.TotalOutstandingAmountInTargetCurrency ?? 0;
            var totalOtherDebtAppsExposure = exposureInfo.NewExposureInOtherApplications?.TotalOutstandingAmountInTargetCurrency ?? 0;
            var totalDebtCreditBureauExposure = exposureInfo.CreditBureauExposure?.TotalOutstandingAmountInTargetCurrency ?? 0;

            exposureInfo.TotalExposureApprovedAmount = totalCurrentExposure + totalCurrentAppExposure + totalOtherAppsExposure;
            exposureInfo.TotalExposureOutstandingAmount = totalCurrentDebtExposure + totalCurrentAppExposure + totalOtherAppsExposure;

            exposureInfo.TotalCbApprovedExposureAmount = totalCreditBureauExposure + totalCurrentAppExposure + totalOtherAppsExposure;
            exposureInfo.TotalCbOutstandingAmount = totalDebtCreditBureauExposure + totalCurrentAppExposure + totalOtherAppsExposure;

            exposureInfo.TotalExposureCurrency = targetCurrency;
            exposureInfo.CalculatedDate = DateTime.UtcNow;
            await getCalculatedExposure(exposureInfo);

            application.ExposureInfo = exposureInfo;
            _applicationRepository.Update(application);
            var messageBuilder = _messageEventFactory.CreateBuilder("offer", "offer-exposure-calculated")
                                 .AddBodyProperty("product-code", application.ProductCode)
                                 .AddBodyProperty("product-name", application.ProductName)
                                 .AddBodyProperty("preferential-price", application.PreferencialPrice)
                                 .AddBodyProperty("term-limit-breached", application.TermLimitBreached)
                                 .AddBodyProperty("amount-limit-brached", application.AmountLimitBreached)
                                 .AddHeaderProperty("application-number", application.ApplicationNumber);
            _eventBus.Publish(messageBuilder.Build());
            Currency result = null;
            bool resultOk = await _applicationRepository.UnitOfWork.SaveEntitiesAsync();
            if (resultOk)
            {
                result = new Currency { Amount = exposureInfo.TotalExposureApprovedAmount ?? 0, Code = exposureInfo.TotalExposureCurrency };
            }
            else
            {
                _logger.LogError("Updating new exposure data for application {applicationNumber} has failed.", message.ApplicationNumber);
                return new CommandStatus<Currency> { CommandResult = StandardCommandResult.INTERNAL_ERROR };
            }

            await _auditClient.WriteLogEntry(AuditLogEntryAction.Post, AuditLogEntryStatus.Success, "exposure-calculation", application.ApplicationNumber.ToString(), "Calculated new exposure", result);

            return new CommandStatus<Currency> { CommandResult = StandardCommandResult.OK, Result = result };
        }

        public async Task<ExposureInfo> getCalculatedExposure(ExposureInfo exposureInfo)
        {
            try
            {
                _logger.LogInformation("ANCI usao u getCalculatedExposure");
                exposureInfo.Calculated = new Dictionary<string, Currency>();
                exposureInfo.CurrencyExchangeRates = new Dictionary<string, decimal>();
                var targetCurrency = _configurationService.GetEffective("offer/exposure/target-currency", "EUR").Result;
                var calculatedFieldsExposureConfig = await _configurationService.GetEffective<ListConfigurationCalculateExposure>("offer/exposure/calculated-fields", "");
                if (calculatedFieldsExposureConfig != null)
                {
                    decimal calculatedValue = 0;
                    var conversionMethod = _configurationService.GetEffective("offer/exposure/currency-conversion-method", "Buy to middle").Result;                    
                    foreach (ConfigurationCalculateExposure calcConfig in calculatedFieldsExposureConfig.ConfigurationCalculateExposure)
                    {
                        var allSoruces = calcConfig.Sources.Split(",");
                        foreach (var src in allSoruces)
                        {
                            object value = typeof(ExposureInfo).GetProperty(src.Trim()).GetValue(exposureInfo);
                            if (value != null)
                            {
                                ExposureList listEposure = (ExposureList)value;
                                foreach (var exposure in listEposure.Exposures)
                                {
                                    if (calcConfig.RiskCategories.Contains(exposure.RiskCategory))
                                    {
                                        var dv = typeof(Exposure).GetProperty(calcConfig.Column).GetValue(exposure);
                                        //if (!exposure.Currency.Equals(calcConfig.Currency))
                                        //{
                                        //    dv = new CurrencyConverter().CurrencyConvert((decimal)dv, exposure.Currency, calcConfig.Currency,
                                        //        DateTime.Today.ToString("o", CultureInfo.InvariantCulture), conversionMethod);
                                        //}
                                        calculatedValue += (decimal)dv;
                                    }
                                }
                            }
                        }
                        _logger.LogInformation("ANCI pre convertovanja");

                        decimal convertedValue = new CurrencyConverter().CurrencyConvert(calculatedValue, targetCurrency, calcConfig.Currency,
                                                DateTime.Today.ToString("o", CultureInfo.InvariantCulture), conversionMethod);
                        _logger.LogDebug("CAS calculatedValue: {0} ", calculatedValue);
                        _logger.LogDebug("CAS targetCurrency: {0} ", targetCurrency);
                        _logger.LogDebug("CAS calcConfig.Currency: {0} ", calcConfig.Currency);
                        _logger.LogDebug("CAS date: {0} ", DateTime.Today.ToString("o", CultureInfo.InvariantCulture));
                        _logger.LogDebug("CAS conversionMethod: {0} ", conversionMethod);

                        _logger.LogInformation("ANCI posle convertovanja {0}",  convertedValue);
                        exposureInfo.Calculated.TryAdd(calcConfig.Name, new Currency() { Code = calcConfig.Currency, Amount = convertedValue });
                        
                        if (!calcConfig.Currency.Equals(targetCurrency))
                        {
                            exposureInfo.CurrencyExchangeRates.TryAdd(calcConfig.Currency, Math.Round(calculatedValue / convertedValue, 4));
                        }
                        calculatedValue = 0;
                    }
                }
                return exposureInfo;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, "An error occured while calculating exposure extended fields");
                return null;
            }
        }

        public async Task<ExposureList> GetExposureOnExternalApplications(string customerNumber)
        {
            try
            {
                var exposureList = new ExposureList
                {
                    Exposures = new List<Exposure>(),
                    TotalApprovedAmountInTargetCurrency = 0
                };
                var dataSourceConfig = await _configurationService.GetEffective<ExposureDataSourceConfiguration>("offer/exposure/other-sources", "");
                if (dataSourceConfig == null || dataSourceConfig.Count == 0)
                {
                    return exposureList;
                }
                var taskList = new List<Task<ExposureInfo>>();
                foreach (var dataSource in dataSourceConfig)
                {
                    taskList.Add(GetDataSourceExposure(dataSource, customerNumber));
                }
                var externalExposures = await Task.WhenAll(taskList);
                foreach (var exposure in externalExposures)
                {
                    if (exposure?.NewExposureInOtherApplications?.Exposures != null)
                    {
                        _logger.LogDebug("Adding {NumOfExternalExposures} external exposures", exposure.NewExposureInOtherApplications.Exposures.Count());
                        exposureList.Exposures.AddRange(exposure.NewExposureInOtherApplications.Exposures);
                        exposureList.TotalApprovedAmountInTargetCurrency += exposure.NewExposureInOtherApplications.TotalApprovedAmountInTargetCurrency;
                    }
                    else
                    {
                        _logger.LogDebug("exposure?.NewExposureInOtherApplications?.Exposures is null");
                    }
                }
                return exposureList;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private async Task<ExposureInfo> GetDataSourceExposure(ExposureDataSource dataSource, string customerNumber)
        {
            using (var httpClient = _httpClientFactory.CreateClient())
            {
                httpClient.AddDefaultJsonHeaders();
                try
                {
                    var requestContent = RequestContentSerializer.GetRequestContentForObject(new { CustomerNumber = customerNumber });
                    var url = GetQualifiedUrl(dataSource.UrlRoot);
                    if (!url.EndsWith("/"))
                    {
                        url += "/";
                    }
                    url += "exposure/calculate?x-asee-auth=true";
                    using (HttpResponseMessage response = await httpClient.PostAsync(url, requestContent))
                    {
                        var res = await response.Content.ReadAsStringAsync();
                        if (!string.IsNullOrEmpty(res))
                        {
                            ExposureInfo exposureInfo = (ExposureInfo)CaseUtil.ConvertFromJsonToObject<ExposureInfo>(res);
                            return exposureInfo;
                        }
                        _logger.LogWarning("Response of external exposure data request for customer number {CustomerNumber} is undefined", customerNumber);
                        return null;
                    }
                }
                catch (HttpRequestException e)
                {
                    _logger.LogError(e, "Could not fetch external exposure data for customer number: {CustomerNumber}", customerNumber);
                    throw e;
                }
            }
        }

        private string GetQualifiedUrl(string url)
        {
            if (!string.IsNullOrEmpty(url) && !url.StartsWith("http://") && !url.StartsWith("https://") && url.IndexOf("/") > -1)
            {
                var indexOfSlash = url.IndexOf("/");
                var serviceName = url.Substring(0, indexOfSlash);
                var qualifiedServiceName = _apiEndPoints.GetServiceProtocol() + EndpointUtil.GetQualifiedServiceName(serviceName);
                return qualifiedServiceName + url.Substring(indexOfSlash);
            }
            return url;
        }

        public async Task<List<OfferApplication>> GetAppsForExposureRelatedGroups(string customerNumber, Party involvedParty, List<OfferApplication> otherApplications, List<ApplicationStatus> statusesList, List<PartyRole> rolesList, List<string> listOfCalcParties)
        {
            var exposureRelatedGroups = _configurationService.GetEffective<List<string>>("offer/exposure-related-groups", "ownership").Result;
            if (!listOfCalcParties.Contains(involvedParty.CustomerNumber))
            {
                listOfCalcParties.Add(involvedParty.CustomerNumber);

                if (involvedParty.PartyKind == PartyKind.Organization)
                {
                    var org = (OrganizationParty)involvedParty;

                    var relatedParties = org.Relationships != null ? org.Relationships.Where(r => exposureRelatedGroups.Contains(r.Kind) && !listOfCalcParties.Contains(r.ToParty.Number)).ToList() : null;
                    otherApplications.AddRange(_applicationRepository.CheckExistingOffersForCustomer(customerNumber, statusesList, rolesList));
                    if (relatedParties != null)
                    {
                        foreach (var relatedParty in relatedParties)
                        {
                            if (!listOfCalcParties.Contains(relatedParty.ToParty.Number))
                            {
                                // listOfCalcParties.Add(relatedParty.ToParty.Number);
                                if (relatedParty.ToParty.Kind == PartyKind.Individual)
                                {
                                    //existing customer
                                    Party partyorg = new IndividualParty { CustomerNumber = relatedParty.ToParty.Number, PartyKind = relatedParty.ToParty.Kind };
                                    var inparty = await _masterPartyDataService.GetPartyData(partyorg);

                                    await GetAppsForExposureRelatedGroups(relatedParty.ToParty.Number, inparty, otherApplications, statusesList, rolesList, listOfCalcParties);
                                    // otherApplications.AddRange(_applicationRepository.CheckExistingOffersForCustomer(relatedParty.ToParty.Number, statusesList, rolesList));
                                }
                                else
                                {
                                    Party partyorg = new OrganizationParty { CustomerNumber = relatedParty.ToParty.Number, PartyKind = relatedParty.ToParty.Kind };
                                    var orgparty = await _masterPartyDataService.GetPartyData(partyorg);

                                    await GetAppsForExposureRelatedGroups(relatedParty.ToParty.Number, orgparty, otherApplications, statusesList, rolesList, listOfCalcParties);
                                }
                            }
                        }
                    }
                }
                else
                {
                    var org = (IndividualParty)involvedParty;

                    var relatedParties = org.Relationships != null ? org.Relationships.Where(r => exposureRelatedGroups.Contains(r.Kind) && !listOfCalcParties.Contains(r.ToParty.Number)).ToList() : null;
                    otherApplications.AddRange(_applicationRepository.CheckExistingOffersForCustomer(customerNumber, statusesList, rolesList));
                    if (relatedParties != null)
                    {
                        foreach (var relatedParty in relatedParties)
                        {
                            if (!listOfCalcParties.Contains(relatedParty.ToParty.Number))
                            {
                                // listOfCalcParties.Add(relatedParty.ToParty.Number);
                                if (relatedParty.ToParty.Kind == PartyKind.Individual)
                                {
                                    Party partyin = new IndividualParty { CustomerNumber = relatedParty.ToParty.Number, PartyKind = relatedParty.ToParty.Kind };
                                    var inparty = await _masterPartyDataService.GetPartyData(partyin);

                                    await GetAppsForExposureRelatedGroups(relatedParty.ToParty.Number, inparty, otherApplications, statusesList, rolesList, listOfCalcParties);
                                    // otherApplications.AddRange(_applicationRepository.CheckExistingOffersForCustomer(relatedParty.ToParty.Number, statusesList, rolesList));
                                }
                                else
                                {
                                    Party partyorg = new OrganizationParty { CustomerNumber = relatedParty.ToParty.Number, PartyKind = relatedParty.ToParty.Kind };
                                    var orgparty = await _masterPartyDataService.GetPartyData(partyorg);

                                    await GetAppsForExposureRelatedGroups(relatedParty.ToParty.Number, orgparty, otherApplications, statusesList, rolesList, listOfCalcParties);
                                }
                            }
                        }
                    }
                }
            }
            return otherApplications;
        }

        private async Task<bool> CalculateExposuresForApplication(OfferApplication application, ExposureList exposureList)
        {
            try
            {
                if (application.ArrangementRequests == null)
                {
                    application = await _applicationRepository.GetAsync(application.ApplicationId, "arrangement-requests");
                }
                var checkBalanceList = _configurationService.GetEffective("offer/exposure/balance-arrangement-kinds", "term-loan").Result;
                var creditArrangementTypes = EnumUtils.GetEnumPropertiesForListString<ArrangementKind>("term-loan,overdraft-facility,credit-facility,credit-card-facility");
                var arrangements = application.ArrangementRequests
                    .Where(x => (x.Enabled ?? false) &&
                        creditArrangementTypes.Contains(x.ArrangementKind.GetValueOrDefault()))
                    .ToList();
                foreach (var arr in arrangements)
                {
                    // due to abovementioned and selected arrangement kinds we can do this                    
                    var financial = (FinanceServiceArrangementRequest)arr;
                    var termMonths = 0;
                    if (!string.IsNullOrEmpty(financial.Term) && financial.Term.StartsWith("P"))
                    {
                        termMonths = Utility.GetMonthsFromPeriod(financial.Term);
                    }
                    else if (!string.IsNullOrEmpty(financial.Term))
                    {
                        termMonths = Convert.ToInt32(financial.Term);
                    }
                    string riskCategory = ResolveRiskCategory(arr.ArrangementKind);
                    Exposure exp = new Exposure
                    {
                        PartyId = application.CustomerNumber,
                        CustomerName = application.CustomerName,
                        ArrangementKind = arr.ArrangementKind,
                        Term = financial.Term != null ? "P" + termMonths / 12 + "Y" + termMonths % 12 + "M" : "P0M",
                        isBalance = checkBalanceList.Contains(StringExtensions.ToKebabCase(arr.ArrangementKind.ToString())) ? true : false,
                        AccountNumber = arr.ApplicationNumber + "/" + arr.ArrangementRequestId,
                        Currency = financial.Currency,
                        ExposureApprovedInSourceCurrency = financial.Amount,
                        AnnuityInSourceCurrency = financial.TotalAnnuity,
                        ExposureOutstandingAmountInSourceCurrency = financial.Amount,
                        RiskCategory = riskCategory
                    };
                    if (arr.ArrangementKind == ArrangementKind.TermLoan)
                    {
                        var termLoan = (TermLoanRequest)arr;
                        exp.AnnuityInSourceCurrency = termLoan.Annuity;
                    }
                    exposureList.Exposures.Add(exp);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "CalculateExposuresForApplication exception");
                throw e;
            }
            return true;
        }

        public string ResolveRiskCategory(ArrangementKind? kind = null)
        {
            // To do - resolve category by arrangement kind
            string riskCategory = "1";
            return riskCategory;
        }
    }


    public class CalculateNewExposureIdentifiedCommandHandler : IdentifiedCommandHandler<CalculateNewExposureCommand, CommandStatus<Currency>>
    {
        public CalculateNewExposureIdentifiedCommandHandler(IMediator mediator, IRequestManager requestManager) : base(mediator, requestManager)
        {
        }

        protected override CommandStatus<Currency> CreateResultForDuplicateRequest()
        {
            return new CommandStatus<Currency> { CommandResult = StandardCommandResult.OK };
        }
    }
}
