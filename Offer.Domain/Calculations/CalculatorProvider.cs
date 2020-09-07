using AutoMapper;
using MicroserviceCommon.Services;
using Newtonsoft.Json.Linq;
using Offer.Domain.AggregatesModel.ApplicationAggregate;
using PriceCalculation.Models.Pricing;
using System;
using System.Collections.Generic;
using System.Linq;
using static Offer.Domain.Calculations.InstallmentPlanCalculation;

namespace Offer.Domain.Calculations
{
    public class CalculatorProvider
    {
        public readonly IServiceProvider _serviceProvider;
        private readonly IConfigurationService _configurationService;

        public CalculatorProvider(IServiceProvider serviceProvider, IConfigurationService configurationService)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _configurationService = configurationService ?? throw new ArgumentNullException(nameof(configurationService));
        }

        public ArrangementRequest Calculate(ArrangementRequest arrangementRequest, Application application = null,
            IDictionary<string, JToken> additionalProperties = null)
        {
            var priceCalculationParameters = arrangementRequest.GetPriceCalculationParameters(application);
            if (additionalProperties != null && additionalProperties.Keys.Count() > 0)
            {
                priceCalculationParameters.AdditionalProperties = priceCalculationParameters.AdditionalProperties ?? new Dictionary<string, JToken>();
                priceCalculationParameters.AdditionalProperties = priceCalculationParameters.AdditionalProperties
                    .Concat(additionalProperties
                            .Where(k => !priceCalculationParameters.AdditionalProperties.ContainsKey(k.Key))
                            .ToDictionary(k => k.Key, v => v.Value))
                    .ToDictionary(k => k.Key, v => v.Value);
            }
            var calculationEngine = _configurationService.GetEffective("offer/calculation-engine", "basic").Result;
            switch (calculationEngine)
            {
                case "basic":
                    return CalculateAsBasic(arrangementRequest, priceCalculationParameters);
                case "calculation-service":
                    try
                    {
                        var calcServiceCalculator = (CalculationServiceCalculator)_serviceProvider.GetService(typeof(CalculationServiceCalculator));
                        return calcServiceCalculator.CalculateAsCalculationService(arrangementRequest, priceCalculationParameters).Result;
                    }
                    catch (Exception e)
                    {
                        throw e;
                    }
                case "simple-calculation":
                    return CalculateBySimpleCalculation(arrangementRequest, priceCalculationParameters);
                default:
                    return CalculateAsBasic(arrangementRequest, priceCalculationParameters);
            }
        }

        private ArrangementRequest CalculateAsBasic(ArrangementRequest arrangementRequest, PriceCalculationParameters priceCalculationParameters)
        {
            var priceCalc = (OfferPriceCalculation)_serviceProvider.GetService(typeof(OfferPriceCalculation));
            var conversionMethod = _configurationService.GetEffective("offer/fee-currency-conversion-method", "Buy to middle").Result;
            arrangementRequest.CalculateOffer(priceCalculationParameters, priceCalc, conversionMethod);
            return arrangementRequest;
        }

        private ArrangementRequest CalculateBySimpleCalculation(ArrangementRequest request, PriceCalculationParameters priceCalculationParameters)
        {
            if (!(request is FinanceServiceArrangementRequest))
            {
                return request;
            }
            var priceCalc = (OfferPriceCalculation)_serviceProvider.GetService(typeof(OfferPriceCalculation));
            var priceArrangementRequest = priceCalc.CalculatePriceBySimpleCalculation(request, priceCalculationParameters).Result;
            request.Conditions = priceArrangementRequest.Conditions;
            priceCalculationParameters.InterestRates = request.Conditions.InterestRates;
            priceCalculationParameters.Fees = request.Conditions.Fees;
            priceCalculationParameters.OtherConditions = request.Conditions.Other;
            var basicCalculation = Mapper.Map<ArrangementRequest, SimpleLoanCalculationRequest>(request);
            basicCalculation.Fees = new List<FeeEntry>();
            basicCalculation.StardDate = new DateTime(basicCalculation.StardDate.Year, basicCalculation.StardDate.Month, basicCalculation.StardDate.Day, 0, 0, 0);
            priceCalculationParameters.Fees.ForEach(x =>
            {
                basicCalculation.Fees.Add(new FeeEntry
                {
                    Kind = x.Kind,
                    Name = x.Title,
                    Frequency = x.Frequency,
                    Percentage = x.Percentage,
                    Currency = x.FixedAmount?.Code ?? "",
                    FixedAmount = x.FixedAmount?.Amount ?? 0,
                    LowerLimit = x.LowerLimit?.Amount ?? 0,
                    UpperLimit = x.UpperLimit?.Amount ?? 0,
                    ServiceCode = x.ServiceCode,
                    Date = basicCalculation.StardDate,
                    CalculationBasisType = CalculationBasisType.AccountBalance
                });
            });

            basicCalculation.RegularInterest = priceCalculationParameters.InterestRates
                .Where(i => i.Currencies.Contains(priceCalculationParameters.Currency) && i.Kind == InterestRateKinds.RegularInterest)
                .Select(x => new InterestRateEntry
                {
                    Date = basicCalculation.StardDate,
                    RatePercentage = (double)x.CalculatedRate,
                    IsCompound = x.IsCompound,
                    CalendarBasis = x.CalendarBasis,
                    Name = x.Title,
                    RateUnitOfTime = Domain.Calculations.SimpleUnitOfTime.Y
                })
                .ToList();

            if (basicCalculation.RegularInterest.Count == 0)
            {
                throw new NotImplementedException("Interest rate is not in product currency");
            }

            var conversionMethod = _configurationService.GetEffective("offer/exposure/currency-conversion-method", "Buy to middle").Result;
            basicCalculation.FeeCurrencyConversionMethod = conversionMethod;

            if (basicCalculation.InstallmentSchedule.FrequencyPeriod == 0)
            {
                basicCalculation.InstallmentSchedule.FrequencyPeriod = 1;
                basicCalculation.InstallmentSchedule.FrequencyUnitOfTime = SimpleUnitOfTime.M;
            }

            if (basicCalculation.Amount > 0 && basicCalculation.Annuity > 0 && basicCalculation.NumberOfInstallments == 0)
            {
                basicCalculation.CalculationTarget = CalculationTarget.Term;
                basicCalculation.NumberOfInstallments = 0;
            }
            else if (basicCalculation.Annuity > 0 && basicCalculation.NumberOfInstallments > 0 && basicCalculation.Amount == 0)
            {
                basicCalculation.CalculationTarget = CalculationTarget.Amount;
            }
            else
            {
                basicCalculation.CalculationTarget = CalculationTarget.Annuity;
                basicCalculation.Annuity = 0;
            }


            var adjustFirstInst = _configurationService.GetEffective("offer/calculation/adjust-first-installment", "false").Result;
            basicCalculation.AdjustFirstInstallment = Boolean.Parse(adjustFirstInst);

            var resultBasicCalculation = CalculateInstallmentPlan(basicCalculation);
            if (request is TermLoanRequest termLoanRequest && resultBasicCalculation != null)
            {
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
            }
            else if (request is CreditCardFacilityRequest creditCardFacilityRequest && resultBasicCalculation != null)
            {
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
            }
            else if (request is OverdraftFacilityRequest overdraftFacilityRequest && resultBasicCalculation != null)
            {
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
            }

            return request;
        }
    }
}
