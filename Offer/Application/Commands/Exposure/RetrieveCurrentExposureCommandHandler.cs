using MediatR;
using MicroserviceCommon.Application.Commands;
using MicroserviceCommon.Infrastructure.Idempotency;
using Offer.Domain.AggregatesModel.ApplicationAggregate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using MicroserviceCommon.ApiUtil;
using Newtonsoft.Json.Linq;
using MicroserviceCommon.API.ApiUtils;
using Offer.Domain.AggregatesModel.ExposureModel;
using MicroserviceCommon.Services;
using MicroserviceCommon.Models;
using System.Threading;
using Offer.API.Extensions;
using Offer.Domain.Services;
using AuditClient;
using AuditClient.Model;
using Newtonsoft.Json;

namespace Offer.API.Application.Commands
{
    public class RetrieveCurrentExposureCommandHandler : IRequestHandler<RetrieveCurrentExposureCommand, CommandStatus>
    {
        private readonly IApplicationRepository _applicationRepository;
        private readonly IConfigurationService _configurationService;
        private readonly IArrangementService _arrangementService;
        private readonly IMasterPartyDataService _masterPartyDataService;
        private readonly ApiEndPoints _apiEndPoints;
        private readonly IMediator _mediator;
        private readonly ILogger<InitiateOnlineOfferCommand> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IAuditClient _auditClient;

        public RetrieveCurrentExposureCommandHandler(
            IMediator mediator,
            IApplicationRepository applicationRepository,
            ILogger<InitiateOnlineOfferCommand> logger,
            ApiEndPoints apiEndPoints,
            IConfigurationService configurationService,
            IArrangementService arrangementService,
            IMasterPartyDataService masterPartyDataService,
            IHttpClientFactory httpClientFactory,
            IAuditClient auditClient)
        {
            _httpClientFactory = httpClientFactory;
            _auditClient = auditClient ?? throw new ArgumentNullException(nameof(auditClient));
            _applicationRepository = applicationRepository;
            _configurationService = configurationService ?? throw new ArgumentNullException(nameof(configurationService));
            _arrangementService = arrangementService ?? throw new ArgumentNullException(nameof(arrangementService));
            _masterPartyDataService = masterPartyDataService ?? throw new ArgumentNullException(nameof(masterPartyDataService));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _apiEndPoints = apiEndPoints ?? throw new ArgumentNullException(nameof(apiEndPoints));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<CommandStatus> Handle(RetrieveCurrentExposureCommand message, CancellationToken cancellationToken)
        {
            var application = await _applicationRepository.GetAsync(message.ApplicationNumber, "exposure-info,involved-parties");
            if (application == null)
            {
                _logger.LogError("Application {applicationNumber} not found for updating current exposure.", message.ApplicationNumber);
                return new CommandStatus { CommandResult = StandardCommandResult.NOT_FOUND };
            }
            List<string> listOfCalcParties = new List<string>();
            var exposureInfo = application.ExposureInfo ?? new ExposureInfo();

            exposureInfo.CurrentExposure = new ExposureList
            {
                Exposures = new List<Exposure>()
            };
            var customerNumber = application.CustomerNumber;
            if (!string.IsNullOrEmpty(customerNumber))
            {
                var customer = application.InvolvedParties.Find(p => p.CustomerNumber == application.CustomerNumber); //customer

                // call arrangement api to retrieve credit arrangements where customer plays "customer", "guarantor", or "co-debtor" role
                try
                {
                    await CalculateExposureRelatedGroups(exposureInfo, customerNumber, message, customer, listOfCalcParties);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "An error occurred while retrieving current exposure for application number {applicationNumber}", message.ApplicationNumber);
                    return new CommandStatus { CommandResult = StandardCommandResult.INTERNAL_ERROR, Exception = e };
                }
            }

            // Convert amounts to TargetCurrency 
            _applicationRepository.CalculateExposureInTargetCurrency(exposureInfo.CurrentExposure);
            _applicationRepository.CalculateExposureInTargetCurrency(exposureInfo.CreditBureauExposure);
            _applicationRepository.CalculateExposureInTargetCurrency(exposureInfo.NewExposureInCurrentApplication);
            _applicationRepository.CalculateExposureInTargetCurrency(exposureInfo.NewExposureInOtherApplications);

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
            application.ExposureInfo = exposureInfo;
            // Records data in OfferexposureInfo.CurrentExposure?.TotalApprovedAmountInTargetCurrency || 0
            _applicationRepository.Update(application);

            bool resultOk = await _applicationRepository.UnitOfWork.SaveEntitiesAsync();

            JObject auditData = new JObject(
                        new JProperty("username", message.Username),
                        new JProperty("product-number", application.ProductCode),
                        new JProperty("product-name", application.ProductName),
                        new JProperty("channel", application.ChannelCode),
                        new JProperty("action", "Updated exposure info")
                );

            await _auditClient.WriteLogEntry(AuditLogEntryAction.Update, AuditLogEntryStatus.Success, "current-exposure", application.ApplicationNumber, auditData);

            if (!resultOk)
            {
                _logger.LogError("Updating current exposure data for application {applicationNumber} has failed.", message.ApplicationNumber);
            }

            return new CommandStatus { CommandResult = StandardCommandResult.OK };
        }

        public async Task<ExposureInfo> GetExposureIndividual(ExposureInfo exposureInfo, string customerNumber, string customerName, RetrieveCurrentExposureCommand message)
        {
            JArray arrangementList = await _arrangementService.GetArrangements(customerNumber, message.ActiveStatuses, message.ActiveRoles, message.ArrangementType);

            foreach (var arrangement in arrangementList)
            {
                string accounts = "";
                string arrangementKind = arrangement["kind"].ToString();
                string term = arrangement["term"] != null ? arrangement["term"].ToString() : "P0M";
                string riskCategory = arrangement["risk-category"] != null ? arrangement["risk-category"].ToString() : "1";
                JArray accountList = (JArray)arrangement["accounts"];
                var primaryAccounts = accountList.Where(x => x["role-kind"].ToString().Equals("primary-account")).ToList();
                accounts = string.Join(",", primaryAccounts.Select(a => a["account-number"].ToString()).ToList());

                exposureInfo.CurrentExposure.Exposures.AddRange(
                    await GetExposuresForArrangement(accounts, arrangementKind, customerNumber, customerName, term, riskCategory)
                );
            }

            return exposureInfo;
        }

        public async Task<ExposureInfo> CalculateExposureRelatedGroups(ExposureInfo exposureInfo, string customerNumber, RetrieveCurrentExposureCommand message, Party involvedParty, List<string> listOfCalcParties)
        {

            var exposureRelatedGroups = _configurationService.GetEffective<List<string>>("offer/exposure-related-groups", "ownership").Result;
            if (!listOfCalcParties.Contains(involvedParty.CustomerNumber))
            {
                listOfCalcParties.Add(involvedParty.CustomerNumber);

                if (involvedParty.PartyKind == PartyKind.Organization)
                {
                    var org = (OrganizationParty)involvedParty;

                    var relatedParties = org.Relationships != null ? org.Relationships.Where(r => exposureRelatedGroups.Contains(r.Kind) && !listOfCalcParties.Contains(r.ToParty.Number)).ToList() : null;

                    await GetExposureIndividual(exposureInfo, org.CustomerNumber, org.CustomerName, message);
                    if (relatedParties != null)
                    {
                        foreach (var relatedParty in relatedParties)
                        {
                            if (!listOfCalcParties.Contains(relatedParty.ToParty.Number))
                            {
                                // listOfCalcParties.Add(relatedParty.ToParty.Number);
                                // call arrangement api to retrieve credit arrangements where customer plays "customer", "guarantor", or "co-debtor" role
                                try
                                {

                                    if (relatedParty.ToParty.Kind == PartyKind.Organization)
                                    {
                                        Party party = new OrganizationParty { CustomerNumber = relatedParty.ToParty.Number, PartyKind = relatedParty.ToParty.Kind };
                                        var orgparty = await _masterPartyDataService.GetPartyData(party);

                                        await CalculateExposureRelatedGroups(exposureInfo, relatedParty.ToParty.Number, message, orgparty, listOfCalcParties);
                                    }
                                    else
                                    {
                                        Party party = new IndividualParty { CustomerNumber = relatedParty.ToParty.Number, PartyKind = relatedParty.ToParty.Kind };
                                        var orgparty = await _masterPartyDataService.GetPartyData(party);

                                        await CalculateExposureRelatedGroups(exposureInfo, relatedParty.ToParty.Number, message, orgparty, listOfCalcParties);
                                    }
                                }
                                catch (Exception e)
                                {
                                    _logger.LogError(e, "An error occurred while retrieving current exposure for application number {applicationNumber}", message.ApplicationNumber);
                                    // return new CommandStatus { CommandResult = StandardCommandResult.INTERNAL_ERROR, Exception = e };
                                }
                            }
                        }
                    }
                }
                else
                {
                    var org = (IndividualParty)involvedParty;

                    var relatedParties = org.Relationships != null ? org.Relationships.Where(r => exposureRelatedGroups.Contains(r.Kind) && !listOfCalcParties.Contains(r.ToParty.Number)).ToList() : null;

                    await GetExposureIndividual(exposureInfo, org.CustomerNumber, org.CustomerName, message);
                    if (relatedParties != null)
                    {
                        foreach (var relatedParty in relatedParties)
                        {
                            if (!listOfCalcParties.Contains(relatedParty.ToParty.Number))
                            {
                                // listOfCalcParties.Add(relatedParty.ToParty.Number);
                                // call arrangement api to retrieve credit arrangements where customer plays "customer", "guarantor", or "co-debtor" role
                                try
                                {

                                    if (relatedParty.ToParty.Kind == PartyKind.Organization)
                                    {
                                        Party party = new OrganizationParty { CustomerNumber = relatedParty.ToParty.Number, PartyKind = relatedParty.ToParty.Kind };
                                        var orgparty = await _masterPartyDataService.GetPartyData(party);

                                        await CalculateExposureRelatedGroups(exposureInfo, relatedParty.ToParty.Number, message, orgparty, listOfCalcParties);
                                    }
                                    else
                                    {
                                        Party party = new IndividualParty { CustomerNumber = relatedParty.ToParty.Number, PartyKind = relatedParty.ToParty.Kind };
                                        var orgparty = await _masterPartyDataService.GetPartyData(party);

                                        await CalculateExposureRelatedGroups(exposureInfo, relatedParty.ToParty.Number, message, orgparty, listOfCalcParties);
                                    }
                                }
                                catch (Exception e)
                                {
                                    _logger.LogError(e, "An error occurred while retrieving current exposure for application number {applicationNumber}", message.ApplicationNumber);
                                    // return new CommandStatus { CommandResult = StandardCommandResult.INTERNAL_ERROR, Exception = e };
                                }
                            }
                        }
                    }
                }
            }
            return exposureInfo;
        }


        public async Task<JArray> GetActiveCreditArrangements(string customerNumber)
        {
            using (var httpClient = _httpClientFactory.CreateClient())
            {
                httpClient.AddDefaultJsonHeaders();
                try
                {
                    var activeStatuses = _configurationService.GetEffective("arrangement/active-statuses", "effective").Result;
                    var activeRoles = _configurationService.GetEffective("offer/exposure/active-roles", "customer,guarantor,co-debtor").Result;
                    var creditArrangementTypes = "term-loan,overdraft-facility,credit-facility,credit-card-facility";
                    JArray list = null;
                    using (HttpResponseMessage response = await httpClient.GetAsync(_apiEndPoints.GetServiceUrl("arrangement") + "arrangements/?customer-id=" + customerNumber + "&statuses=" + activeStatuses
                                                                   + "&page-size=1000&role-kinds=" + activeRoles + "&kinds=" + creditArrangementTypes))
                    {
                        var res = await response.Content.ReadAsStringAsync();
                        if (!res.Equals(""))
                        {
                            JObject json = JObject.Parse(res);
                            list = (JArray)json["items"];
                        }
                        return list;
                    }
                }
                catch (HttpRequestException e)
                {
                    _logger.LogError(e, "Could not fetch arrangements data for party number: {partyNumber}", customerNumber);
                    throw e;
                }
            }
        }

        public async Task<List<Exposure>> GetExposuresForArrangement(string accountsList, string arrangementKind, string customerNumber, string customerName, string term, string riskCategory)
        {
            var exposuresList = new List<Exposure>();
            var checkBalanceList = _configurationService.GetEffective("offer/exposure/balance-arrangement-kinds", "term-loan").Result;
            // call account-data API to retrieve balances for accounts in this arrangement
            List<Balance> balanceList = await GetBalancesPerArrangementKind(accountsList, arrangementKind);

            var queryGroupAccountsByCurrency =
                    from balance in balanceList
                    group balance by new { balance.AccountNumber, balance.Currency } into newGroup
                    select newGroup;

            foreach (var balanceGroup in queryGroupAccountsByCurrency)
            {
                Exposure exp = new Exposure
                {
                    PartyId = customerNumber,
                    CustomerName = customerName,
                    ArrangementKind = EnumUtils.ToEnum<ArrangementKind>(arrangementKind),
                    Term = term,
                    isBalance = checkBalanceList.Contains(arrangementKind) ? true : false,
                    AccountNumber = balanceGroup.Key.AccountNumber,
                    Currency = balanceGroup.Key.Currency,
                    ExposureApprovedInSourceCurrency = 0,
                    RiskCategory = riskCategory
                };
                foreach (var balance in balanceGroup)
                {
                    if (balance.Direction.Equals("c"))
                    {
                        balance.Amount = -1 * balance.Amount;
                    }
                    exp.ExposureApprovedInSourceCurrency += balance.Amount;
                }
                exp.ExposureOutstandingAmountInSourceCurrency = exp.ExposureApprovedInSourceCurrency;
                exposuresList.Add(exp);
            }

            return exposuresList;
        }

        public string ResolveRiskCategory(ArrangementKind? kind = null)
        {
            string riskCategory = "1";
            return riskCategory;
        }

        public async Task<List<Balance>> GetBalancesPerArrangementKind(string accountsList, string arrangementKind)
        {
            using (var httpClient = _httpClientFactory.CreateClient())
            {
                httpClient.AddDefaultJsonHeaders();
                try
                {
                    _logger.LogDebug("BEfore get confiuration");
                    // filter balances by type of arrangement kind / load from configuration
                    var balanceTypes = _configurationService.GetEffective("offer/exposure/balance-types/" + arrangementKind, "available,outstanding&&x-asee-auth=true").Result;
                    _logger.LogDebug("Get confiuration balanceTypes {0}", balanceTypes);
                    List<Balance> list = null;
                    using (HttpResponseMessage response = await httpClient.GetAsync(_apiEndPoints.GetServiceUrl("account-data") + "balances/?balance-kinds=" + balanceTypes + "&accounts=" + accountsList + "&&x-asee-auth=true"))
                    {
                        var res = await response.Content.ReadAsStringAsync();
                        if (!res.Equals(""))
                        {
                            list = (List<Balance>)CaseUtil.ConvertFromJsonToObject(res, typeof(List<Balance>));
                        }
                        return list;
                    }
                }
                catch (HttpRequestException e)
                {
                    _logger.LogError(e, "Could not fetch balances for accounts: {accountNumbers}", accountsList);
                    throw e;
                }
            }
        }

    }


    public class RetrieveCurrentExposureIdentifiedCommandHandler : IdentifiedCommandHandler<RetrieveCurrentExposureCommand, CommandStatus>
    {
        public RetrieveCurrentExposureIdentifiedCommandHandler(IMediator mediator, IRequestManager requestManager) : base(mediator, requestManager)
        {
        }

        protected override CommandStatus CreateResultForDuplicateRequest()
        {
            return new CommandStatus { CommandResult = StandardCommandResult.OK };
        }
    }
}