using CalculationService.Models;
using Offer.Domain.AggregatesModel.ApplicationAggregate;
using Offer.Domain.AggregatesModel.Calculations;
using PriceCalculation.Models.Lifecycle;
using PriceCalculation.Models.Pricing;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Offer.Domain.Calculations
{
    public class SchedulingPeriodsResolver
    {
        public static List<ScheduledPeriod> SchedulePeriods(ResolveSchedulingPeriodsRequest request)
        {
            var scheduledPeriods = new List<ScheduledPeriod>();

            foreach (var period in request.SchedulingPeriods)
            {
                if (string.IsNullOrEmpty(period.MaximalLength))
                {
                    continue;
                }
                var startDate = GetPeriodDateTimeFromEvent(period.StartEvent, request);
                if (startDate.HasValue)
                {
                    continue;
                }
                if (!string.IsNullOrEmpty(period.StartOffset))
                {
                    startDate += TimeSpan.Parse(period.StartOffset);
                }
                var endDate = startDate + TimeSpan.Parse(period.MaximalLength);
                scheduledPeriods.Add(new ScheduledPeriod
                {
                    PeriodType = period.PeriodType,
                    StartDate = startDate.Value,
                    EndDate = endDate.Value
                });
            }

            return scheduledPeriods;
        }

        /*public static List<PricedScheduledPeriod> ScheduleAndPricePeriods(ResolveSchedulingPeriodsRequest request,
            ArrangementRequest arrangementRequest, PriceCalculationParameters priceParams, OfferPriceCalculation priceCalc,
            string conversionMethod)
        {
            var scheduledAndPricedPeriods = new List<PricedScheduledPeriod>();
            ArrangementRequest arrangementRequestTemp;
            foreach (var period in priceParams.ScheduledPeriods)
            {
                arrangementRequestTemp = Mapper.Map<ArrangementRequest, ArrangementRequest>(arrangementRequest);
                arrangementRequestTemp.CalculateOffer(priceParams, priceCalc, conversionMethod);
                scheduledAndPricedPeriods.Add(new PricedScheduledPeriod
                {
                    Percentage = (double)(arrangementRequestTemp?.Conditions?.InterestRates?.Where(r => r.Kind == InterestRateKinds.RegularInterest).Select(r => r.CalculatedRate).FirstOrDefault() ?? 0),
                    PeriodType = period.PeriodType,
                    StartDate = period.StartDate,
                    EndDate = period.EndDate,
                    // TODO To be resolved from DMN maybe?
                    UnitOfTime = CalculationService.Services.SimpleUnitOfTime.Y
                });
            }
            return scheduledAndPricedPeriods;
        }*/

        public static List<PricedScheduledPeriod> PricePeriods(
            ArrangementRequest arrangementRequest, PriceCalculationParameters priceParams,
            OfferPriceCalculation priceCalc)
        {
            var scheduledAndPricedPeriods = new List<PricedScheduledPeriod>();
            var scheduledPeriods = new List<ScheduledPeriod>();
            var rates = new List<InterestRateCondition>();
            if (priceParams.ScheduledPeriods != null)
            {
                priceParams.ScheduledPeriods.ForEach(p => scheduledPeriods.Add(p));
            }
            if (priceParams.InterestRates != null)
            {
                priceParams.InterestRates.ForEach(r => rates.Add(r));
            }
            foreach (var period in scheduledPeriods)
            {
                priceParams.ScheduledPeriods.Clear();
                priceParams.ScheduledPeriods.Add(period);
                _ = priceCalc.CalculatePrice(arrangementRequest, priceParams).Result;
                // arrangementRequest.CalculateOffer(priceParams, priceCalc, conversionMethod);
                scheduledAndPricedPeriods.Add(new PricedScheduledPeriod
                {
                    Percentage = (double)(arrangementRequest?.Conditions?.InterestRates?.Where(r => r.Kind == InterestRateKinds.RegularInterest).Select(r => r.CalculatedRate).FirstOrDefault() ?? 0),
                    PeriodType = period.PeriodType,
                    StartDate = period.StartDate,
                    EndDate = period.EndDate,
                    // TODO To be resolved from DMN maybe?
                    UnitOfTime = CalculationService.Services.SimpleUnitOfTime.Y
                });
            }
            if (scheduledPeriods != null)
            {
                priceParams.ScheduledPeriods = scheduledPeriods;
            }
            if (rates != null)
            {
                priceParams.InterestRates = rates;
            }
            return scheduledAndPricedPeriods;
        }

        private static DateTime? GetPeriodDateTimeFromEvent(LifecycleEvent @event, ResolveSchedulingPeriodsRequest request)
        {
            switch (@event)
            {
                case LifecycleEvent.RequestDate:
                    return request.RequestDate ?? request.CalculationDate;
                case LifecycleEvent.ContractSigned:
                    return request.SigningDate ?? (request.RequestDate ?? request.CalculationDate).Value.AddMonths(1);
                case LifecycleEvent.Approved:
                    return request.ApprovalDate ?? (request.RequestDate ?? request.CalculationDate).Value.AddMonths(1);
                case LifecycleEvent.FundsDisbursed:
                    return request.DisbursmentDate ?? (request.RequestDate ?? request.CalculationDate).Value.AddMonths(1);
                case LifecycleEvent.FirstInstallmentDate:
                    return request.FirstInstallmentDate ?? (request.DisbursmentDate.HasValue ? (DateTime?)request.DisbursmentDate.Value.AddMonths(1) : null) ??
                        (request.RequestDate ?? request.CalculationDate).Value.AddMonths(2);
                case LifecycleEvent.MaturityDate:
                    return request.MaturityDate ?? (request.DisbursmentDate.HasValue ? (DateTime?)request.DisbursmentDate.Value.AddMonths(12) : null) ??
                        (request.RequestDate ?? request.CalculationDate).Value.AddMonths(13);
                default:
                    return null;
            }
        }
    }
}
