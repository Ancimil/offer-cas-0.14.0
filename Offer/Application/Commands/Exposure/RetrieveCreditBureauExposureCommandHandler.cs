using AssecoCurrencyConvertion;
using AuditClient;
using AuditClient.Model;
using MediatR;
using MicroserviceCommon.API.ApiUtils;
using MicroserviceCommon.Application.Commands;
using MicroserviceCommon.Infrastructure.Idempotency;
using MicroserviceCommon.Models;
using MicroserviceCommon.Services;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Offer.Domain.AggregatesModel.ApplicationAggregate;
using Offer.Domain.AggregatesModel.ExposureModel;
using Offer.Domain.Repository;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Offer.API.Application.Commands
{
    public class RetrieveCreditBureauExposureCommandHandler : IRequestHandler<RetrieveCreditBureauExposureCommand, CommandStatus>
    {
        private readonly IApplicationRepository _applicationRepository;
        private readonly IConfigurationService _configurationService;
        private readonly IInvolvedPartyRepository _involvedPartyRepository;
        private readonly IAuditClient _auditClient;
        private readonly ILogger<RetrieveCreditBureauExposureCommand> _logger;

        public RetrieveCreditBureauExposureCommandHandler(IApplicationRepository applicationRepository, IConfigurationService configurationService, 
            ILogger<RetrieveCreditBureauExposureCommand> logger, IInvolvedPartyRepository involvedPartyRepository, IAuditClient auditClient)
        {
            _applicationRepository = applicationRepository ?? throw new ArgumentNullException(nameof(applicationRepository));
            _configurationService = configurationService ?? throw new ArgumentNullException(nameof(configurationService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _involvedPartyRepository = involvedPartyRepository ?? throw new ArgumentNullException(nameof(involvedPartyRepository));
            _auditClient = auditClient ?? throw new ArgumentNullException(nameof(auditClient));
        }

        public async Task<CommandStatus> Handle(RetrieveCreditBureauExposureCommand request, CancellationToken cancellationToken)
        {
            var application = await _applicationRepository.GetAsync(request.ApplicationNumber, "exposure-info,involved-parties");
            List<int> listParties = new List<int>();
            foreach (var e in request.CreditBureauExposures)
            {
                if (!listParties.Contains(int.Parse(e.PartyId)))
                {
                    listParties.Add(int.Parse(e.PartyId));
                }
            }
            // var listParties = application.InvolvedParties.ToList(); // Find(p => p.CustomerNumber == application.CustomerNumber);
            if (application == null)
            {
                _logger.LogError("Application {applicationNumber} not found for updating credit bureau exposure.", request.ApplicationNumber);
                return new CommandStatus { CommandResult = StandardCommandResult.NOT_FOUND };
            }
            var exposureInfo = application.ExposureInfo ?? new ExposureInfo();
            var listExposure = new List<Exposure>();

            foreach (var party in listParties)
            {
                await GetCreditBureauExposureIndividual(listExposure, request, party);
            }

            exposureInfo.CreditBureauExposure = new ExposureList()
            {
                Exposures = listExposure
            };
            _applicationRepository.CalculateExposureInTargetCurrency(exposureInfo.CreditBureauExposure);
            _applicationRepository.CalculateExposureInTargetCurrency(exposureInfo.NewExposureInOtherApplications);
            _applicationRepository.CalculateExposureInTargetCurrency(exposureInfo.NewExposureInCurrentApplication);
            _applicationRepository.CalculateExposureInTargetCurrency(exposureInfo.CurrentExposure);

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

            bool resultOk = await _applicationRepository.UnitOfWork.SaveEntitiesAsync();

            await _auditClient.WriteLogEntry(AuditLogEntryAction.Get, AuditLogEntryStatus.Success, "credit-bireau-exposure", application.ApplicationNumber, "Updated credit bureau exposure data", application.ExposureInfo);

            if (!resultOk)
            {
                _logger.LogError("Updating credit bureau exposure data for application {applicationNumber} has failed.", request.ApplicationNumber);
            }

            return new CommandStatus { CommandResult = StandardCommandResult.OK };
        }

        public async Task<ExposureInfo> getCalculatedExposure(ExposureInfo exposureInfo)
        {
            try
            {
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
                        decimal convertedValue = new CurrencyConverter().CurrencyConvert(calculatedValue, targetCurrency, calcConfig.Currency,
                                                DateTime.Today.ToString("o", CultureInfo.InvariantCulture), conversionMethod);
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

        public async Task<List<Exposure>> GetCreditBureauExposureIndividual(List<Exposure> listExposure, RetrieveCreditBureauExposureCommand request, int partyId)
        {
            try
            {
                var _party = await _involvedPartyRepository.GetPartyGeneralInformation(request.ApplicationNumber, partyId, "employment-data,household-info,financial-profile,contact-points,credit-bureau,product-usage");
                var checkBalanceList = _configurationService.GetEffective("offer/exposure/balance-arrangement-kinds", "term-loan").Result;
                var creditArrangementTypes = _configurationService.GetEffective("offer/exposure/credit-bureau-placement-kinds", "term-loan,term-loan-debtor,overdraft-facility,credit-facility,credit-card-facility").Result.Split(",");

                if (_party.PartyKind == PartyKind.Individual)
                {
                    var ind = (IndividualParty)_party;
                    var listExposures = request.CreditBureauExposures.Where(p => p.PartyId.Equals(ind.PartyId.ToString())).ToList();

                    if (listExposure != null)
                    {
                        foreach (var exp in listExposures)
                        {
                            if (creditArrangementTypes.Contains(exp.Kind))
                            {
                                Exposure exposure = new Exposure
                                {
                                    AccountNumber = "",
                                    PartyId = ind.CustomerNumber,
                                    Currency = exp.Currency,
                                    ExposureApprovedInSourceCurrency = exp.ExposureInSourceCurrency ?? 0,
                                    CustomerName = ind.CustomerName,
                                    ArrangementKind = ArrangementKind.OtherProductArrangement,
                                    Term = "P0M",
                                    isBalance = checkBalanceList.Contains(exp.Kind),
                                    AnnuityInSourceCurrency = exp.AnnuityInSourceCurrency ?? 0,
                                    RiskCategory = exp.RiskCategory,
                                    ExposureOutstandingAmountInSourceCurrency = exp.ExposureDebtInSourceCurrency ?? 0
                                };

                                var kind = EnumUtils.ToEnum<ArrangementKind>(exp.Kind);
                                if (kind != null)
                                {
                                    exposure.ArrangementKind = kind.Value;
                                }

                                if (exp.EndDate != null && exp.StartDate != null)
                                {
                                    int months = ((exp.EndDate.Value.Year - exp.StartDate.Value.Year) * 12) + exp.EndDate.Value.Month - exp.StartDate.Value.Month;
                                    exposure.Term = "P" + months / 12 + "Y" + months % 12 + "M";
                                }
                                listExposure.Add(exposure);
                            }
                        }
                    }
                }
                return listExposure;
            }
            catch (Exception ex)
            {
                _logger.LogError("Updating credit bureau exposure data for application {applicationNumber} has failed. Error: " + ex.Message, request.ApplicationNumber);
                return null;
            }
        }
    }

    public class RetrieveCreditBureauExposureIdentifiedCommandHandler : IdentifiedCommandHandler<RetrieveCreditBureauExposureCommand, CommandStatus>
    {
        public RetrieveCreditBureauExposureIdentifiedCommandHandler(IMediator mediator, IRequestManager requestManager) : base(mediator, requestManager)
        {
        }

        protected override CommandStatus CreateResultForDuplicateRequest()
        {
            return new CommandStatus { CommandResult = StandardCommandResult.OK };
        }
    }
}
