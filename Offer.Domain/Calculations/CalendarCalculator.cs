using System;
using System.Collections.Generic;
using System.Linq;

namespace Offer.Domain.Calculations
{
    internal static class CalendarCalculator
    {
        #region Public methods

        internal static IEnumerable<CalculationOccurrence> CalculateOccurrences(CalculationTrigger trigger, DateTime startDate, DateTime? endDate)
        {
            if (trigger == null)
            {
                throw new ArgumentNullException("trigger");
            }

            List<CalculationTrigger> triggers = new List<CalculationTrigger>() { trigger };

            return CalculateOccurrences(triggers, startDate, endDate);
        }

        internal static IEnumerable<CalculationOccurrence> CalculateOccurrences(IEnumerable<CalculationTrigger> triggers, DateTime startDate, DateTime? endDate)
        {
            IEnumerable<CalculationOccurrence> calculatedOccurences = new List<CalculationOccurrence>();

            if (triggers == null || triggers.Count() == 0)
            {
                return calculatedOccurences;
            }

            // 1. Calculate occurrences
            calculatedOccurences = GetOccurrencesWithRecurrenceEvaluation(triggers, startDate, endDate);

            //2. Fix occurrences that are scheduled on non-working days
            ResolveNonWorkingDaysResolutionMethod(calculatedOccurences, startDate, endDate);

            return calculatedOccurences;
        }

        internal static IEnumerable<DateTime> CalculateOccurrenceDates(CalculationTrigger trigger, DateTime startDate, DateTime? endDate)
        {
            //var occurrences = GetOccurrencesWithRecurrenceEvaluation(new Collection<CalculationTrigger> { trigger }, startDate, endDate);

            var occurrences = CalculateOccurrences(new List<CalculationTrigger> { trigger }, startDate, endDate);

            if (occurrences == null)
            {
                return new List<DateTime>();
            }
            else
            {
                return occurrences.Select(it => it.ScheduledDate).ToList();
            }
        }

        internal static IEnumerable<CalculationOccurrence> GetOccurrencesWithRecurrenceEvaluation(CalculationTrigger trigger, DateTime startDate, DateTime? endDate)
        {
            if (trigger == null)
            {
                throw new ArgumentNullException("trigger");
            }

            List<CalculationTrigger> events = new List<CalculationTrigger>() { trigger };

            return GetOccurrencesWithRecurrenceEvaluation(events, startDate, endDate);
        }

        internal static IEnumerable<CalculationOccurrence> GetOccurrencesWithRecurrenceEvaluation(IEnumerable<CalculationTrigger> triggers, DateTime startDate, DateTime? endDate)
        {
            List<CalculationOccurrence> occurrencesTotal = new List<CalculationOccurrence>();

            if (triggers == null || triggers.Count() == 0)
            {
                return occurrencesTotal;
            }

            foreach (var trigger in triggers)
            {
                if (trigger == null)
                {
                    continue;
                }


                if (trigger.ScheduleRecurrenceType == ScheduleRecurrenceType.RecurrenceWithLimitedPeriod ||
                    trigger.ScheduleRecurrenceType == ScheduleRecurrenceType.RecurrenceWithLimitedOccurrences)
                {
                    if (endDate.HasValue == false)
                    {
                        throw new ArgumentException("EndDate must not be null if ScheduleRecurrenceType is set to RecurrenceWithLimitedPeriod or RecurrenceWithLimitedOccurrences.");
                    }
                }

                if (endDate.HasValue == false)
                {
                    endDate = startDate.AddYears(100); // dummy end date;
                }

                var occurrences = GetDatesInternal(trigger, startDate, endDate.Value);

                if (trigger.Recurrence != null && occurrences != null && occurrences.Count() > 0)
                {
                    var lastOccurrence = occurrences.OrderBy(it => it.ScheduledDate.Date).Last();
                    trigger.Recurrence.OccurrencesCalculatedUpTo = lastOccurrence.ScheduledDate;
                }

                if (occurrences != null && occurrences.Count() > 0)
                {
                    occurrencesTotal.AddRange(occurrences);
                }
            }

            return occurrencesTotal;
        }

        #endregion

        #region NonPublic methods

        private static IEnumerable<CalculationOccurrence> GetDatesInternal(CalculationTrigger trigger, DateTime startDate, DateTime endDate)
        {
            List<CalculationOccurrence> dates = new List<CalculationOccurrence>();

            if (trigger == null)
                return dates;

            if (trigger.ScheduleRecurrenceType == ScheduleRecurrenceType.Custom)
            {
                if (trigger.Occurrences != null)
                    dates.AddRange(trigger.Occurrences);

                return dates;
            }

            // Fix dates
            trigger.TriggerStartDate = trigger.TriggerStartDate.Date;
            startDate = startDate.Date;
            endDate = endDate.Date;

            if (trigger.TriggerStartDate > startDate)
                startDate = trigger.TriggerStartDate;

            DateTime date = startDate;

            if (trigger.Recurrence == null || trigger.ScheduleRecurrenceType == ScheduleRecurrenceType.SingleOccurrence)
            {
                if (IsInRange(date, startDate, endDate) == false) return dates;
                dates.Add(CreateCalendarEventOccurrence(date, trigger));
                return dates;
            }

            if (trigger.Recurrence == null)
                return dates;

            if (trigger.Recurrence.IterationPeriodNumerator == 0)
                trigger.Recurrence.IterationPeriodNumerator = 1;

            long currentNumberOfOccurences = 0;

            switch (trigger.Recurrence.RecurrencePattern)
            {
                case RecurrencePattern.Daily:
                    #region Daily

                    if (trigger.Recurrence.DailyRecurrencePatternType.HasValue == false)
                        trigger.Recurrence.DailyRecurrencePatternType = DailyRecurrencePatternType.EveryNDays;

                    switch (trigger.Recurrence.DailyRecurrencePatternType.Value)
                    {
                        case DailyRecurrencePatternType.EveryNDays:

                            #region Every N Days
                            if (trigger.Recurrence.IterationPeriodNumerator > 0)
                            {
                                date = startDate;
                                while (ShouldCreateOccurrence(trigger.ScheduleRecurrenceType, date, endDate, currentNumberOfOccurences, trigger.Recurrence.MaxOccurances))
                                {
                                    dates.Add(CreateCalendarEventOccurrence(date, trigger));
                                    currentNumberOfOccurences++;
                                    date = date.AddDays(trigger.Recurrence.IterationPeriodNumerator);
                                }
                            }
                            else
                            {
                                date = endDate;
                                while (ShouldCreateOccurrence(trigger.ScheduleRecurrenceType, startDate, date, currentNumberOfOccurences, trigger.Recurrence.MaxOccurances))
                                {
                                    dates.Add(CreateCalendarEventOccurrence(date, trigger));
                                    currentNumberOfOccurences++;
                                    date = date.AddDays(trigger.Recurrence.IterationPeriodNumerator);
                                }
                            }
                            #endregion
                            break;

                        case DailyRecurrencePatternType.EveryWeekday:

                            #region Every week day

                            date = startDate;
                            while (ShouldCreateOccurrence(trigger.ScheduleRecurrenceType, date, endDate, currentNumberOfOccurences, trigger.Recurrence.MaxOccurances))
                            {
                                if (IsWorkingDay(date))
                                {
                                    dates.Add(CreateCalendarEventOccurrence(date, trigger));
                                    currentNumberOfOccurences++;
                                }
                                date = date.AddDays(1);
                            }

                            #endregion
                            break;

                        default:
                            throw new NotSupportedException(string.Format("DailyRecurrencePatternType {0} is not supported.", trigger.Recurrence.DailyRecurrencePatternType));
                    }

                    break;

                #endregion

                case RecurrencePattern.Weekly:
                    #region Weekly

                    if (trigger.Recurrence.DayOfWeek.HasValue == false)
                        throw new Exception("Value of Recurrence.DayOfWeek is not set, it is null.");

                    var multiplier = Math.Abs(trigger.Recurrence.IterationPeriodNumerator);

                    #region Create list of dates from flag enumeration daysOfWeek

                    var daysOfWeek = new List<DayOfWeek>();
                    foreach (DayOfWeek dayOfWeek in Enum.GetValues(typeof(DayOfWeek)))
                    {
                        if (trigger.Recurrence.DayOfWeek.HasValue &&
                            trigger.Recurrence.DayOfWeek.Value.HasFlag(dayOfWeek))
                        {
                            daysOfWeek.Add(dayOfWeek);
                        }
                    }

                    #endregion

                    #region Set first date

                    date = startDate;
                    if (!daysOfWeek.Where(a => a == date.DayOfWeek).Any())
                    {
                        if (daysOfWeek.Where(a => a > date.DayOfWeek).Any())
                        {
                            var currentDayOfWeek = daysOfWeek.Where(a => a > date.DayOfWeek).FirstOrDefault();
                            while (date.DayOfWeek != currentDayOfWeek) date = date.AddDays(1);
                        }
                        else
                        {
                            var currentDayOfWeek = daysOfWeek.FirstOrDefault();
                            date = date.AddDays(7 * multiplier);
                            while (date.DayOfWeek != currentDayOfWeek) date = date.AddDays(-1);
                        }
                    }

                    #endregion

                    #region Create And Add Create Occurrences
                    while (ShouldCreateOccurrence(trigger.ScheduleRecurrenceType, date, endDate, currentNumberOfOccurences, trigger.Recurrence.MaxOccurances))
                    {
                        dates.Add(CreateCalendarEventOccurrence(date, trigger));
                        currentNumberOfOccurences++;

                        if (daysOfWeek.Where(a => a > date.DayOfWeek).Any())
                        {
                            var currentDayOfWeek = daysOfWeek.Where(a => a > date.DayOfWeek).FirstOrDefault();
                            while (date.DayOfWeek != currentDayOfWeek) date = date.AddDays(1);
                        }
                        else
                        {
                            var currentDayOfWeek = daysOfWeek.FirstOrDefault();
                            date = date.AddDays(7 * multiplier);
                            while (date.DayOfWeek != currentDayOfWeek) date = date.AddDays(-1);
                        }
                    }
                    #endregion

                    break;
                #endregion

                case RecurrencePattern.Monthly:
                    #region Monthly

                    if (trigger.Recurrence.MonthlyRecurrencePatternType.HasValue == false)
                        trigger.Recurrence.MonthlyRecurrencePatternType = MonthlyRecurrencePatternType.DayOfMonth;

                    trigger.Recurrence.IterationPeriodNumerator = Math.Abs(trigger.Recurrence.IterationPeriodNumerator);

                    switch (trigger.Recurrence.MonthlyRecurrencePatternType.Value)
                    {
                        case MonthlyRecurrencePatternType.DayOfMonth:

                            #region Day Of Month

                            if (trigger.Recurrence.DayOfMonth.HasValue == false)
                            {
                                trigger.Recurrence.DayOfMonth = startDate.Day;
                                //throw new CalculationServiceException("Value of Recurrence.DayOfMonth is not set, it is null.");
                            }

                            #region Set first date

                            if (startDate.Day == trigger.Recurrence.DayOfMonth)
                            {
                                date = startDate;
                            }
                            else
                            {
                                int day = DateTime.DaysInMonth(date.Year, date.Month) < trigger.Recurrence.DayOfMonth.Value ? DateTime.DaysInMonth(date.Year, date.Month) : trigger.Recurrence.DayOfMonth.Value;
                                date = new DateTime(date.Year, date.Month, day);

                                while (date < startDate)
                                {
                                    date = date.AddMonths(1);
                                    day = DateTime.DaysInMonth(date.Year, date.Month) < trigger.Recurrence.DayOfMonth.Value ? DateTime.DaysInMonth(date.Year, date.Month) : trigger.Recurrence.DayOfMonth.Value;
                                    date = new DateTime(date.Year, date.Month, day);
                                }
                            }

                            #endregion

                            #region Create And Add Create Occurrences
                            while (ShouldCreateOccurrence(trigger.ScheduleRecurrenceType, date, endDate, currentNumberOfOccurences, trigger.Recurrence.MaxOccurances))
                            {
                                dates.Add(CreateCalendarEventOccurrence(date, trigger));
                                currentNumberOfOccurences++;

                                date = date.AddMonths(trigger.Recurrence.IterationPeriodNumerator);
                                int day = DateTime.DaysInMonth(date.Year, date.Month) < trigger.Recurrence.DayOfMonth.Value ? DateTime.DaysInMonth(date.Year, date.Month) : trigger.Recurrence.DayOfMonth.Value;
                                date = new DateTime(date.Year, date.Month, day);
                            }
                            #endregion

                            #endregion

                            break;

                        case MonthlyRecurrencePatternType.DayOfWeekOfMonth:

                            #region Day of week of month

                            if (trigger.Recurrence.DayOfWeek.HasValue == false)
                                throw new Exception("Value of Recurrence.DayOfWeek is not set, it is null.");

                            if (trigger.Recurrence.WeekOfMonth.HasValue == false)
                                throw new Exception("Value of Recurrence.WeekOfMonth is not set, it is null.");

                            #region Set first date

                            date = FindDayOfWeekOfMonth(trigger.Recurrence.DayOfWeek.GetValueOrDefault(),
                                                        (WeekOfMonth)trigger.Recurrence.WeekOfMonth,
                                                        new DateTime(startDate.Year, startDate.Month, 1));
                            if (date < startDate)
                            {
                                date = FindDayOfWeekOfMonth(trigger.Recurrence.DayOfWeek.GetValueOrDefault(),
                                                            (WeekOfMonth)trigger.Recurrence.WeekOfMonth,
                                                             new DateTime(date.Year, date.Month, 1).AddMonths(1));
                            }
                            #endregion

                            #region Create and add Occurence

                            while (ShouldCreateOccurrence(trigger.ScheduleRecurrenceType, date, endDate, currentNumberOfOccurences, trigger.Recurrence.MaxOccurances))
                            {
                                dates.Add(CreateCalendarEventOccurrence(date, trigger));
                                currentNumberOfOccurences++;
                                date = FindDayOfWeekOfMonth(trigger.Recurrence.DayOfWeek.GetValueOrDefault(),
                                                            (WeekOfMonth)trigger.Recurrence.WeekOfMonth,
                                                            new DateTime(date.Year, date.Month, 1).AddMonths(1));
                            }

                            #endregion

                            #endregion

                            break;

                        default:
                            throw new NotSupportedException(string.Format("MonthlyRecurrencePatternType {0} is not supported.", trigger.Recurrence.MonthlyRecurrencePatternType)); ;
                    }

                    break;

                #endregion

                case RecurrencePattern.Yearly:
                    #region Yearly

                    if (trigger.Recurrence.MonthlyRecurrencePatternType.HasValue == false)
                        trigger.Recurrence.MonthlyRecurrencePatternType = MonthlyRecurrencePatternType.DayOfMonth;

                    trigger.Recurrence.IterationPeriodNumerator = Math.Abs(trigger.Recurrence.IterationPeriodNumerator);

                    switch (trigger.Recurrence.MonthlyRecurrencePatternType.Value)
                    {
                        case MonthlyRecurrencePatternType.DayOfMonth:

                            if (trigger.Recurrence.DayOfMonth.HasValue == false)
                            {
                                trigger.Recurrence.DayOfMonth = startDate.Day;
                                //throw new CalculationServiceException("Value of Recurrence.DayOfMonth is not set.");
                            }

                            if (trigger.Recurrence.MonthOfYear.HasValue == false)
                                throw new NullReferenceException("Value of Recurrence.MonthOfYear is not set.");

                            #region Set start date

                            int month = GetMonthNumber(trigger.Recurrence.MonthOfYear.Value);
                            int day = DateTime.DaysInMonth(date.Year, date.Month) < trigger.Recurrence.DayOfMonth.Value
                                    ? DateTime.DaysInMonth(date.Year, date.Month)
                                    : trigger.Recurrence.DayOfMonth.Value;

                            date = new DateTime(date.Year, month, day);

                            while (date < startDate)
                                date = date.AddYears(1);

                            #endregion

                            #region Create And Add Create Occurrences
                            while (ShouldCreateOccurrence(trigger.ScheduleRecurrenceType, date, endDate, currentNumberOfOccurences, trigger.Recurrence.MaxOccurances))
                            {
                                dates.Add(CreateCalendarEventOccurrence(date, trigger));
                                currentNumberOfOccurences++;
                                date = date.AddYears(trigger.Recurrence.IterationPeriodNumerator);
                            }
                            #endregion

                            break;

                        case MonthlyRecurrencePatternType.DayOfWeekOfMonth:

                            if (trigger.Recurrence.DayOfWeek.HasValue == false)
                                throw new Exception("Value of Recurrence.DayOfWeek is not set, it is null.");

                            if (trigger.Recurrence.WeekOfMonth.HasValue == false)
                                throw new Exception("Value of Recurrence.WeekOfMonth is not set, it is null.");

                            if (trigger.Recurrence.MonthOfYear.HasValue == false)
                                throw new Exception("Value of Recurrence.MonthOfYear is not set, it is null.");

                            #region Set start date

                            date = new DateTime(startDate.Year, GetMonthNumber((MonthOfYear)trigger.Recurrence.MonthOfYear), 1);
                            date = FindDayOfWeekOfMonth(trigger.Recurrence.DayOfWeek.GetValueOrDefault(),
                                                        (WeekOfMonth)trigger.Recurrence.WeekOfMonth,
                                                        new DateTime(date.Year, GetMonthNumber((MonthOfYear)trigger.Recurrence.MonthOfYear), 1));
                            if (date < startDate)
                                date = FindDayOfWeekOfMonth(trigger.Recurrence.DayOfWeek.GetValueOrDefault(),
                                                            (WeekOfMonth)trigger.Recurrence.WeekOfMonth,
                                                            new DateTime(date.Year + 1, GetMonthNumber((MonthOfYear)trigger.Recurrence.MonthOfYear), 1));

                            #endregion

                            #region Create and Add Occurrences

                            while (ShouldCreateOccurrence(trigger.ScheduleRecurrenceType, date, endDate, currentNumberOfOccurences, trigger.Recurrence.MaxOccurances))
                            {
                                dates.Add(CreateCalendarEventOccurrence(date, trigger));
                                currentNumberOfOccurences++;
                                date = FindDayOfWeekOfMonth(trigger.Recurrence.DayOfWeek.GetValueOrDefault(),
                                                            (WeekOfMonth)trigger.Recurrence.WeekOfMonth,
                                                            new DateTime(date.Year + 1, GetMonthNumber((MonthOfYear)trigger.Recurrence.MonthOfYear), 1));
                            }

                            #endregion

                            break;

                        default:
                            throw new NotSupportedException(string.Format("MonthlyRecurrencePatternType {0} is not supported.", trigger.Recurrence.MonthlyRecurrencePatternType)); ;
                    }

                    break;

                #endregion

                default:
                    throw new NotSupportedException(string.Format("RecurrencePattern {0} is not supported.", trigger.Recurrence.RecurrencePattern)); ;
            }
            return dates;
        }

        private static DateTime FindDayOfWeekOfMonth(DayOfWeek dayOfWeek, WeekOfMonth week, DateTime date)
        {
            var weekOrdinal = GetWeekOrdinalFromEnum(week);

            while (date.DayOfWeek.ToString() != dayOfWeek.ToString()) date = date.AddDays(1);

            if (weekOrdinal < 4)
            {
                date = date.AddDays(weekOrdinal * 7);
            }
            else
            {
                if (date.AddDays(weekOrdinal * 7).Month != date.Month)
                    date = date.AddDays(7 * (weekOrdinal - 1));
                else
                    date = date.AddDays(weekOrdinal * 7);
            }

            return date;
        }

        private static int GetWeekOrdinalFromEnum(WeekOfMonth week)
        {
            switch (week)
            {
                case WeekOfMonth.First: return 0;
                case WeekOfMonth.Second: return 1;
                case WeekOfMonth.Third: return 2;
                case WeekOfMonth.Fourth: return 3;
                case WeekOfMonth.Last: return 4;
                default: throw new Exception("Error converting Week enum to ordinal!");
            }
        }

        private static bool ShouldCreateOccurrence(ScheduleRecurrenceType recurenceType, DateTime currentDate, DateTime? endDate, long currentNumberOfOccurences, long? maxOccurrences)
        {
            if (recurenceType == ScheduleRecurrenceType.RecurrenceWithLimitedOccurrences)
            {
                if (maxOccurrences.HasValue == false)
                    throw new ArgumentNullException("MaxOccurrences value is not provided. ScheduleRecurrenceType is RecurrenceWithLimitedOccurrences", "maxOccurrences");

                return (currentNumberOfOccurences < maxOccurrences.Value);
            }
            else if (recurenceType == ScheduleRecurrenceType.RecurrenceWithLimitedPeriod || recurenceType == ScheduleRecurrenceType.RecurrenceWithoutEndSpecified)
            {
                if (endDate.HasValue == false)
                    throw new ArgumentNullException("EndDate value is not provided. ScheduleRecurrenceType is RecurrenceWithLimitedPeriod or RecurrenceWithoutEndSpecified", "endDate");

                return (currentDate <= endDate.Value);
            }
            else
                return false;
        }

        private static int GetMonthNumber(MonthOfYear monthOfYear)
        {
            switch (monthOfYear)
            {
                case MonthOfYear.January:
                    return 1;

                case MonthOfYear.February:
                    return 2;

                case MonthOfYear.March:
                    return 3;

                case MonthOfYear.April:
                    return 4;

                case MonthOfYear.May:
                    return 5;

                case MonthOfYear.Jun:
                    return 6;

                case MonthOfYear.July:
                    return 7;

                case MonthOfYear.August:
                    return 8;

                case MonthOfYear.September:
                    return 9;

                case MonthOfYear.October:
                    return 10;

                case MonthOfYear.November:
                    return 11;

                case MonthOfYear.December:
                    return 12;

                default:
                    throw new NotSupportedException(string.Format("MonthOfYear value {0}is not supported.", monthOfYear));
            }
        }

        private static CalculationOccurrence CreateCalendarEventOccurrence(DateTime date, CalculationTrigger trigger)
        {
            CalculationOccurrence occurrence = new CalculationOccurrence();

            occurrence.Trigger = trigger;

            occurrence.Status = OccurrenceStatus.Undue;

            if (trigger.Recurrence != null && trigger.Recurrence.FollowTheEndOfMonth == true)
            {
                date = ToEoM(date);
                if (trigger.TriggerEndDate.HasValue && date > trigger.TriggerEndDate)
                {
                    date = trigger.TriggerEndDate.Value;
                }
            }

            occurrence.ScheduledDate = date;

            return occurrence;
        }

        private static bool IsInRange(DateTime date, DateTime rangeStartDate, DateTime rangeEndDate)
        {
            if (date >= rangeStartDate && date <= rangeEndDate) return true;
            else return false;
        }

        private static void ResolveNonWorkingDaysResolutionMethod(IEnumerable<CalculationOccurrence> inputOccurences, DateTime startDate, DateTime? endDate)
        {
            if (inputOccurences == null || inputOccurences.Count() == 0)
                return;

            if (inputOccurences.Any(it => it == null))
            {

            }

            var occurencesWithNonWorkingDaysResolutionMethog = from it in inputOccurences
                                                               where
                                                               it != null &&
                                                               it.Trigger != null &&
                                                               it.Trigger.NonWorkingDayCalendar != null
                                                               group it by it.Trigger into g
                                                               select new { Trigger = g.Key, Occurences = g };

            foreach (var occurenceGroup in occurencesWithNonWorkingDaysResolutionMethog)
            {
                var parentTrigger = occurenceGroup.Trigger;

                if (parentTrigger.NonWorkingDayRule.HasValue == false ||
                    parentTrigger.NonWorkingDayRule == NonWorkingDayResolutionType.TreatAllDaysAsWorking)
                {
                    continue;
                }

                var occurences = occurenceGroup.Occurences;

                NonWorkingDayCalendar nonWorkingCalendar = parentTrigger.NonWorkingDayCalendar;

                if (nonWorkingCalendar == null ||
                    nonWorkingCalendar.NonWorkingDays == null ||
                    nonWorkingCalendar.NonWorkingDays.Count == 0)
                {
                    continue;
                }

                var match = from it in occurences
                            from at in nonWorkingCalendar.NonWorkingDays
                            where it.ScheduledDate.Date.Equals(at.Date)
                            select it;

                foreach (var item in match)
                {
                    item.ScheduledDate = GetNextAvailableDay(item.ScheduledDate, parentTrigger.NonWorkingDayRule.Value, nonWorkingCalendar.NonWorkingDays, startDate, endDate);
                }
            }
        }

        private static DateTime ToEoM(DateTime date)
        {
            int lastDayInMonth = DateTime.DaysInMonth(date.Year, date.Month);

            if (date.Day < lastDayInMonth)
            {
                date = new DateTime(date.Year, date.Month, lastDayInMonth, date.Hour, date.Minute, date.Second, date.Millisecond, date.Kind);
            }

            return date;
        }

        private static DateTime GetNextAvailableDay(DateTime occurenceDate, NonWorkingDayResolutionType resolutionType, IEnumerable<DateTime> nonWorkingDays, DateTime startDate, DateTime? endDate)
        {
            switch (resolutionType)
            {
                case NonWorkingDayResolutionType.TreatAllDaysAsWorking:
                    return occurenceDate;

                case NonWorkingDayResolutionType.FirstWorkingDayAfter:
                    occurenceDate = GetFirstWorkingDay(occurenceDate, 1, nonWorkingDays);
                    break;

                case NonWorkingDayResolutionType.FirstWorkingDayAfterExceptLastOne:
                    if (endDate.HasValue && endDate.Value.Date == occurenceDate.Date)
                    {
                        // ne radi nista ako je poslednji, otavi ga takvog
                    }
                    else
                    {
                        occurenceDate = GetFirstWorkingDay(occurenceDate, 1, nonWorkingDays);
                    }
                    break;

                case NonWorkingDayResolutionType.FirstWorkingDayAfterExceptLastOneThatGoesBefore:
                    if (endDate.HasValue && endDate.Value.Date == occurenceDate.Date)
                    {
                        occurenceDate = GetFirstWorkingDay(occurenceDate, -1, nonWorkingDays);
                    }
                    else
                    {
                        occurenceDate = GetFirstWorkingDay(occurenceDate, 1, nonWorkingDays);
                    }
                    break;

                case NonWorkingDayResolutionType.FirstWorkingDayAfterIfDoNotMoveInNextMonth:
                    DateTime tempOccurenceDate = GetFirstWorkingDay(occurenceDate, 1, nonWorkingDays);
                    if (tempOccurenceDate.Month != occurenceDate.Month)
                    {
                        // ostavi isti datum, ako novo izracunati datum prelazi u drugi mesec
                    }
                    else
                    {
                        occurenceDate = tempOccurenceDate;
                    }
                    break;

                case NonWorkingDayResolutionType.FirstWorkingDayBefore:
                    occurenceDate = GetFirstWorkingDay(occurenceDate, -1, nonWorkingDays);
                    break;

                case NonWorkingDayResolutionType.FirstWorkingDayBeforeExceptLastOne:
                    if (endDate.HasValue && endDate.Value.Date == occurenceDate.Date)
                    {
                        // ne radi nista ako je poslednji, otavi ga takvog
                    }
                    else
                    {
                        occurenceDate = GetFirstWorkingDay(occurenceDate, -1, nonWorkingDays);
                    }
                    break;

                default:
                    throw new NotSupportedException(string.Format("NonWorkingDayResolutionType {0} is not supported.", resolutionType));
            }

            return occurenceDate;

            //var match = from it in nonWorkingDays
            //            where it.Date.Equals(occurenceDate.Date)
            //            select it;

            //if (match.Any() == false)
            //{
            //    return occurenceDate;
            //}
            //else
            //{
            //    return GetNextAvailableDay(occurenceDate, resolutionType, nonWorkingDays);
            //}
        }

        private static DateTime GetFirstWorkingDay(DateTime input, int seekDirection, IEnumerable<DateTime> nonWorkingDays)
        {
            while (IsWorkingDay(input, nonWorkingDays) == false)
            {
                input = input.AddDays(seekDirection);
            }
            return input;
        }

        private static bool IsWorkingDay(DateTime date, IEnumerable<DateTime> nonWorkingDays = null)
        {
            if (nonWorkingDays != null && nonWorkingDays.Any(it => it.Date.Date == date.Date))
            {
                return false;
            }

            switch (date.DayOfWeek)
            {
                case System.DayOfWeek.Saturday:
                case System.DayOfWeek.Sunday:
                    return false;
            }

            return true;
        }

        internal static DateTime? EvaluateStartDateTime(DateTime? startDateTime, TimePeriod offset)
        {
            if (offset == null)
            {
                return startDateTime;
            }

            return EvaluateStartDateTime(startDateTime, offset.Period, offset.UnitOfTime);
        }

        internal static DateTime? EvaluateStartDateTime(DateTime? startDateTime, int? offset, SimpleUnitOfTime? offsetPeriod)
        {
            if (startDateTime.HasValue == false)
            {
                return null;
            }

            if (offset.HasValue == false ||
                offset == 0)
            {
                return startDateTime;
            }

            if (offsetPeriod.HasValue == false)
            {
                throw new ArgumentException("Offset.Period (UnitOfTime) is not set. It is NULL.");
            }

            switch (offsetPeriod.Value)
            {
                case SimpleUnitOfTime.D:
                    return (startDateTime.Value.AddDays(offset.Value));

                case SimpleUnitOfTime.M:
                    return (startDateTime.Value.AddMonths(offset.Value));

                case SimpleUnitOfTime.Y:
                    return (startDateTime.Value.AddYears(offset.Value));

                default:
                    return (startDateTime.Value.AddDays(offset.Value));
            }
        }

        #endregion
    }
}
