using PriceCalculation.Models.Lifecycle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FinancialCalculations
{
    /*public enum CalendarBasisKind
    {
        [EnumMember(Value = "30a-360")]
        Calendar30A360,

        [EnumMember(Value = "30u-360")]
        Calendar30U360,

        [EnumMember(Value = "30e-360")]
        Calendar30E360,

        [EnumMember(Value = "30e-360-isda")]
        Calendar30E360ISDA,

        [EnumMember(Value = "act-act-icma")]
        CalendarActActICMA,

        [EnumMember(Value = "act-act-isda")]
        CalendarActActISDA,

        [EnumMember(Value = "act-365-fixed")]
        CalendarAct365Fixed,

        [EnumMember(Value = "act-360")]
        CalendarAct360,

        [EnumMember(Value = "act-364")]
        CalendarAct364,

        [EnumMember(Value = "act-365-l")]
        CalendarAct365L,

        [EnumMember(Value = "act-act-afb")]
        CalendarActActAFB,

        [EnumMember(Value = "30e-365")]
        Calendar30E365,

        [EnumMember(Value = "30u-365")]
        Calendar30U365,

        [EnumMember(Value = "30e-actual")]
        Calendar30EActual,

        [EnumMember(Value = "30u-actual")]
        Calendar30UActual

    }*/

    public static class InterestCalculation
    {
        public static DateTime ZeroDate = new DateTime(1900, 1, 1, 0, 0, 0, 0);
        public static readonly string SQLServerDateTimeFormat = "yyyy-MM-dd HH:mm:ss.fff";
        public static readonly string Year = "Y";
        public static readonly string Month = "M";
        public static readonly string Day = "D";
        public static readonly string Second = "S";

        public static double YearFrac(DateTime zeroDate, DateTime currentDate)
        {
            double tZero = 365;
            if (DateTime.IsLeapYear(zeroDate.Year))
            {
                tZero = 366;
            }
            else
            {
                tZero = 365;
            }

            double d = 0;
            double t = 365;
            double numberOfYears = 0;

            d = Math.Abs((currentDate - zeroDate).Days);

            if (DateTime.IsLeapYear(currentDate.Year))
            {
                t = 366;
            }
            else
            {
                t = 365;
            }

            if (currentDate.Year == zeroDate.Year)
            {
                numberOfYears = d / t;
            }
            else
            {
                int numberOfYearsBetween = Math.Abs(currentDate.Year - zeroDate.Year);
                if (numberOfYearsBetween == 0 || numberOfYearsBetween == 1)
                {
                    numberOfYearsBetween = 0;
                }
                else
                {
                    numberOfYearsBetween = numberOfYearsBetween - 1;
                }

                numberOfYears = Math.Abs((new DateTime(zeroDate.Year, 12, 31) - zeroDate).Days / tZero) + numberOfYearsBetween + Math.Abs((currentDate - new DateTime(currentDate.Year - 1, 12, 31)).Days / t);
            }

            return numberOfYears;
        }

        public static decimal CalculateDiscountedNetCashFlow(double netCashFlow, double effectiveInterestRate, DateTime currentDate, DateTime startDate)
        {
            double discountedNetCashFlow = netCashFlow * Math.Pow((1 + effectiveInterestRate / 100), -YearFrac(startDate, currentDate));

            return Convert.ToDecimal(discountedNetCashFlow);
        }

        /// <summary>
        /// Caclulates effective interest rate.
        /// </summary>
        /// <param name="netCashFlows">Net cash flows as input for calculation.</param>
        /// <param name="accuracy">Accuracy to be used in the approximation.</param>
        /// <param name="decimalsToRound">Number of decimals to round the results.</param>
        /// <returns>Calculated effective interest rate.</returns>
        public static decimal CalculateEffectiveInterestRate(
            IEnumerable<NetCashFlowItem> netCashFlows,
            decimal? accuracy = 0.0001m,
            int? decimalsToRound = 2)
        {
            if (netCashFlows == null || netCashFlows.Count() == 0)
            {
                return 0;
            }

            if (accuracy.HasValue == false ||
                accuracy == decimal.Zero)
            {
                accuracy = 0.0001m;
            }

            var netCashFlowsList = netCashFlows.OrderBy(it => it.Date).ToList();

            DateTime zeroDate = netCashFlowsList.First().Date;

            double netCashFlow = 0;
            double discountedNetCashFlow = 0;
            double derivativeDiscountedNetCashFlow = 0;

            double numberOfYears = 0;

            double effectiveInterestRate = 0;

            double functionValue = 0;
            double derivativeFunctionValue = 0;

            bool firstIteration = true;

            int iteration = 0;

            do
            {
                iteration++;

                if (iteration > 100)
                {
                    throw new Exception(string.Format("Something went wrong, effective interest rate was not foud in {0} iterations. Check provided net cash flow amounts (must have negative and positive amounts).", iteration));
                }

                netCashFlow = 0;
                discountedNetCashFlow = 0;
                derivativeDiscountedNetCashFlow = 0;

                if (firstIteration)
                {
                    effectiveInterestRate = 0;
                    firstIteration = false;
                }
                else
                {
                    // izracunavanje sledece vrednosti: x1 = x0 - f(x0)/f'(x0)
                    effectiveInterestRate = effectiveInterestRate - (functionValue / derivativeFunctionValue);
                }

                functionValue = 0;
                derivativeFunctionValue = 0;

                for (int i = 0; i < netCashFlowsList.Count(); i++)
                {
                    // DateTime.DaysInMonth

                    NetCashFlowItem currentItem = netCashFlowsList[i];

                    DateTime currentDate = currentItem.Date;

                    //netCashFlow = currentRow.NetCashFlow;
                    netCashFlow = (double)currentItem.NetCashFlow;

                    numberOfYears = YearFrac(zeroDate, currentDate);

                    // Funckija f(eks)                    
                    discountedNetCashFlow = netCashFlow * Math.Pow((1 + effectiveInterestRate / 100), -numberOfYears);
                    //// Slobin nacin
                    //discountedNetCashFlow = CalculateInterest(currentDate, zeroDate, zeroDate, effectiveInterestRate, 'Y', true, null, netCashFlow).Last().AccumulatedBalance;
                    currentItem.DiscountedNetCashFlow = Convert.ToDecimal(discountedNetCashFlow);
                    functionValue += discountedNetCashFlow;

                    // Izvod funkcije f'(eks)                    
                    derivativeDiscountedNetCashFlow = -(numberOfYears * (netCashFlow / 100)) * Math.Pow((100 / (100 + effectiveInterestRate)), numberOfYears + 1);
                    derivativeFunctionValue += derivativeDiscountedNetCashFlow;
                }
            }
            while (functionValue != 0 && Math.Abs(0 - functionValue) > (double)accuracy);

            decimal resultRate = Convert.ToDecimal(effectiveInterestRate);

            if (decimalsToRound.HasValue)
            {
                return Math.Round(resultRate, decimalsToRound.Value);
            }
            else
            {
                return resultRate;
            }
        }


        private static int DatePart(string datepart, DateTime date)
        {
            switch (datepart)
            {
                case "Y":
                    return date.Year;
                case "M":
                    return date.Month;
                case "D":
                    return date.Day;
                default:
                    throw new InvalidCastException("Unknown unit of measure of time.");
            }
        }

        public static DateTime DateAdd(string datepart, int number, DateTime date)
        {
            switch (datepart)
            {
                case "Y":
                    return date.AddYears(number);
                case "M":
                    return date.AddMonths(number);
                case "D":
                    return date.AddDays(number);
                default:
                    throw new InvalidCastException("Unknown unit of measure of time.");
            }
        }

        private static int DateDiff(string datepart, DateTime startdate, DateTime enddate)
        {
            switch (datepart)
            {
                case "Y":
                    return enddate.Year - startdate.Year;
                case "M":
                    int _years = enddate.Year - startdate.Year;
                    int _months = enddate.Month - startdate.Month;
                    return _years * 12 + _months;
                case "D":
                    return (enddate.Date - startdate.Date).Days;

                case "S":
                    DateTime _ssec, _esec;
                    short _sgnsec;
                    if (enddate >= startdate)
                    {
                        _sgnsec = 1;
                        _ssec = startdate;
                        _esec = enddate;
                    }
                    else
                    {
                        _sgnsec = -1;
                        _ssec = enddate;
                        _esec = startdate;
                    }

                    int _seconds = (_esec - _ssec).Seconds;

                    if (_ssec.Second > _esec.Second)
                        _seconds++;
                    else
                    {
                        if (_ssec.Second == _esec.Second)
                        {
                            if (_ssec.Millisecond > _esec.Millisecond)
                                _seconds++;
                        }
                    }

                    return _seconds * _sgnsec;

                default:
                    throw new InvalidCastException("Unknown unit of measure of time.");
            }
        }

        public static bool IsEndOfFebruary(DateTime d)
        {
            bool r;
            DateTime nextday = DateAdd(Day, 1, d);
            if (DatePart(Day, nextday) == 1 && DatePart(Month, nextday) == 3)
                r = true;
            else
                r = false;

            return r;
        }

        public static bool IsEndOfMonth(DateTime d)
        {
            bool r;
            if (DatePart(Day, (DateAdd(Day, 1, d))) == 1)
                r = true;
            else
                r = false;

            return r;
        }

        private static bool IsLeapYear(DateTime date)
        {
            return DateTime.IsLeapYear(date.Year);
        }

        public static int CountDaysForRateUnitOfTime(DateTime date,
                                             int sgn,
                                             char rateUnitOfTime = 'Y')
        {
            int daysInCalendar;

            if (rateUnitOfTime == 'Y')
            {
                // nadjemo pocetak godine
                DateTime d = new DateTime(date.Year, 1, 1);
                // ako je anticipativno, a nije pocetak godine, idemo na pocetak sledece godine
                if ((sgn < 0) && (date > d))
                    d = d.AddYears(1);
                // racunamo godinu dana od tog dana, u smeru obracuna
                daysInCalendar = Math.Abs(DateDiff("D", d, DateAdd("Y", sgn, d)));
            }
            else if (rateUnitOfTime == 'Q')
            {
                // nadjemo pocetak kvartala
                DateTime d = new DateTime(date.Year, ((date.Month - 1) / 3) * 3, 1);
                // ako je anticipativno, a nije pocetak kvartala, idemo na pocetak sledeceg kvartala
                if ((sgn < 0) && (date > d))
                    d = d.AddMonths(3);
                // racunamo 3 meseca od tog dana, u smeru obracuna
                daysInCalendar = Math.Abs(DateDiff("D", d, DateAdd("M", 3 * sgn, d)));
            }
            else if (rateUnitOfTime == 'M')
            {
                // nadjemo pocetak meseca
                DateTime d = new DateTime(date.Year, date.Month, 1);
                // ako je anticipativno, a nije pocetak meseca, idemo na pocetak sledeceg meseca
                if ((sgn < 0) && (date > d))
                    d = d.AddMonths(1);
                // racunamo mesec dana od tog dana, u smeru obracuna
                daysInCalendar = Math.Abs(DateDiff("D", d, DateAdd("M", sgn, d)));
            }
            else if (rateUnitOfTime == 'D')
                daysInCalendar = 1;
            else
                throw new InvalidCastException("Unknown unit of measure of time.");

            return daysInCalendar;
        }

        public static List<DateSegment> SplitToYearlySegments(DateTime date1,
                                                              DateTime date2,
                                                              char RateUnitOfTime = 'Y')
        {
            List<DateSegment> t = new List<DateSegment>();

            DateTime d1, d2;
            int rn = 0, zeroyear = ZeroDate.Year, sgn;

            if (date1 < date2)
                sgn = 1;
            else
                sgn = -1;

            int y = date1.Year, m = date1.Month;

            d1 = date1;
            d2 = DateAdd(Year, sgn == -1 ? 0 : 1, DateAdd(Year, y - zeroyear, ZeroDate));
            if (d2 == d1)
                d2 = DateAdd(Year, sgn, d1);

            while (DateDiff(Day, d2, date2) * sgn > 0)
            {
                rn = rn + 1;
                t.Add(new DateSegment()
                {
                    RowNum = rn,
                    DateFrom = d1,
                    DateTo = d2,
                    DaysInPeriod = (d2 - d1).TotalDays,
                    DaysInCalendar = Math.Abs(DateDiff(Day, d2, DateAdd(Year, -sgn, d2))),
                    RateDays = CountDaysForRateUnitOfTime(d1, sgn, RateUnitOfTime)
                });
                d1 = d2;
                y = d1.Year;
                m = d1.Month;
                d2 = DateAdd(Year, sgn, d1);
            }

            // idemo jos jednom ako ima vremena manjeg od godine
            if (DateDiff(Second, d2, date2) * sgn > 0)
            {
                rn = rn + 1;
                t.Add(new DateSegment()
                {
                    RowNum = rn,
                    DateFrom = d1,
                    DateTo = d2,
                    DaysInPeriod = (d2 - d1).TotalDays,
                    DaysInCalendar = Math.Abs(DateDiff(Day, d2, DateAdd(Year, -sgn, d2))),
                    RateDays = CountDaysForRateUnitOfTime(d1, sgn, RateUnitOfTime)
                });
                d1 = d2;
            }

            rn = rn + 1;
            t.Add(new DateSegment()
            {
                RowNum = rn,
                DateFrom = d1,
                DateTo = date2,
                DaysInPeriod = (date2 - d1).TotalDays,
                DaysInCalendar = Math.Abs(DateDiff(Day, d1, DateAdd(Year, sgn, d1))),
                RateDays = CountDaysForRateUnitOfTime(d1, sgn, RateUnitOfTime)
            });

            return t;
        }

        public static List<DateSegment> SplitToMonthlySegments(DateTime date1,
                                                               DateTime date2,
                                                               char RateUnitOfTime = 'Y')
        {

            List<DateSegment> t = new List<DateSegment>();

            DateTime d1, d2;
            int rn = 0, zeroyear = ZeroDate.Year, sgn;

            if (date1 < date2)
                sgn = 1;
            else
                sgn = -1;

            int y = date1.Year, m = date1.Month;

            d1 = date1;
            d2 = DateAdd(Month, m + (sgn == -1 ? -1 : 0), DateAdd(Year, y - zeroyear, ZeroDate));
            if (d2 == d1)
                d2 = DateAdd(Month, sgn, d1);

            while (DateDiff(Day, d2, date2) * sgn > 0)
            {
                rn = rn + 1;
                t.Add(new DateSegment()
                {
                    RowNum = rn,
                    DateFrom = d1,
                    DateTo = d2,
                    DaysInPeriod = (d2 - d1).TotalDays,
                    DaysInCalendar = Math.Abs(DateDiff(Day, d2, DateAdd(Month, -sgn, d2))),
                    RateDays = CountDaysForRateUnitOfTime(d1, sgn, RateUnitOfTime)
                });
                d1 = d2;
                y = d1.Year;
                m = d1.Month;
                d2 = DateAdd(Month, sgn, d1);
            }

            // idemo jos jednom ako ima vremena manjeg od dana
            if (DateDiff(Second, d2, date2) * sgn > 0)
            {
                rn = rn + 1;
                t.Add(new DateSegment()
                {
                    RowNum = rn,
                    DateFrom = d1,
                    DateTo = d2,
                    DaysInPeriod = (d2 - d1).TotalDays,
                    DaysInCalendar = Math.Abs(DateDiff(Day, d2, DateAdd(Month, -sgn, d2))),
                    RateDays = CountDaysForRateUnitOfTime(d1, sgn, RateUnitOfTime)
                });
                d1 = d2;
            }

            rn = rn + 1;
            t.Add(new DateSegment()
            {
                RowNum = rn,
                DateFrom = d1,
                DateTo = date2,
                DaysInPeriod = (date2 - d1).TotalDays,
                DaysInCalendar = Math.Abs(DateDiff(Day, d1, DateAdd(Month, sgn, d1))),
                RateDays = CountDaysForRateUnitOfTime(d1, sgn, RateUnitOfTime)
            });

            return t;
        }


        public static InterestCalculationResult CalculateInterest(
                                                                   DateTime date1,
                                                                   DateTime date2,
                                                                   DateTime date3,
                                                                   double ratePercentage,
                                                                   char rateUnitOfTime = 'Y',
                                                                   bool isCompound = true,
                                                                   CalendarBasisKind calendarBasis = CalendarBasisKind.CalendarActActISDA,
                                                                   double amount = 1,
                                                                   double? calaculationPrecision = null,
                                                                   bool useAnticipativeMethod = false // nije kao u PUB, vec znaci da se procenat primenjuje na osnovicu kakva ce biti na date2, za anticipativ kao u PUB, samo obrnuti datume, tako da je date1 > date2
                                                                  )
        {
            // TODO: Throw not implemented for unsupported methods
            InterestCalculationResult t = new InterestCalculationResult();

            if (date3 == null)
                date3 = date2;

            if (rateUnitOfTime == 'G')
                rateUnitOfTime = 'Y';

            int y1, M1, d1, y2, M2, d2, zeroyear = ZeroDate.Year;
            double s1, s2, daysincalendar, interestdays, BaseAmount = 0, interest = 0, ac;
            DateTime fd1, fd2;

            if (useAnticipativeMethod == false)
                ac = 1;
            else
                ac = -1;

            y1 = DatePart(Year, date1);
            M1 = DatePart(Month, date1);
            d1 = DatePart(Day, date1);
            y2 = DatePart(Year, date2);
            M2 = DatePart(Month, date2);
            d2 = DatePart(Day, date2);

            fd1 = DateAdd(Day, d1 - 1, DateAdd(Month, M1 - 1, DateAdd(Year, y1 - zeroyear, ZeroDate)));
            fd2 = DateAdd(Day, d2 - 1, DateAdd(Month, M2 - 1, DateAdd(Year, y2 - zeroyear, ZeroDate)));

            s1 = (date1 - fd1).TotalDays;
            s2 = (date2 - fd2).TotalDays;

            if (calendarBasis == CalendarBasisKind.Calendar30U360)
            {
                if ((IsEndOfFebruary(fd1) == true) && (IsEndOfFebruary(fd2) == true))
                {
                    d2 = 30;
                    s2 = 0;
                }
                if (IsEndOfFebruary(fd1) == true)
                {
                    d1 = 30;
                    s1 = 0;
                }
                if (d2 == 31 && (d1 == 30 || d1 == 31))
                    d2 = 30;
                if (d1 == 31)
                    d1 = 30;
            }
            else if (calendarBasis == CalendarBasisKind.Calendar30E360)
            {
                if (d1 == 31)
                    d1 = 30;
                if (d2 == 31)
                    d2 = 30;
            }
            else if (calendarBasis == CalendarBasisKind.Calendar30E360ISDA)
            {
                if (IsEndOfMonth(fd1) == true)
                {
                    d1 = 30;
                    if (IsEndOfFebruary(fd1) == true)
                    {
                        s1 = 0;
                    }
                }
                if (IsEndOfMonth(fd2) == true)
                {
                    if (!(IsEndOfFebruary(fd2) == true && date2 == date3)) // isLastPeriod = 1
                    {
                        d2 = 30;
                        if (IsEndOfFebruary(fd2) == true)
                        {
                            s2 = 0;
                        }
                    }
                }
            }
            else if (calendarBasis == CalendarBasisKind.Calendar30A360) // CHECK "30E+/360"
            {
                if (d1 == 31)
                    d1 = 30;
                if (d2 == 31)
                {
                    fd2 = DateAdd(Day, 1, fd2);
                    y2 = fd2.Year;
                    M2 = fd2.Month;
                    d2 = 1;
                }
            }

            double perc, fract, d13, freq;
            char ut;
            DateTime dt;

            CalendarBasisKind[] actualMethods = { CalendarBasisKind.CalendarAct360, CalendarBasisKind.CalendarAct364, CalendarBasisKind.CalendarAct365Fixed, CalendarBasisKind.CalendarAct365L, CalendarBasisKind.CalendarActActAFB, CalendarBasisKind.CalendarActActICMA, CalendarBasisKind.CalendarActActISDA };

            bool is30360 = (calendarBasis == CalendarBasisKind.Calendar30A360) || 
                (calendarBasis == CalendarBasisKind.Calendar30E360) || 
                (calendarBasis == CalendarBasisKind.Calendar30E360ISDA) ||
                (calendarBasis == CalendarBasisKind.Calendar30U360);

            if (is30360) //CalendarBasis like '30%/360%'
            {
                if (rateUnitOfTime == 'Y')
                    daysincalendar = 360;
                else if (rateUnitOfTime == 'Q')
                    daysincalendar = 90;
                else if (rateUnitOfTime == 'M')
                    daysincalendar = 30;
                else
                    daysincalendar = 1;

                interestdays = (360 * (y2 - y1) + 30 * (M2 - M1) + (d2 - d1)) + (s2 - s1);

                t.Add(new DateSegment()
                {
                    RowNum = 1,
                    DateFrom = date1,
                    DateTo = date2,
                    DaysInPeriod = interestdays,
                    DaysInCalendar = daysincalendar,
                    Rate = ratePercentage,
                    RateUnitOfTime = rateUnitOfTime,
                    RateDays = daysincalendar,
                    BaseAmount = amount,
                    Interest = 0,
                    AccumulatedBalance = amount
                });

                //insert into t(DateFrom, DateTo, InterestDays, DaysInCalendar, BaseAmount, Interest)
                //values(date1, date2, cast(date1 as float), cast(date2 as float), cast(fd1 as float), cast(fd2 as float))
                //insert into t(DateFrom, DateTo, InterestDays, DaysInCalendar, BaseAmount, Interest)
                //values(fd1, fd2, cast(date1 as float), cast(date2 as float), cast(fd1 as float), cast(fd2 as float))
            }
            else if (calendarBasis == CalendarBasisKind.CalendarAct365Fixed || calendarBasis == CalendarBasisKind.CalendarAct360) //CalendarBasis in ('Actual/365', 'Actual/360')
            {
                if (calendarBasis == CalendarBasisKind.CalendarAct365Fixed)
                    daysincalendar = 365;
                else
                    daysincalendar = 360;
                fract = 1;
                ut = 'Y';

                if (rateUnitOfTime == 'Q')
                    fract = 4;
                else if (rateUnitOfTime == 'M')
                    fract = 12;
                else if (rateUnitOfTime == 'D')
                {
                    daysincalendar = 1;
                    fract = 1;
                    ut = 'D';
                }

                // pretvaranje stope - videti sa UseAnticipativeMethod
                if (isCompound == true)
                    perc = (Math.Pow(1 + ratePercentage / 100 * ac, fract) - 1) * 100;
                else
                    perc = ratePercentage * fract;

                interestdays = DateDiff(Day, fd1, fd2) + (s2 - s1);

                t.Add(new DateSegment()
                {
                    RowNum = 1,
                    DateFrom = date1,
                    DateTo = date2,
                    DaysInPeriod = interestdays,
                    DaysInCalendar = daysincalendar,
                    Rate = perc,
                    RateUnitOfTime = ut,
                    RateDays = daysincalendar,
                    BaseAmount = amount,
                    Interest = 0,
                    AccumulatedBalance = amount
                });
            }
            else if (calendarBasis == CalendarBasisKind.CalendarAct365L)
            {
                d13 = DateDiff(Day, date1, date3); // + (s3 - s1)
                freq = Math.Round(365 / d13, 6);
                daysincalendar = 365;

                if (IsLeapYear(date3) == true && date3.Month >= 3)
                    daysincalendar = 366;
                else if (Math.Abs(freq) > 1 && IsLeapYear(date3) == true)
                    daysincalendar = 366;
                else if (Math.Abs(freq) <= 1)
                {
                    if (date1.Month <= 2 && IsEndOfFebruary(date1) == false) //(dt.Month <= 2 && IsEndOfFebruary(date1) == false)
                        dt = date1;
                    else
                        dt = DateAdd(Year, 1, date1);
                    while (dt <= date3)
                    {
                        if (IsLeapYear(dt) == true)
                        {
                            daysincalendar = 366;
                            break;
                        }
                        dt = DateAdd(Year, 1, dt);
                    }
                }

                fract = 1;
                ut = 'Y';

                if (rateUnitOfTime == 'Q')
                    fract = 4;
                else if (rateUnitOfTime == 'M')
                    fract = 12;
                else if (rateUnitOfTime == 'D')
                {
                    fract = DateDiff(Day, fd1, DateAdd(Year, 1, fd1));
                }

                // pretvaranje stope - videti sa UseAnticipativeMethod
                if (isCompound == true)
                    perc = (Math.Pow(1 + ratePercentage / 100 * ac, fract) - 1) * 100;
                else
                    perc = ratePercentage * fract;

                interestdays = DateDiff(Day, fd1, fd2) + (s2 - s1);

                t.Add(new DateSegment()
                {
                    RowNum = 1,
                    DateFrom = date1,
                    DateTo = date2,
                    DaysInPeriod = interestdays,
                    DaysInCalendar = daysincalendar,
                    Rate = perc,
                    RateUnitOfTime = ut,
                    RateDays = daysincalendar,
                    BaseAmount = amount,
                    Interest = 0,
                    AccumulatedBalance = amount
                });
            }
            else if (calendarBasis == CalendarBasisKind.CalendarActActICMA)
            {
                d13 = DateDiff(Day, date1, date3); // + (s3 - s1)
                freq = Math.Round(365 / d13, 6);
                daysincalendar = Math.Round(freq * d13, 0);

                fract = 1;
                ut = 'Y';

                if (rateUnitOfTime == 'Q')
                    fract = 4;
                else if (rateUnitOfTime == 'M')
                    fract = 12;
                else if (rateUnitOfTime == 'D')
                {
                    fract = DateDiff(Day, fd1, DateAdd(Year, 1, fd1));
                }

                // pretvaranje stope - videti sa UseAnticipativeMethod
                if (isCompound == true)
                    perc = (Math.Pow(1 + ratePercentage / 100 * ac, fract) - 1) * 100;
                else
                    perc = ratePercentage * fract;

                interestdays = DateDiff(Day, fd1, fd2) + (s2 - s1);

                t.Add(new DateSegment()
                {
                    RowNum = 1,
                    DateFrom = date1,
                    DateTo = date2,
                    DaysInPeriod = interestdays,
                    DaysInCalendar = d13,
                    Rate = perc,
                    RateUnitOfTime = ut,
                    RateDays = daysincalendar,
                    BaseAmount = amount,
                    Interest = 0,
                    AccumulatedBalance = amount
                });
            }
            else if (actualMethods.Contains(calendarBasis))//CalendarBasis like 'Actual%'
            {
                if (rateUnitOfTime == 'Y')
                {
                    foreach (DateSegment item in SplitToYearlySegments(date1, date2, rateUnitOfTime))
                    {
                        t.Add(new DateSegment()
                        {
                            RowNum = item.RowNum,
                            DateFrom = item.DateFrom,
                            DateTo = item.DateTo,
                            DaysInPeriod = item.DaysInPeriod,
                            DaysInCalendar = item.DaysInCalendar,
                            Rate = ratePercentage,
                            RateUnitOfTime = rateUnitOfTime,
                            RateDays = item.RateDays,
                            BaseAmount = amount,
                            Interest = 0,
                            AccumulatedBalance = amount
                        });
                    }
                }
                else //if (RateUnitOfTime == 'Q' || RateUnitOfTime == 'M') -- if RateUnitOfTime in ('Q', 'M')
                {
                    foreach (DateSegment item in SplitToMonthlySegments(date1, date2, rateUnitOfTime))
                    {
                        t.Add(new DateSegment()
                        {
                            RowNum = item.RowNum,
                            DateFrom = item.DateFrom,
                            DateTo = item.DateTo,
                            DaysInPeriod = item.DaysInPeriod,
                            DaysInCalendar = item.DaysInCalendar,
                            Rate = ratePercentage,
                            RateUnitOfTime = rateUnitOfTime,
                            RateDays = item.RateDays,
                            BaseAmount = amount,
                            Interest = 0,
                            AccumulatedBalance = amount
                        });
                    }
                }
            }

            DateTime? dat = null;
            int sgn;
            if (date1 > date2)
                sgn = -1;
            else
                sgn = 1;

            if (isCompound == false)
            {
                double factor = (from it in t
                                 select (it.DaysInPeriod / it.RateDays * ratePercentage / 100))
                                  .Sum();

                double interestCoefficientBase = (Math.Pow((factor * ac + sgn), sgn * ac) * sgn - 1);
                interest = amount * interestCoefficientBase;

                if (factor != 0)
                {
                    Parallel.ForEach(t, currenItem =>
                    {
                        double interestCoefficient = (currenItem.DaysInPeriod / currenItem.RateDays * currenItem.Rate / 100) / factor * interestCoefficientBase;
                        currenItem.InterestCoefficient = interestCoefficient;
                        //currenItem.Interest = interestCoefficient * amount;
                        currenItem.Interest = (currenItem.DaysInPeriod / currenItem.RateDays * currenItem.Rate / 100) / factor * interest;
                    });
                }

                for (int i = 0; i < t.Count; i++)
                {
                    BaseAmount = t[i].AccumulatedBalance = (dat == null ? t[i].AccumulatedBalance : BaseAmount) + t[i].Interest;
                    dat = t[i].DateFrom;
                }
            }
            else
            {
                for (int i = 0; i < t.Count; i++)
                {
                    BaseAmount = (dat == null ? t[i].BaseAmount : BaseAmount + interest);
                    dat = t[i].DateFrom;
                    double interestCoefficient = (Math.Pow((t[i].Rate / 100 * ac + 1), t[i].DaysInPeriod * ac / t[i].RateDays) - 1);
                    t[i].InterestCoefficient = interestCoefficient;
                    interest = t[i].Interest = BaseAmount * interestCoefficient;
                    t[i].BaseAmount = BaseAmount;
                    t[i].AccumulatedBalance = BaseAmount + interest;
                }
            }

            return t;
        }

    }

    public class DateSegment
    {
        public int RowNum { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public double DaysInPeriod { get; set; }
        public double DaysInCalendar { get; set; }
        public double RateDays { get; set; }
        public double Rate { get; set; }
        public char RateUnitOfTime { get; set; }
        public double BaseAmount { get; set; }
        public double InterestCoefficient { get; set; }
        public double Interest { get; set; }
        public double AccumulatedBalance { get; set; }
    }

    public class InterestCalculationResult: List<DateSegment>
    {
        public decimal TotalInterest
        {
            get
            {
                return Convert.ToDecimal(this.Sum(it => it.Interest));
            }
        }
    }

    public class NetCashFlowItem
    {
        private DateTime m_Date;
        private decimal m_NetCashFlow;
        private decimal m_DiscountedNetCashFlow;



        public DateTime Date
        {
            get { return m_Date; }
            set { m_Date = value; }
        }

        public decimal NetCashFlow
        {
            get { return m_NetCashFlow; }
            set { m_NetCashFlow = value; }
        }

        internal decimal DiscountedNetCashFlow
        {
            get { return m_DiscountedNetCashFlow; }
            set { m_DiscountedNetCashFlow = value; }
        }
    }

}
