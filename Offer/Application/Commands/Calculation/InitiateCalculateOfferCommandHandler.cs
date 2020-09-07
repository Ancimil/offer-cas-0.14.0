using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Offer.Domain.AggregatesModel.ApplicationAggregate;
using Offer.Domain.Calculations;
using MicroserviceCommon.Application.Commands;
using MicroserviceCommon.Infrastructure.Idempotency;
using Microsoft.Extensions.Logging;
using MicroserviceCommon.Services;
using MicroserviceCommon.Models.Product;
using AutoMapper;
using System.Threading;
using Offer.Domain.Utils;
using PriceCalculation.Exceptions;
using PriceCalculation.Services;
using PriceCalculation.Models.Product;
using PriceCalculation.Calculations;
using OfferApplication = Offer.Domain.AggregatesModel.ApplicationAggregate.Application;
using MediatR;
using AuditClient;
using AuditClient.Model;

namespace Offer.API.Application.Commands
{
    public class InitiateCalculateOfferCommandHandler : IRequestHandler<InitiateCalculateOfferCommand, ArrangementRequest>
    {
        private readonly ILogger<InitiateCalculateOfferCommand> _logger;
        private readonly IPriceCalculationService _priceCalculation;
        private readonly ArrangementRequestFactory _requestFactory;
        private readonly CalculatorProvider _calculatorProvider;
        private readonly IConfigurationService _configurationService;
        private readonly IAuditClient _auditClient;

        public InitiateCalculateOfferCommandHandler(ILogger<InitiateCalculateOfferCommand> logger,
            IPriceCalculationService priceCalculation, ArrangementRequestFactory requestFactory,
            CalculatorProvider calculatorProvider, IConfigurationService configurationService,
            IAuditClient auditClient)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _priceCalculation = priceCalculation ?? throw new ArgumentNullException(nameof(priceCalculation));
            _requestFactory = requestFactory ?? throw new ArgumentNullException(nameof(requestFactory));
            _calculatorProvider = calculatorProvider ?? throw new ArgumentNullException(nameof(calculatorProvider));
            _configurationService = configurationService ?? throw new ArgumentNullException(nameof(configurationService));
            _auditClient = auditClient ?? throw new ArgumentNullException(nameof(auditClient)); ;
        }

        public async Task<ArrangementRequest> Handle(InitiateCalculateOfferCommand message, CancellationToken cancellationToken)
        {
            ProductConditions conditions = await _priceCalculation.ReadVariationDefinitions(message.ProductConditions);
            var parameters = Mapper.Map<InitiateCalculateOfferCommand, ArrangementRequestInitializationParameters>(message);
            try
            {
                parameters.MaturityDate = Utility.GetEndDateFromPeriod(parameters.Term, message.CalculationDate);
            }
            catch (InvalidTermException)
            {
                // do nothing
            }
            ArrangementRequest result = _requestFactory.GetForArrangementKind(parameters, message.ArrangementKind);

            if (parameters.MaturityDate.HasValue && result.IsFinanceService())
            {
                FinanceServiceArrangementRequest fsr = result as FinanceServiceArrangementRequest;
                fsr.MaturityDate = parameters.MaturityDate.Value;
            }

            result.ParentProductCode = message.PartOfBundle;
            result.Campaign = message.Campaign;
            result.Options = message.ProductOptions;
            if (result is FinanceServiceArrangementRequest finRequest)
            {
                finRequest.CollateralModel = message.CollateralModel;
            }
            Dictionary<string, OptionGroup> productShapsotOptions = new Dictionary<string, OptionGroup>();
            if (message.ProductOptions != null)
            {

                foreach (var option in message.ProductOptions)
                {
                    if (productShapsotOptions.TryGetValue(option.GroupCode, out OptionGroup optionGroup))
                    {
                        optionGroup.Options.Add(new Option
                        {
                            Code = option.Code,
                            Description = option.Description,
                            Effects = option.Effects
                        });
                    }
                    else
                    {
                        productShapsotOptions.Add(option.GroupCode, new OptionGroup
                        {
                            Code = option.GroupCode,
                            Description = option.GroupDescription,
                            Options = new List<Option>
                            {
                                new Option
                                {
                                    Code = option.Code,
                                    Description = option.Description,
                                    Effects = option.Effects
                                }
                            }
                        });
                    }
                }
            }
            result.ProductSnapshot = new ProductSnapshot
            {
                Conditions = conditions,
                Campaign = message.Campaign,
                OptionGroups = productShapsotOptions.Values.ToList(),
                MinimumDaysForFirstInstallment = message.MinimumDaysForFirstInstallment
            };

            OfferApplication application = new OfferApplication
            {
                CustomerSegment = message.CustomerSegment,
                CollateralModel = message.CollateralModel,
                RiskScore = message.RiskScore ?? null,
                ChannelCode = message.Channel,
                DebtToIncome = message.DebtToIncome,
                CustomerValue = message.CustomerValue,
                CreditRating = message.CreditRating,
                RequestDate = message.RequestDate
            };

            try
            {
                _logger.LogInformation("Calculating offer for term: {term}, annuity: {annutiy} and amount {amount} and interest rate: {rate}", message.Term, message.Annuity, message.Amount, message.InterestRate);

                result = _calculatorProvider.Calculate(result, application, message.BundledComponents);
                RemoveDmnFields(result);

                try
                {
                    await _auditClient.WriteLogEntry(AuditLogEntryAction.Execute, AuditLogEntryStatus.Success, "application", application.ApplicationNumber, "Calculating offer for term: " + message.Term + ", annuity: " + message.Annuity + " and amount " + message.Amount, new { });

                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Audit error in UpdateApplicationStatusCommandHandler");
                }

                return result;
            }
            catch (MaxNumberOfIterationsException e)
            {
                _logger.LogInformation("Error in calculation: {calcErr}. Command data: {command}", e.Message, message);
                throw e;
            }
            catch (Exception exp)
            {
                _logger.LogError(exp, "Exception while calculating offer for data, term: {term}, annuity: {annutiy} and amount {amount} and interest rate: {rate}", message.Term, message.Annuity, message.Amount, message.InterestRate);
                throw new NotImplementedException(exp.Message);
            }
        }

        /*private ArrangementRequest SimpleLoanCalculation(ArrangementRequest result, InitiateCalculateOfferCommand message)
        {
            try
            {
                var basicCalculation = Mapper.Map<InitiateCalculateOfferCommand, SimpleLoanCalculationRequest>(message);
                var conversionMethod = _configurationService.GetEffective("offer/exposure/currency-conversion-method", "Buy to middle").Result;
                basicCalculation.FeeCurrencyConversionMethod = conversionMethod;
                if (basicCalculation.InstallmentSchedule.FrequencyPeriod == 0)
                {
                    basicCalculation.InstallmentSchedule.FrequencyPeriod = 1;
                    basicCalculation.InstallmentSchedule.FrequencyUnitOfTime = SimpleUnitOfTime.M;
                }

                if (message.Amount > 0 && !string.IsNullOrEmpty(message.Term))
                {
                    basicCalculation.CalculationTarget = CalculationTarget.Annuity;
                }
                else if (message.Amount > 0 && message.Annuity > 0)
                {
                    basicCalculation.CalculationTarget = CalculationTarget.Term;
                    basicCalculation.NumberOfInstallments = 0;
                }
                else
                {
                    basicCalculation.CalculationTarget = CalculationTarget.Amount;
                }

                var resultBasicCalculation = InstallmentPlanCalculation.CalculateInstallmentPlan(basicCalculation);
                if (result is TermLoanRequest termLoanRequest && resultBasicCalculation != null)
                {
                    message.ProductConditions.InterestRates.Where(i => i.Currencies.Contains(message.Currency)).ToList().ForEach((interest) =>
                    {
                        termLoanRequest.Napr += interest?.CalculatedRate ?? 0;
                    });
                    termLoanRequest.Annuity = resultBasicCalculation.Annuity;
                    termLoanRequest.Amount = resultBasicCalculation.Amount;
                    termLoanRequest.Eapr = resultBasicCalculation.APR;
                    termLoanRequest.InstallmentPlan = resultBasicCalculation.Rows;
                    termLoanRequest.NumberOfInstallments = resultBasicCalculation.NumberOfInstallments;
                    termLoanRequest.InstallmentPlan = resultBasicCalculation.Rows;
                    termLoanRequest.RepaymentType = basicCalculation.RepaymentType;
                    termLoanRequest.InstallmentScheduleDayOfMonth = basicCalculation.InstallmentSchedule?.DayOfMonth ?? 1;
                    if (basicCalculation.CalculationTarget == CalculationTarget.Term)
                    {
                        termLoanRequest.Term = resultBasicCalculation.NumberOfInstallments.ToString();
                    }
                    RemoveDmnFields(termLoanRequest);
                    return termLoanRequest;
                }

                if (result is CreditCardFacilityRequest creditCardFacilityRequest && resultBasicCalculation != null)
                {
                    message.ProductConditions.InterestRates.Where(i => i.Currencies.Contains(message.Currency)).ToList().ForEach((interest) =>
                    {
                        creditCardFacilityRequest.Napr += interest?.CalculatedRate ?? 0;
                    });
                    creditCardFacilityRequest.Amount = resultBasicCalculation.Amount;
                    creditCardFacilityRequest.Eapr = resultBasicCalculation.APR;
                    creditCardFacilityRequest.InstallmentPlan = resultBasicCalculation.Rows;
                    creditCardFacilityRequest.NumberOfInstallments = resultBasicCalculation.NumberOfInstallments;
                    creditCardFacilityRequest.InstallmentPlan = resultBasicCalculation.Rows;
                    creditCardFacilityRequest.RepaymentType = basicCalculation.RepaymentType;
                    creditCardFacilityRequest.InstallmentScheduleDayOfMonth = basicCalculation.InstallmentSchedule?.DayOfMonth ?? 1;
                    if (basicCalculation.CalculationTarget == CalculationTarget.Term)
                    {
                        creditCardFacilityRequest.Term = resultBasicCalculation.NumberOfInstallments.ToString();
                    }
                    RemoveDmnFields(creditCardFacilityRequest);
                    return creditCardFacilityRequest;
                }

                if (result is OverdraftFacilityRequest overdraftFacilityRequest && resultBasicCalculation != null)
                {
                    message.ProductConditions.InterestRates.Where(i => i.Currencies.Contains(message.Currency)).ToList().ForEach((interest) =>
                    {
                        overdraftFacilityRequest.Napr += interest?.CalculatedRate ?? 0;
                    });
                    overdraftFacilityRequest.Amount = resultBasicCalculation.Amount;
                    overdraftFacilityRequest.Eapr = resultBasicCalculation.APR;
                    overdraftFacilityRequest.InstallmentPlan = resultBasicCalculation.Rows;
                    overdraftFacilityRequest.NumberOfInstallments = resultBasicCalculation.NumberOfInstallments;
                    overdraftFacilityRequest.InstallmentPlan = resultBasicCalculation.Rows;
                    overdraftFacilityRequest.RepaymentType = basicCalculation.RepaymentType;
                    overdraftFacilityRequest.InstallmentScheduleDayOfMonth = basicCalculation.InstallmentSchedule?.DayOfMonth ?? 1;
                    if (basicCalculation.CalculationTarget == CalculationTarget.Term)
                    {
                        overdraftFacilityRequest.Term = resultBasicCalculation.NumberOfInstallments.ToString();
                    }
                    RemoveDmnFields(overdraftFacilityRequest);
                    return overdraftFacilityRequest;
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception Simple Calculation for data, term: {term}, annuity: {annutiy} and amount {amount} and interest rate: {rate}", message.Term, message.Annuity, message.Amount, message.InterestRate);
                return null;
            }
        }*/

        private void RemoveDmnFields(ArrangementRequest request)
        {
            var snapshot = request.ProductSnapshot;
            if (snapshot.Conditions?.Fees != null)
            {
                foreach (var fee in snapshot.Conditions.Fees)
                {
                    fee.VariationsDefinitionDMN = null;
                }
            }
            if (snapshot.Conditions?.InterestRates != null)
            {
                foreach (var rate in snapshot.Conditions.InterestRates)
                {
                    rate.VariationsDefinitionDMN = null;
                    rate.LowerLimitVariationsDefinitionDMN = null;
                    rate.UpperLimitVariationsDefinitionDMN = null;
                }
            }
            request.ProductSnapshot = snapshot;
        }
    }

    public class InitiateCalculateOfferIdentifiedCommandHandler : IdentifiedCommandHandler<InitiateCalculateOfferCommand, ArrangementRequest>
    {
        public InitiateCalculateOfferIdentifiedCommandHandler(IMediator mediator, IRequestManager requestManager) : base(mediator, requestManager)
        {

        }

        protected override ArrangementRequest CreateResultForDuplicateRequest()
        {
            return null;
        }
    }
}
