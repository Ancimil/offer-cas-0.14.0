using CalculationService.Calculations;
using CalculationService.Models;
using MicroserviceCommon.Services;
using Offer.Domain.AggregatesModel.ApplicationAggregate;
using PriceCalculation.Calculations;
using PriceCalculation.Models.Pricing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CalculateInstallmentPlanRequestCS = CalculationService.Models.CalculateInstallmentPlanRequest;

namespace Offer.Domain.Calculations
{
    public class CalculationServiceCalculator
    {
        public readonly Calculator _calculator;
        private readonly IConfigurationService _configurationService;
        private readonly OfferPriceCalculation _offerPriceCalculation;

        public CalculationServiceCalculator(
            Calculator calculator,
            IConfigurationService configurationService,
            OfferPriceCalculation offerPriceCalculation)
        {
            _calculator = calculator ?? throw new ArgumentNullException(nameof(calculator));
            _configurationService = configurationService ?? throw new ArgumentNullException(nameof(configurationService));
            _offerPriceCalculation = offerPriceCalculation ?? throw new ArgumentNullException(nameof(offerPriceCalculation));
        }

        public async Task<ArrangementRequest> CalculateAsCalculationService(
            ArrangementRequest arrangementRequest, PriceCalculationParameters priceCalculationParameters)
        {
            List<PricedScheduledPeriod> pricedAndScheduledPeriods = null;
            if (priceCalculationParameters.ScheduledPeriods != null)
            {
                // var conversionMethod = await _configurationService.GetEffective("offer/fee-currency-conversion-method", "Buy to middle");
                pricedAndScheduledPeriods = SchedulingPeriodsResolver.PricePeriods(arrangementRequest,
                    priceCalculationParameters, _offerPriceCalculation);
            }

            // Perform price calculation for main conditions (whole repayment period)
            List<ScheduledPeriod> scheduledPeriods = new List<ScheduledPeriod>();
            var hasPeriods = priceCalculationParameters.ScheduledPeriods != null && priceCalculationParameters.ScheduledPeriods.Count() > 0;
            if (hasPeriods)
            {
                priceCalculationParameters.ScheduledPeriods.ForEach(p => scheduledPeriods.Add(p));
                priceCalculationParameters.ScheduledPeriods.Clear();
            }
            _ = await _offerPriceCalculation.CalculatePrice(arrangementRequest, priceCalculationParameters);
            if (hasPeriods)
            {
                priceCalculationParameters.ScheduledPeriods = scheduledPeriods;
            }

            var request = new CalculateInstallmentPlanRequestCS
            {
                Amount = (double)(arrangementRequest is FinanceServiceArrangementRequest fR ? fR.Amount : 0),
                RegularInterestPercentage = (double)(arrangementRequest?.Conditions?.InterestRates?.Where(r => r.Kind == InterestRateKinds.RegularInterest && string.IsNullOrEmpty(r.Periods)).Select(r => r.CalculatedRate).FirstOrDefault() ?? 0),
                // todo check
                StartDate = priceCalculationParameters.RequestDate,
                Currency = arrangementRequest is FinanceServiceArrangementRequest fRc ? GetCurrencyCode(fRc.Currency) : "978",
                RegularInterestUnitOfTime = CalculationService.Services.SimpleUnitOfTime.Y,
                Term = arrangementRequest is FinanceServiceArrangementRequest fRt ? fRt.Term : "0",
                Periods = pricedAndScheduledPeriods
            };

            request = AppendFees(request, arrangementRequest);
            request = AppendPredefinedPeriods(request, arrangementRequest);

            var plan = _calculator.CalculateInstallmentPlan(request);
            arrangementRequest.CalculationDate = priceCalculationParameters.RequestDate;
            arrangementRequest.NumberOfInstallments = (int)(plan.NumberOfInstallments ?? 0);
            arrangementRequest.InstallmentPlan = AggregatesModel.ApplicationAggregate.InstallmentPlanRow.FromInstallmentPlanCSList(plan.Installments);
            // TODO Think about this casts
            if (arrangementRequest is FinanceServiceArrangementRequest fSR)
            {
                fSR.Eapr = (decimal)(plan.EffectiveInterestRate ?? 0);
                if (arrangementRequest is TermLoanRequest trl)
                {
                    trl.Annuity = (decimal)(plan.Annuity ?? 0);
                }
            }
            return arrangementRequest;
        }

        private CalculateInstallmentPlanRequestCS AppendFees(CalculateInstallmentPlanRequestCS request, ArrangementRequest arrangementRequest)
        {
            // TODO Trigger price presentation before appending fees
            var fees = arrangementRequest?.Conditions.Fees;

            var originationFee = fees.FirstOrDefault(f => f.Kind == FeeConditionKind.OriginationFee);
            if (originationFee != null)
            {
                request.OriginationFeeFixedAmount = (double)originationFee.CalculatedFixedAmount;
                // request.OriginationFeeFixedAmountLcl ???
                request.OriginationFeeLowerLimit = (double)originationFee.CalculatedLowerLimit;
                //request.OriginationFeeLowerLimitLclValAmount
                request.OriginationFeeUpperLimit = (double)originationFee.CalculatedUpperLimit;
                //request.OriginationFeeUpperLimitLclValAmount
                request.OriginationFeePercentage = (double)originationFee.CalculatedPercentage;
                //request.OriginationFeeCapitalization
            }

            var managementFee = fees.FirstOrDefault(f => f.Kind == FeeConditionKind.ManagementFee);
            if (managementFee != null)
            {
                // TODO resolve this fields
                //request.ExcludeManagementFeeFromEAPR
                //request.FirstManagementFeeDate
                //request.IncludeManagementFeeInAnnuity
                switch (managementFee.Frequency)
                {
                    case FeeConditionFrequency.Monthly:
                        request.ManagementFeeCalculationFrequencyPeriod = 1;
                        break;
                    case FeeConditionFrequency.Quarterly:
                        request.ManagementFeeCalculationFrequencyPeriod = 3;
                        break;
                    case FeeConditionFrequency.Semiyearly:
                        request.ManagementFeeCalculationFrequencyPeriod = 6;
                        break;
                    case FeeConditionFrequency.Yearly:
                        request.ManagementFeeCalculationFrequencyPeriod = 12;
                        break;
                    default:
                        // TODO What to do if it is event trigered or something similar? (invalid data so maybe throw an error?s)
                        request.ManagementFeeCalculationFrequencyPeriod = 12;
                        break;
                }
                request.ManagementFeeCalculationFrequencyUnitOfTime = CalculationService.Services.SimpleUnitOfTime.M;
                request.ManagementFeeLowerLimit = (double)managementFee.CalculatedLowerLimit;
                request.ManagementFeePercentage = (double)managementFee.CalculatedPercentage;
            }
            return request;
        }

        private CalculateInstallmentPlanRequestCS AppendPredefinedPeriods(
            CalculateInstallmentPlanRequestCS request, ArrangementRequest arrangementRequest)
        {
            // TODO Chech this hardcoded cast
            if (arrangementRequest is TermLoanRequest termLoanRequest)
            {
                if (termLoanRequest.GracePeriod != null && termLoanRequest.GracePeriodStartDate != null)
                {
                    request.GracePeriodEnd = Utility.GetEndDateFromPeriod(termLoanRequest.GracePeriod, termLoanRequest.GracePeriodStartDate);
                }
                if (termLoanRequest.DrawdownPeriod != null && termLoanRequest.DrawdownPeriodStartDate != null)
                {
                    request.DrawdownPeriodEnd = Utility.GetEndDateFromPeriod(termLoanRequest.DrawdownPeriod, termLoanRequest.DrawdownPeriodStartDate);
                }
            }
            return request;
        }

        private string GetCurrencyCode(string currency)
        {
            // TODO Call to Reference API
            if (currency == "EUR")
            {
                return "978";
            }
            else if (currency == "RSD")
            {
                return "941";
            }
            else
            {
                return "978";
            }
        }
    }
}
