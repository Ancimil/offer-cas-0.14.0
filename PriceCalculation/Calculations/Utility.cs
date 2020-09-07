using AutoMapper;
using PriceCalculation.Exceptions;
using PriceCalculation.Models.Pricing;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PriceCalculation.Calculations
{
    public static class Utility
    {
        public static T Copy<T>(this T source)
        {
            T copy = default(T);
            var config = new MapperConfiguration(cfg => {
                cfg.CreateMap<T, T>();
            });
            IMapper mapper = config.CreateMapper();
            copy = mapper.Map<T, T>(source);
            return copy;
        }

        public static int GetMonthsFromPeriod(string period)
        {
            var numberOfDaysInMonthPerYear = 365.25 / 12;
            return Convert.ToInt32(GetDaysFromPeriod(period) / numberOfDaysInMonthPerYear);
        }

        public static int GetDaysFromPeriod(string period, DateTime? startingDate = null)
        {
            return Convert.ToInt32(GetTimeSpanFromPeriod(period, startingDate).TotalDays);
        }

        public static DateTime GetEndDateFromPeriod(string period, DateTime? startingDate = null)
        {
            return (startingDate ?? DateTime.Today) + GetTimeSpanFromPeriod(period, startingDate);
        }

        public static int DaysBetween(DateTime from, DateTime? to = null)
        {
            var toDate = to ?? DateTime.Today;
            return Convert.ToInt32((toDate - from).TotalDays);
        }

        public static int MonthsBetween(DateTime from, DateTime? to = null)
        {
            return Convert.ToInt16(DaysBetween(from, to));
        }

        public static int SumPeriodsToDays(List<string> periods)
        {
            return periods.Select(p => GetDaysFromPeriod(p)).Sum();
        }

        public static int SumPeriodsToMonthsays(List<string> periods)
        {
            return Convert.ToInt16(SumPeriodsToDays(periods));
        }

        public static TimeSpan GetTimeSpanFromPeriod(string period, DateTime? startingDate = null)
        {
            startingDate = startingDate ?? DateTime.Today;
            int days = 0;
            int months = 0;
            int years = 0;
            bool isValidPeriod = false;
            if (!string.IsNullOrEmpty(period))
            {
                period = period.Replace("P", ""); // delete "P" from start
                int yearPosition = period.IndexOf("Y");
                if (yearPosition > 0)
                {
                    years = Convert.ToInt16(period.Substring(0, yearPosition));
                    isValidPeriod = true;
                    if (period.Length > yearPosition + 1)
                    {
                        period = period.Substring(yearPosition + 1);
                    }
                }
                int monthPosition = period.IndexOf("M");
                if (monthPosition > 0)
                {
                    isValidPeriod = true;
                    months = Convert.ToInt16(period.Substring(0, monthPosition));
                    if (period.Length > monthPosition + 1)
                    {
                        period = period.Substring(monthPosition + 1);
                    }
                }
                int dayPosition = period.IndexOf("D");
                if (dayPosition > 0)
                {
                    isValidPeriod = true;
                    days = Convert.ToInt16(period.Substring(0, dayPosition));
                }
                if (yearPosition == -1 && monthPosition == -1 && dayPosition == -1)
                {
                    isValidPeriod = true;
                    months = Convert.ToInt16(period);
                }

            }
            if (!isValidPeriod && days == 0 && months == 0 && years == 0)
            {
                throw new InvalidTermException("Term is invalid. Current value: " + period ?? "null");
            }
            return (startingDate.Value.AddYears(years).AddMonths(months).AddDays(days) - startingDate).Value;
        }

        public static List<InterestRateCondition> MergeRates(List<InterestRateCondition> mergeToRates,
            List<InterestRateCondition> mergeFromRates)
        {
            if (mergeFromRates == null || mergeToRates == null || mergeToRates.Count == 0)
            {
                return mergeToRates;
            }
            foreach (var to in mergeToRates)
            {
                var mergeFrom = mergeFromRates.Where(f => f.CorrespondsTo(to)).FirstOrDefault();
                if (mergeFrom != null)
                {
                    if (mergeFrom.Variations != null && mergeFrom.Variations.Count > 0)
                    {
                        to.Variations = to.Variations ?? new List<InterestRateVariation>();
                        to.Variations.AddRange(mergeFrom.Variations);
                    }
                    if (mergeFrom.UpperLimitVariations != null && mergeFrom.UpperLimitVariations.Count > 0)
                    {
                        to.UpperLimitVariations = to.UpperLimitVariations ?? new List<InterestRateVariation>();
                        to.UpperLimitVariations.AddRange(mergeFrom.UpperLimitVariations);
                    }
                    if (mergeFrom.LowerLimitVariations != null && mergeFrom.LowerLimitVariations.Count > 0)
                    {
                        to.LowerLimitVariations = to.LowerLimitVariations ?? new List<InterestRateVariation>();
                        to.LowerLimitVariations.AddRange(mergeFrom.LowerLimitVariations);
                    }
                }
            }
            return mergeToRates;
        }

        public static List<FeeCondition> MergeFees(List<FeeCondition> mergeToFees, List<FeeCondition> mergeFromFees)
        {
            if (mergeFromFees == null || mergeToFees == null || mergeToFees.Count == 0)
            {
                return mergeToFees;
            }
            foreach (var to in mergeToFees)
            {
                var mergeFrom = mergeFromFees.Where(f => f.CorrespondsTo(to)).FirstOrDefault();
                if (mergeFrom != null)
                {
                    if (mergeFrom.Variations != null && mergeFrom.Variations.Count > 0)
                    {
                        to.Variations = to.Variations ?? new List<FeeVariation>();
                        to.Variations.AddRange(mergeFrom.Variations);
                    }
                }
            }
            return mergeToFees;
        }
    }
}
