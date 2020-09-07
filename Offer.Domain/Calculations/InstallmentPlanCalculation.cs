using FinancialCalculations;
using MicroserviceCommon.Contracts;
using Offer.Domain.AggregatesModel.ApplicationAggregate;
using PriceCalculation.Models.Lifecycle;
using PriceCalculation.Models.Pricing;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;

namespace Offer.Domain.Calculations
{
    public static class InstallmentPlanCalculation
    {
        [Enumeration("intercalar-interest-repayment-type", "Intercalar Interest Repayment Type", "Intercalar Interest Repayment Type")]
        public enum IntercalarInterestRepaymentType
        {
            [EnumMember(Value = "with-first-installment")]
            [Description("With First Installment")]
            WithFirstInstallment,
            [EnumMember(Value = "with-disbursement")]
            [Description("With Disbursement")]
            WithDisbursement,
            [EnumMember(Value = "after-intercalar-period")]
            [Description("After Intercalar Period")]
            AfterIntercalarPeriod
        }

        [Enumeration("repayment-type", "Repayment Type", "Repayment Type")]
        public enum RepaymentType
        {
            [EnumMember(Value = "fixed-annuity")]
            [Description("Fixed Annuity")]
            FixedAnnuity,
            [EnumMember(Value = "fixed-principal-repayment")]
            [Description("Fixed Principal Repayment")]
            FixedPrincipalRepayment,
            [EnumMember(Value = "bullet")]
            [Description("Bullet")]
            Bullet,
            [EnumMember(Value = "custom")]
            [Description("Custom")]
            Custom
        }

        public class InterestRateEntry
        {
            public DateTime Date { get; set; }
            public string Name { get; set; }
            public double RatePercentage { get; set; }
            public SimpleUnitOfTime RateUnitOfTime { get; set; }
            public bool IsCompound { get; set; }
            public CalendarBasisKind CalendarBasis { get; set; }
        }

        public class FeeEntry
        {
            public DateTime Date { get; set; }
            public FeeConditionKind Kind { get; set; }
            public string Name { get; set; }
            public FeeConditionFrequency Frequency { get; set; }
            public CalculationBasisType CalculationBasisType { get; set; }
            public string ServiceCode { get; set; }
            public string TariffCode { get; set; }
            public decimal Percentage { get; set; }
            public string Currency { get; set; }
            public decimal FixedAmount { get; set; }
            public decimal LowerLimit { get; set; }
            public decimal UpperLimit { get; set; }
            public decimal FixedAmountInDealCurrency { get; set; }
            public decimal LowerLimitInDealCurrency { get; set; }
            public decimal UpperLimitInDealCurrency { get; set; }
        }

        public class SimpleSchedule
        {
            public int DayOfMonth { get; set; }
            public int FrequencyPeriod { get; set; }
            public SimpleUnitOfTime FrequencyUnitOfTime { get; set; }
        }
        [Enumeration("calculation-target", "Calculation Target", "Calculation Target")]
        public enum CalculationTarget
        {
            [EnumMember(Value = "annuity")]
            [Description("Annuity")]
            Annuity,
            [EnumMember(Value = "amount")]
            [Description("Amount")]
            Amount,
            [EnumMember(Value = "term")]
            [Description("Term")]
            Term
        }

        public class SimpleLoanCalculationRequest
        {
            public CalculationTarget CalculationTarget { get; set; }
            public DateTime StardDate { get; set; }
            public DateTime EndDate { get; set; }

            public string Currency { get; set; }
            public decimal Amount { get; set; }
            public decimal Annuity { get; set; }
            public RepaymentType RepaymentType { get; set; }
            public int NumberOfInstallments { get; set; }
            public int MinimumDaysForFirstInstallment { get; set; }
            public bool AdjustFirstInstallment { get; set; }
            public bool PayFeeFromDisbursement { get; set; }

            public SimpleSchedule InstallmentSchedule { get; set; }
            public SimpleSchedule InterestSchedule { get; set; }
            public bool ForceInterestWithInstallment { get; set; }


            public List<InterestRateEntry> RegularInterest { get; set; }
            public IntercalarInterestRepaymentType IntercalarInterestRepaymentType { get; set; }

            public List<FeeEntry> Fees { get; set; }
            public bool FeeCurrencyConversionDone { get; set; }
            public string FeeCurrencyConversionMethod { get; set; }
        }

        public class InstallmentPlanCalculationResult
        {
            public int NumberOfInstallments { get; set; }
            public decimal Annuity { get; set; }
            public decimal Amount { get; set; }
            public decimal APR { get; set; }
            public List<InstallmentPlanRow> Rows { get; set; }
        }

        public static DateTime AddPeriod(this DateTime date, int period, SimpleUnitOfTime unitOfTime)
        {
            switch (unitOfTime)
            {
                case SimpleUnitOfTime.D:
                    return date.AddDays(period);
                case SimpleUnitOfTime.M:
                    return date.AddMonths(period);
                case SimpleUnitOfTime.Y:
                    return date.AddYears(period);
                default:
                    return date.AddMonths(period);
            }
        }

        public static char CharLiteral(this SimpleUnitOfTime unitOfTime)
        {
            switch (unitOfTime)
            {
                case SimpleUnitOfTime.D:
                    return 'D';
                case SimpleUnitOfTime.M:
                    return 'M';
                case SimpleUnitOfTime.Y:
                    return 'Y';
                default:
                    return 'M';
            }
        }

        public static double CalculateAnnuity(
            DateTime repaymentPeriodStartDate, 
            DateTime firstInstallmentDate, 
            int numberOfInstallments, 
            int installmentFrequencyPeriod, 
            SimpleUnitOfTime installmentFrequencyUnitOfTime,
            double ratePercentage,
            SimpleUnitOfTime rateUnitOfTime,
            bool isCompound, 
            CalendarBasisKind calendarBasis,
            double principalAmount,
            bool adjustFirstInstallment = false)
        {
            DateTime d2 = firstInstallmentDate.AddPeriod(installmentFrequencyPeriod * (numberOfInstallments - 1), installmentFrequencyUnitOfTime);

            double dummyAnnuity = 100;
            double outstanding = 0;
            double basis = 0;
            double interest = 0;
            DateTime d1;

            while (d2 > firstInstallmentDate)
            {
                d1 = d2.AddPeriod(- installmentFrequencyPeriod, installmentFrequencyUnitOfTime);
                basis = outstanding + dummyAnnuity;
                interest = InterestCalculation.CalculateInterest(d2, d1, d1, ratePercentage, rateUnitOfTime.CharLiteral(), isCompound, calendarBasis, basis).Sum(it => it.Interest);
                outstanding = basis + interest;
                d2 = d1;
            }

            d1 = repaymentPeriodStartDate;

            if (adjustFirstInstallment)
            {
                var prevDate = firstInstallmentDate.AddPeriod(-installmentFrequencyPeriod, installmentFrequencyUnitOfTime);
                if (prevDate < repaymentPeriodStartDate)
                {
                    d1 = prevDate;
                }
            }
            basis = outstanding + dummyAnnuity;
            interest = InterestCalculation.CalculateInterest(d2, d1, d1, ratePercentage, rateUnitOfTime.CharLiteral(), isCompound, calendarBasis, basis).Sum(it => it.Interest);
            outstanding = basis + interest;

            return principalAmount / outstanding * dummyAnnuity;
        }

        public static void AddInterest(this InstallmentPlanRow row, decimal interest, string description)
        {
            row.Description = row.Description + ", " + description;
            row.InterestRepayment += interest;
            row.Annuity += interest;
            row.NetCashFlow += interest;
            row.DiscountedNetCashFlow += interest;
        }

        public static DateTime MoveTo(this DateTime d, int dayOfMonth)
        {
            if (d.Day < dayOfMonth) // correction for day of month 29..31
            {
                DateTime d2 = d.AddMonths(1);
                int moveTo = (new DateTime(d2.Year, d2.Month, 1)).AddDays(-1).Day;
                if (dayOfMonth < moveTo)
                {
                    moveTo = dayOfMonth;
                }
                return new DateTime(d.Year, d.Month, moveTo);
            }
            else
                return d;
        }

        public static InstallmentPlanCalculationResult CalculateInstallmentPlan(SimpleLoanCalculationRequest request)
        {
            if (request.NumberOfInstallments == 0)
            {
                request.NumberOfInstallments = 1;
            }

            if (request.CalculationTarget == CalculationTarget.Term)
            {
                request.NumberOfInstallments = 12 * 200; // this will be cut off
            }

            //request.StardDate = request.StardDate.Date;

            // default installment schedule
            if (request.InstallmentSchedule == null)
            {
                request.InstallmentSchedule = new SimpleSchedule();
                request.InstallmentSchedule.DayOfMonth = request.StardDate.Day;
                request.InstallmentSchedule.FrequencyPeriod = 1;
                request.InstallmentSchedule.FrequencyUnitOfTime = SimpleUnitOfTime.M;
            }
            if (request.InstallmentSchedule.DayOfMonth == 0)
            {
                request.InstallmentSchedule.DayOfMonth = request.StardDate.Day;
            }

            // default interest schedule
            if (request.InterestSchedule == null)
            {
                request.InterestSchedule = new SimpleSchedule();
                request.InterestSchedule.DayOfMonth = request.InstallmentSchedule.DayOfMonth;
                request.InterestSchedule.FrequencyPeriod = request.InstallmentSchedule.FrequencyPeriod;
                request.InterestSchedule.FrequencyUnitOfTime = request.InstallmentSchedule.FrequencyUnitOfTime;
            }

            int dayOfMonth = request.InstallmentSchedule.DayOfMonth;

            DateTime startDate = request.StardDate;
            DateTime repaymentPeriodStartDate = request.StardDate;

            DateTime firstInstallmentDate = new DateTime(startDate.Year, startDate.Month, dayOfMonth);
            DateTime firstInterestDate = new DateTime(startDate.Year, startDate.Month, request.InterestSchedule.DayOfMonth);
            if (firstInterestDate <= startDate)
            {
                firstInterestDate = firstInterestDate.AddMonths(1);
            }
            if (firstInstallmentDate < startDate)
            {
                firstInstallmentDate = firstInstallmentDate.AddMonths(1);
            }
            Decimal intercalarInterest = 0;

            if (request.RegularInterest.Count != 1)
            {
                throw new NotImplementedException("Currently support only single interest rate entry");
            }
            var inr = request.RegularInterest.FirstOrDefault();

            if (firstInstallmentDate == startDate)
            {
                firstInstallmentDate = firstInstallmentDate.AddPeriod(request.InstallmentSchedule.FrequencyPeriod, request.InstallmentSchedule.FrequencyUnitOfTime);
            }
            if (firstInterestDate == startDate)
            {
                firstInterestDate = firstInterestDate.AddPeriod(request.InterestSchedule.FrequencyPeriod, request.InterestSchedule.FrequencyUnitOfTime);
            }

            // for days 29..31
            firstInstallmentDate = firstInstallmentDate.MoveTo(request.InstallmentSchedule.DayOfMonth);
            firstInterestDate = firstInterestDate.MoveTo(request.InterestSchedule.DayOfMonth);

            bool hasIntercalar = false;
            if (startDate.AddDays(request.MinimumDaysForFirstInstallment) > firstInstallmentDate)
            {
                // This means that date of first interest calculation and first installment will differ
                // Intercalar period must be forced
                repaymentPeriodStartDate = firstInterestDate;
                firstInstallmentDate = repaymentPeriodStartDate.AddPeriod(request.InstallmentSchedule.FrequencyPeriod, request.InstallmentSchedule.FrequencyUnitOfTime);
                hasIntercalar = true;
            }


            decimal annuity = request.Amount / request.NumberOfInstallments;

            if (request.RepaymentType == RepaymentType.FixedAnnuity)
            {
                if (request.CalculationTarget == CalculationTarget.Amount)
                {
                    decimal dummyAmount = 100000;
                    annuity = (decimal)CalculateAnnuity(repaymentPeriodStartDate, firstInstallmentDate,
                        request.NumberOfInstallments, request.InstallmentSchedule.FrequencyPeriod, request.InstallmentSchedule.FrequencyUnitOfTime,
                        inr.RatePercentage, inr.RateUnitOfTime, inr.IsCompound, inr.CalendarBasis, (double)dummyAmount, request.AdjustFirstInstallment);

                    request.Amount = Math.Round(dummyAmount / annuity * request.Annuity, 2, MidpointRounding.ToEven);
                    annuity = request.Annuity;
                }
            }

            if (request.CalculationTarget == CalculationTarget.Annuity && request.RepaymentType == RepaymentType.FixedAnnuity)
            {
                annuity = (decimal)CalculateAnnuity(repaymentPeriodStartDate, firstInstallmentDate,
                    request.NumberOfInstallments, request.InstallmentSchedule.FrequencyPeriod, request.InstallmentSchedule.FrequencyUnitOfTime,
                    inr.RatePercentage, inr.RateUnitOfTime, inr.IsCompound, inr.CalendarBasis, (double)request.Amount, request.AdjustFirstInstallment);

            }
            else if (request.CalculationTarget == CalculationTarget.Annuity && request.RepaymentType == RepaymentType.FixedPrincipalRepayment)
            {
                annuity = request.Amount / request.NumberOfInstallments;
            }
            else if (request.CalculationTarget == CalculationTarget.Annuity && request.RepaymentType == RepaymentType.Bullet)
            {
                annuity = request.Amount;
                request.NumberOfInstallments = 1;
            }
            else
            {
                annuity = request.Annuity;
            }

            annuity = Math.Round(annuity, 2, MidpointRounding.ToEven);

            if (hasIntercalar && inr != null)
            {
                intercalarInterest = InterestCalculation.CalculateInterest(startDate, firstInterestDate, firstInterestDate, inr.RatePercentage, inr.RateUnitOfTime.CharLiteral(), inr.IsCompound, inr.CalendarBasis, (double)request.Amount).TotalInterest;
                intercalarInterest = Math.Round(intercalarInterest, 2, MidpointRounding.ToEven);
            }

            InstallmentPlanCalculationResult result = new InstallmentPlanCalculationResult
            {
                NumberOfInstallments = request.NumberOfInstallments,
                Annuity = annuity,
                Amount = request.Amount,
                Rows = new List<InstallmentPlanRow>()
            };

            // Disbursement row
            InstallmentPlanRow row = new InstallmentPlanRow
            {
                Ordinal = 0,
                Date = startDate,
                ActivityKind = ActivityKind.Disbursement,
                Description = "Disbursement",
                Disbursement = request.Amount,
                NetCashFlow = -request.Amount
            };
            row.DiscountedNetCashFlow = row.NetCashFlow;
            row.OutstandingBalance = request.Amount;            
            result.Rows.Add(row);

            int ordinal = 1;
            bool addIntercalarToFirstInstallment = false;

            // Intercalary interest
            if (firstInterestDate != firstInstallmentDate && intercalarInterest != 0)
            {
                if (request.IntercalarInterestRepaymentType == IntercalarInterestRepaymentType.AfterIntercalarPeriod)
                {
                    row = new InstallmentPlanRow
                    {
                        Ordinal = ordinal,
                        Date = firstInterestDate,
                        Description = "Intercalar interest",
                        ActivityKind = ActivityKind.InterestPayment,
                        InterestRepayment = intercalarInterest,
                        OutstandingBalance = request.Amount,
                        StartingBalance = request.Amount,
                        Annuity = intercalarInterest,
                        NetCashFlow = intercalarInterest,
                        DiscountedNetCashFlow = intercalarInterest
                    };
                    result.Rows.Add(row);
                    ordinal++;
                }
                else if (request.IntercalarInterestRepaymentType == IntercalarInterestRepaymentType.WithDisbursement)
                {
                    row.AddInterest(intercalarInterest, "Intercalar interest");
                }
                else
                {
                    // As this row is still not created, we leave this for later
                    addIntercalarToFirstInstallment = true;
                }
            }

            DateTime d1 = repaymentPeriodStartDate;
            DateTime d2 = firstInstallmentDate;
            decimal outstanding = request.Amount;

            #region new way
            DateTime maturityDate = firstInstallmentDate.AddPeriod(request.InstallmentSchedule.FrequencyPeriod * (request.NumberOfInstallments - 1), request.InstallmentSchedule.FrequencyUnitOfTime);
            maturityDate = maturityDate.MoveTo(request.InstallmentSchedule.DayOfMonth); // correction for day of month 29..31

            SortedDictionary<DateTime, InstlCalcRow> calcDates = new SortedDictionary<DateTime, InstlCalcRow>();

            DateTime d = firstInterestDate;
            if (intercalarInterest != 0) // if there was intercalar interest calculated, then skip this date
            {
                d1 = firstInterestDate;
                d = d.AddPeriod(request.InterestSchedule.FrequencyPeriod, request.InterestSchedule.FrequencyUnitOfTime);
                d = d.MoveTo(request.InterestSchedule.DayOfMonth); // correction for day of month 29..31
            }

            while (d < maturityDate)
            {
                calcDates.Add(d, new InstlCalcRow() { HasInterestRepayment = true });
                d = d.AddPeriod(request.InterestSchedule.FrequencyPeriod, request.InterestSchedule.FrequencyUnitOfTime);
                d = d.MoveTo(request.InterestSchedule.DayOfMonth); // correction for day of month 29..31
            }

            d = firstInstallmentDate;
            while (d < maturityDate)
            {
                if (calcDates.ContainsKey(d))
                {
                    calcDates[d].HasPrincipalRepayment = true;

                } 
                else
                {
                    calcDates.Add(d, new InstlCalcRow() { HasInterestRepayment = request.ForceInterestWithInstallment, HasPrincipalRepayment = true });
                }
                d = d.AddPeriod(request.InstallmentSchedule.FrequencyPeriod, request.InstallmentSchedule.FrequencyUnitOfTime);
                d = d.MoveTo(request.InstallmentSchedule.DayOfMonth); // correction for day of month 29..31
            }

            // on maturity date we have to add interest and principal
            calcDates.Add(maturityDate, new InstlCalcRow() { HasInterestRepayment = true, HasPrincipalRepayment = true });

            decimal inrCalc = 0;
            decimal inrBase = 0;
            bool installmentAdjusted = false;
            bool suddenEnd = false;
            decimal minimalInstallment = 1; // TODO: read from configuration

            foreach (var calcDate in calcDates)
            {
                d2 = calcDate.Key;
                inrBase = outstanding;
                if (inr.IsCompound)
                {
                    inrBase = outstanding + inrCalc;
                }
                inrCalc = inrCalc + Math.Round(InterestCalculation.CalculateInterest(d1, d2, d2, inr.RatePercentage, inr.RateUnitOfTime.CharLiteral(), inr.IsCompound, inr.CalendarBasis, (double)inrBase).TotalInterest, 2, MidpointRounding.ToEven);

                decimal inrRepayment = 0;
                if (calcDate.Value.HasInterestRepayment)
                {
                    inrRepayment = inrCalc;
                    inrCalc = 0;
                }

                row = new InstallmentPlanRow
                {
                    Ordinal = ordinal,
                    Date = d2,
                    ActivityKind = ActivityKind.Repayment,
                    Description = "Repayment",
                    StartingBalance = outstanding,
                    InterestRepayment = inrRepayment
                };

                if (calcDate.Value.HasPrincipalRepayment)
                {
                    if (d2 == maturityDate) // if last installment
                    {
                        row.PrincipalRepayment = outstanding;
                    }
                    else if (request.AdjustFirstInstallment && !installmentAdjusted && d2 < maturityDate && request.RepaymentType == RepaymentType.FixedAnnuity)
                    {
                        // We calculate PrincipalRepayment using dummy interest from simplified installment plan where all periods are equal
                        var dummyDate = d2.AddPeriod(-request.InstallmentSchedule.FrequencyPeriod, request.InstallmentSchedule.FrequencyUnitOfTime);
                        var dummyInterest = InterestCalculation.CalculateInterest(dummyDate, d2, d2, inr.RatePercentage, inr.RateUnitOfTime.CharLiteral(), inr.IsCompound, inr.CalendarBasis, (double)outstanding).TotalInterest;
                        dummyInterest = Math.Round(dummyInterest, 2, MidpointRounding.ToEven);
                        row.PrincipalRepayment = annuity - dummyInterest;
                        installmentAdjusted = true;
                    }
                    else if (request.RepaymentType == RepaymentType.FixedAnnuity)
                    {
                        row.PrincipalRepayment = annuity - row.InterestRepayment;
                    }
                    else // fixed principal repayment
                    {
                        row.PrincipalRepayment = annuity;
                    }

                    if (row.PrincipalRepayment > outstanding - minimalInstallment)
                    {
                        // break before reaching maturityDate (for example when calculating term)
                        // minimalInstallment is used to avoid insignificant amounts in last installment
                        row.PrincipalRepayment = outstanding;
                        result.NumberOfInstallments = calcDates.Count(it => it.Key < d2 && it.Value.HasPrincipalRepayment) + 1;
                        maturityDate = d2;
                        suddenEnd = true;
                        // force interest repayment if there wasn't one...
                        row.InterestRepayment += inrCalc;
                        inrCalc = 0;
                    }
                }
                else
                {
                    row.PrincipalRepayment = 0;
                }

                row.Annuity = row.PrincipalRepayment + row.InterestRepayment + row.Fee;
                row.OutstandingBalance = outstanding -= row.PrincipalRepayment;
                row.NetCashFlow = row.PrincipalRepayment + row.InterestRepayment + row.Fee;
                row.DiscountedNetCashFlow = row.PrincipalRepayment + row.InterestRepayment + row.Fee;
                result.Rows.Add(row);

                if (suddenEnd)
                {
                    break;
                }

                ordinal++;
                d1 = d2;
            }

            #endregion

            if (addIntercalarToFirstInstallment)
            {
                var firstInstallment = result.Rows.FirstOrDefault(it => it.Date == firstInstallmentDate);
                if (firstInstallment != null)
                {
                    firstInstallment.AddInterest(intercalarInterest, "Intercalar interest");
                }
            }

            // Now fees
            if (request.Fees != null)
            {
                if (!request.FeeCurrencyConversionDone && !string.IsNullOrEmpty(request.Currency))
                {
                    AssecoCurrencyConvertion.CurrencyConverter currencyConverter = new AssecoCurrencyConvertion.CurrencyConverter();

                    foreach (var feeEntry in request.Fees)
                    {
                        if (!string.IsNullOrWhiteSpace(feeEntry.Currency) && feeEntry.Currency != request.Currency)
                        {
                            feeEntry.FixedAmountInDealCurrency = currencyConverter.CurrencyConvert(feeEntry.FixedAmount, feeEntry.Currency, request.Currency, request.StardDate.ToString("o", System.Globalization.CultureInfo.InvariantCulture), request.FeeCurrencyConversionMethod);
                            feeEntry.LowerLimitInDealCurrency = currencyConverter.CurrencyConvert(feeEntry.LowerLimit, feeEntry.Currency, request.Currency, request.StardDate.ToString("o", System.Globalization.CultureInfo.InvariantCulture), request.FeeCurrencyConversionMethod);
                            feeEntry.UpperLimitInDealCurrency = currencyConverter.CurrencyConvert(feeEntry.UpperLimit, feeEntry.Currency, request.Currency, request.StardDate.ToString("o", System.Globalization.CultureInfo.InvariantCulture), request.FeeCurrencyConversionMethod);
                        }
                        else
                        {
                            feeEntry.FixedAmountInDealCurrency = feeEntry.FixedAmount;
                            feeEntry.LowerLimitInDealCurrency = feeEntry.LowerLimit;
                            feeEntry.UpperLimitInDealCurrency = feeEntry.UpperLimit;
                        }
                    }
                }

                List<FeeEntry> newFeeDates = new List<FeeEntry>();
                foreach (var feeEntry in request.Fees.Where(it => it.Frequency != FeeConditionFrequency.EventTriggered))
                {
                    d = feeEntry.Date;
                    while (d < maturityDate)
                    {
                        switch (feeEntry.Frequency)
                        {
                            case FeeConditionFrequency.Monthly:
                                d = d.AddMonths(1);
                                break;
                            case FeeConditionFrequency.Quarterly:
                                d = d.AddMonths(3);
                                break;
                            case FeeConditionFrequency.Semiyearly:
                                d = d.AddMonths(6);
                                break;
                            case FeeConditionFrequency.Yearly:
                                d = d.AddYears(1);
                                break;
                            default:
                                break;
                        }

                        // TODO: check if d is last date and if it should be applied
                        bool applyFeeOnLastDate = false;

                        if ((d <= maturityDate) && (applyFeeOnLastDate || d < d1))
                        {
                            FeeEntry f = new FeeEntry
                            {
                                CalculationBasisType = feeEntry.CalculationBasisType,
                                Currency = feeEntry.Currency,
                                Date = d,
                                FixedAmount = feeEntry.FixedAmount,
                                Frequency = feeEntry.Frequency,
                                Kind = feeEntry.Kind,
                                LowerLimit = feeEntry.LowerLimit,
                                Name = feeEntry.Name,
                                Percentage = feeEntry.Percentage,
                                ServiceCode = feeEntry.ServiceCode,
                                TariffCode = feeEntry.TariffCode,
                                UpperLimit = feeEntry.UpperLimit,
                                FixedAmountInDealCurrency = feeEntry.FixedAmountInDealCurrency,
                                LowerLimitInDealCurrency = feeEntry.LowerLimitInDealCurrency,
                                UpperLimitInDealCurrency = feeEntry.UpperLimitInDealCurrency
                            };
                            newFeeDates.Add(f);
                        }
                    }
                }

                foreach (var feeEntry in request.Fees.Union(newFeeDates))
                {
                    row = result.Rows.Where(it => it.Date == feeEntry.Date).LastOrDefault();
                    if (row == null)
                    {
                        row = new InstallmentPlanRow();
                        var prevDate = result.Rows.Where(it => it.Date < feeEntry.Date).Max(it => it.Date);
                        int index = 0;
                        if (prevDate != null)
                        {
                            var prevRow = result.Rows.Where(it => it.Date == prevDate).LastOrDefault();
                            index = result.Rows.IndexOf(prevRow) + 1;
                            row.StartingBalance = prevRow.StartingBalance;
                            row.OutstandingBalance = prevRow.OutstandingBalance;
                        }
                        result.Rows.Insert(index, row);
                        row.Ordinal = 9999999; // we will have to renumerate this.
                        row.Description = feeEntry.Name;
                    }
                    else
                    {
                        row.Description = row.Description + ", " + feeEntry.Name;
                    }

                    decimal feeBasis = request.Amount;
                    if (feeEntry.CalculationBasisType == CalculationBasisType.AccountBalance)
                    {
                        feeBasis = row.OutstandingBalance; // as we calculate for future period
                    }

                    decimal fee = feeEntry.FixedAmountInDealCurrency + feeEntry.Percentage / 100 * feeBasis;
                    if (feeEntry.UpperLimitInDealCurrency > 0 && fee > feeEntry.UpperLimitInDealCurrency)
                    {
                        fee = feeEntry.UpperLimitInDealCurrency;
                    }
                    if (fee < feeEntry.LowerLimitInDealCurrency)
                    {
                        fee = feeEntry.LowerLimitInDealCurrency;
                    }

                    fee = Math.Round(fee, 2, MidpointRounding.ToEven);

                    row.Fee += fee;
                    row.NetCashFlow += fee;
                    row.DiscountedNetCashFlow = row.NetCashFlow;

                    // TODO: Add installment plan column OtherPayments (Druge isplate) to support this
                    /*
                    if (row.Disbursement > 0 && request.payFeeFromDisbursement)
                    {
                        row.Disbursement -= fee;
                        row.OtherPayments += fee;

                    }
                    */
                }
            }

            // TODO: Check if there are rows with row.Ordinal = 9999999 and renumerate

            // At the end, APR calculation
            List<NetCashFlowItem> cashFlow = result.Rows.Select(it => new NetCashFlowItem() { 
                Date = it.Date, 
                NetCashFlow = -it.NetCashFlow, 
                DiscountedNetCashFlow = -it.DiscountedNetCashFlow }).ToList();

            result.APR = InterestCalculation.CalculateEffectiveInterestRate(cashFlow);
            // Update DiscountedNetCashFlow
            foreach (var item in result.Rows)
            {
                item.DiscountedNetCashFlow = InterestCalculation.CalculateDiscountedNetCashFlow((double)item.NetCashFlow, (double)result.APR, item.Date, startDate);
            }

            return result;
        }

    }

    internal class InstlCalcRow
    {
        public bool HasInterestRepayment { get; set; }
        public bool HasPrincipalRepayment { get; set; }
    }


}
