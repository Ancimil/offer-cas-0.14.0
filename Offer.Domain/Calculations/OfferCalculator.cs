using System;
using System.Collections.Generic;
using System.Linq;
using Offer.Domain.Calculations;
using FinancialCalculations;
using PriceCalculation.Models.Pricing;
using PriceCalculation.Calculations;
using PriceCalculation.Models.Lifecycle;

namespace Offer.Domain.AggregatesModel.ApplicationAggregate
{
    public partial class TermLoanRequest
    {
        public override void CalculateOffer(Application application, OfferPriceCalculation priceCalculator, string conversionMethod)
        {
            //if (!ValidateParameters(this))
            //{
            //    // do something man !!!
            //}
            // 
            _ = priceCalculator.CalculatePrice(application, this).Result;
            CalculateInstallmentPlan(conversionMethod);
        }
        public override void CalculateOffer(PriceCalculationParameters calculationParameters,
            OfferPriceCalculation priceCalculator, string conversionMethod)
        {
            _ = priceCalculator.CalculatePrice(this, calculationParameters).Result;
            CalculateInstallmentPlan(conversionMethod);
        }

        private static bool ValidateParameters(TermLoanRequest arrangementRequest)
        {
            var minDownpaymentPercentage = arrangementRequest.ProductSnapshot.MinimalDownpaymentPercentage;
            var invoiceAmount = arrangementRequest.InvoiceAmount;
            var downpaymentAmount = arrangementRequest.DownpaymentAmount;

            return (minDownpaymentPercentage > 0 &&
                (invoiceAmount <= 0 || ((minDownpaymentPercentage * invoiceAmount) / 100 < downpaymentAmount)));
        }

        public void ResolveCalculationParameters()
        {
            if (Term == null)
            {
                // calculate term from amount and annuity
                // SLOBA: arrangementRequest.Napr / 12 should be changed to appropriate formula due to interest calculation method. For compound it should be POWER(1 + arrangementRequest.Napr/100, 1/12) - 1
                int term = SolveForPeriods(Amount, Napr / 12, Annuity);
                Term = "P" + term.ToString() + "M";
                return;
            }
            else
            {
                int term = Utility.GetMonthsFromPeriod(Term);
                if (Amount == 0)
                {
                    // calculate amount from term and annuity
                    // SLOBA: arrangementRequest.Napr / 12 should be changed to appropriate formula due to interest calculation method. For compound it should be POWER(1 + arrangementRequest.Napr/100, 1/12) - 1
                    Amount = SolveForAmount(term, Napr / 12, Annuity);
                    return;
                }
                else
                {
                    // calculate annuity from term and amount
                    // SLOBA: arrangementRequest.Napr / 12 should be changed to appropriate formula due to interest calculation method. For compound it should be POWER(1 + arrangementRequest.Napr/100, 1/12) - 1
                    Annuity = SolveForInstallment(Amount, term, Napr / 12);
                    return;
                }
            }
        }
        /* this method return 0 when all four parameters  are "good" */
        public static decimal CalculateOutstandingBalance(decimal amount, int periods, decimal interestRatePerPeriod, decimal installment)
        {
            decimal interest = 0;
            decimal current = amount;
            decimal currentInterest = 0;
            for (var i = 0; i < periods; i++)
            {
                currentInterest = Math.Round(current * interestRatePerPeriod / 100, 2);
                interest = currentInterest + interest;
                current = current - (installment - currentInterest);
            }
            return current;
        }

        /* Period can't be precise as other parameters 
        It is integer value..*/
        public static int SolveForPeriods(decimal amount, decimal interestRatePerPeriod, decimal installment)
        {
            int periods = 0;
            decimal runningAmount = amount;
            decimal interest = 0;
            while (runningAmount > 10)
            {
                interest = Math.Round(runningAmount * interestRatePerPeriod / 100, 2);
                runningAmount = runningAmount - (installment - interest);
                periods = periods + 1;
            }
            return periods;
        }

        public static decimal SolveForAmount(int periods, decimal interestRatePerPeriod, decimal installment)
        {
            decimal diff = 0.01M;
            int max_iteration = 10;
            int iteration = 0;
            decimal guessn1 = installment / (1 + interestRatePerPeriod / 100) * periods;
            decimal guessn2 = installment * (1 + interestRatePerPeriod / 100) * periods;
            decimal guessn = 0;
            while (iteration < max_iteration)
            {
                iteration = iteration + 1;
                var f1 = CalculateOutstandingBalance(guessn1, periods, interestRatePerPeriod, installment);
                var f2 = CalculateOutstandingBalance(guessn2, periods, interestRatePerPeriod, installment);
                if (Math.Abs(f1) < diff)
                {
                    return Math.Round(guessn1 * 100) / 100;
                }
                if (Math.Abs(f2) < diff)
                {
                    return Math.Round(guessn2 * 100) / 100;
                }
                guessn = (guessn2 * f1 - guessn1 * f2) / (f1 - f2);
                guessn2 = guessn1;
                guessn1 = guessn;
            }
            return Math.Round(guessn * 100) / 100;

        }
        
        // SLOBA: not used. What is this for?
        public static decimal SolveForRate(decimal amount,int periods, decimal installment)
        {
            decimal diff = 0.01M;
            int max_iteration = 100;
            int iteration = 0;
            decimal guessn1 = 0;
            decimal guessn2 = 5;
            decimal guessn = 0;
            while (iteration < max_iteration)
            {
                iteration = iteration + 1;

                decimal f1 = CalculateOutstandingBalance(amount, periods, guessn1, installment);
                decimal f2 = CalculateOutstandingBalance(amount, periods, guessn2, installment);
                if (Math.Abs(f1) < diff)
                {
                    return guessn1;
                }

                if (Math.Abs(f2) < diff)
                {
                    return guessn2;
                }
                guessn = (guessn2 * f1 - guessn1 * f2) / (f1 - f2);
                guessn2 = guessn1;
                guessn1 = guessn;
            }
            return Math.Round(guessn * 100) / 100;

        }

        public static decimal SolveForInstallment(decimal amount, int periods, decimal interestRatePerPeriod)
        {

            decimal diff = 0.001M;
            int max_iteration = 10;
            int iteration = 0;
            decimal guessn1 = 0;
            decimal guessn2 = amount * (1 + interestRatePerPeriod);
            decimal guessn = 0;
            while (iteration < max_iteration)
            {

                iteration = iteration + 1;
                var f1 = CalculateOutstandingBalance(amount, periods, interestRatePerPeriod, guessn1);
                var f2 = CalculateOutstandingBalance(amount, periods, interestRatePerPeriod, guessn2);
                if (Math.Abs(f2) < diff)
                {
                    return Math.Round(guessn2 * 100) / 100;
                }
                if (Math.Abs(f1) < diff)
                {
                    return Math.Round(guessn1 * 100) / 100;
                }
                guessn = (guessn2 * f1 - guessn1 * f2) / (f1 - f2);
                guessn2 = guessn1;
                guessn1 = guessn;
            }
            return Math.Round(guessn * 100) / 100;
        }

        public decimal MonthlyRate()
        {
            bool isCompound = false;
            CalendarBasisKind calendarBasis = CalendarBasisKind.CalendarActActISDA;
            double yearlyRate = 0;
            var ir = Conditions.InterestRates.FirstOrDefault(it => it.Kind == InterestRateKinds.RegularInterest);
            if (ir != null)
            {
                isCompound = ir.IsCompound;
                calendarBasis = ir.CalendarBasis;
                yearlyRate = Convert.ToDouble(ir.CalculatedRate / 100);
            }

            double monthlyRate = yearlyRate / 12;
            if (isCompound)
            {
                monthlyRate = Math.Pow(1 + yearlyRate, 1 / 12) - 1;
            }

            return Convert.ToDecimal(monthlyRate);
        }

        // SLOBA: term and fees parameters are inconsistently extracted from arrangementRequest
        public void CalculateInstallmentPlan(string feeCurrencyConversionMethod)
        {
            int term = Utility.GetMonthsFromPeriod(Term);
            List<InstallmentPlanRow> rows = new List<InstallmentPlanRow>();
            List<FeeCondition> rowFeeList = new List<FeeCondition>();
            List<FeeCondition> fees = Conditions.Fees;
            fees = FeeCalculation.PrepareFees(fees, Currency, feeCurrencyConversionMethod);
            
            decimal runningAmount = Math.Round(Amount, 2);
            DateTime runningDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day, 0, 0, 0, 0);
            if (CalculationDate > DateTime.Today)
            {
                CalculationDate = runningDate;
            }
            else
            {
                runningDate = CalculationDate ?? DateTime.Today;
            }
            DateTime startDate = runningDate;

            // SLOBA: Fee scheduling, and scheduling in general has to be improved in advanced calculator.
            decimal totalFeeAmount = 0;
            if (fees != null)
            {
                List<FeeCondition> feeList = fees.FindAll(f => f.Frequency == FeeConditionFrequency.EventTriggered && f.EffectiveDate <= DateTime.Today && f.Currencies.Contains(Currency));
                totalFeeAmount = Math.Round(FeeCalculation.CalculateFee(Amount, feeList), 4);
            }
            NumberOfInstallments = 1;

            bool isCompound = false;
            CalendarBasisKind calendarBasis = CalendarBasisKind.CalendarActActISDA;
            double yearlyRate = 0;
            var ir = Conditions.InterestRates.FirstOrDefault(it => it.Kind == InterestRateKinds.RegularInterest);
            if (ir != null)
            {
                isCompound = ir.IsCompound;
                calendarBasis = ir.CalendarBasis;
                yearlyRate = Convert.ToDouble(ir.CalculatedRate / 100);
            }

            // this is huge simplification. not good
            double monthlyRate = yearlyRate / 12;
            if (isCompound)
            {
                monthlyRate = Math.Pow(1 + yearlyRate, 1.0000000 / 12.0000000) - 1;
            }

            // SLOBA: Logic for calculating cash flow is done in manner that is later hard to extend. Also columns are hardcoded. 
            // SLOBA: This is totally ok for basic calculator though

            /* Disbursment */
            InstallmentPlanRow rowDisbursement = new InstallmentPlanRow();
            rowDisbursement.Date = runningDate;
            rowDisbursement.Ordinal = 0;
            rowDisbursement.ActivityKind = ActivityKind.Disbursement;
            rowDisbursement.Description = "Loan disbursement";
            rowDisbursement.Disbursement = Amount;
            rowDisbursement.StartingBalance = 0;
            rowDisbursement.OutstandingBalance = Amount;
            rowDisbursement.PrincipalRepayment = 0;
            rowDisbursement.InterestRepayment = 0;
            rowDisbursement.NetCashFlow = Amount - totalFeeAmount;
            rowDisbursement.Fee = totalFeeAmount;
            rowDisbursement.YearFrac = 0;
            rows.Add(rowDisbursement);

            for (var i = 0; i < term; i++)
            {
                decimal interest = Math.Round(runningAmount * Convert.ToDecimal(monthlyRate), 2);
                decimal principal = Math.Round(Annuity - interest, 2);

                if (i == term - 1)
                { // last ... corection for decimal places
                    principal = Math.Round(runningAmount, 2);
                    interest = Math.Round(Annuity - principal, 2);
                }

                InstallmentPlanRow rowInterest = new InstallmentPlanRow();

                rowInterest.Ordinal = i + 1;
                runningDate = runningDate.AddMonths(1);
                rowInterest.Date = runningDate;
                rowInterest.ActivityKind = ActivityKind.Repayment;
                rowInterest.Description = "Anuity repayment";
                rowInterest.InterestRepayment = interest;
                rowInterest.PrincipalRepayment = principal;
                rowInterest.Annuity = Annuity;
                rowInterest.StartingBalance = Math.Round(runningAmount, 2);
                runningAmount = (runningAmount - principal);
                rowInterest.OutstandingBalance = Math.Round(runningAmount, 2);

                #region Fee calculation
                if (fees != null)
                {
                    decimal totalRowFee = 0;
                    rowFeeList = fees.FindAll(f => f.Frequency == FeeConditionFrequency.Monthly && f.EffectiveDate <= DateTime.Today && f.Currencies.Contains(Currency));
                    if (rowFeeList.Count > 0)
                        totalRowFee += FeeCalculation.CalculateFee(runningAmount, rowFeeList);
                    if ((i + 1) % 4 == 0)
                    {
                        rowFeeList = fees.FindAll(f => f.Frequency == FeeConditionFrequency.Quarterly && f.EffectiveDate <= DateTime.Today && f.Currencies.Contains(Currency));
                        if (rowFeeList.Count > 0)
                            totalRowFee += FeeCalculation.CalculateFee(runningAmount, rowFeeList);
                    }
                    if ((i + 1) % 6 == 0)
                    {
                        rowFeeList = fees.FindAll(f => f.Frequency == FeeConditionFrequency.Semiyearly && f.EffectiveDate <= DateTime.Today && f.Currencies.Contains(Currency));
                        if (rowFeeList.Count > 0)
                            totalRowFee += FeeCalculation.CalculateFee(runningAmount, rowFeeList);
                    }
                    if ((i + 1) % 12 == 0)
                    {
                        rowFeeList = fees.FindAll(f => f.Frequency == FeeConditionFrequency.Yearly && f.EffectiveDate <= DateTime.Today && f.Currencies.Contains(Currency));
                        if (rowFeeList.Count > 0)
                            totalRowFee += FeeCalculation.CalculateFee(runningAmount, rowFeeList);
                    }
                    rowInterest.Fee = Math.Round(totalRowFee, 4);
                }
                #endregion
                rowInterest.NetCashFlow = rowInterest.Disbursement - rowInterest.PrincipalRepayment - rowInterest.InterestRepayment - rowInterest.Fee - rowInterest.OtherExpenses;
                rowInterest.YearFrac = Yearfrac(startDate, runningDate, calendarBasis);
                rows.Add(rowInterest);
                NumberOfInstallments += 1;
            }
            // arrangementRequest.InstallmentPlan = rows;
            CalculateAPR(rows);
        }
        
        private static decimal Yearfrac(DateTime startDate, DateTime endDate, CalendarBasisKind dayCount)
        {
            decimal nbDaysInPeriod = (decimal)(endDate - startDate).Days;

            switch (dayCount)
            {
                case (CalendarBasisKind.CalendarAct360):
                    return nbDaysInPeriod / (decimal)360;
                case (CalendarBasisKind.CalendarAct365L):
                case (CalendarBasisKind.CalendarAct365Fixed):
                    return nbDaysInPeriod / (decimal)365;
                case (CalendarBasisKind.Calendar30A360):
                case (CalendarBasisKind.Calendar30E360):
                    decimal result = (endDate.Year - startDate.Year) * 360 + (endDate.Month - startDate.Month) * 30 + (Math.Min(endDate.Day, 30) - Math.Min(startDate.Day, 30));
                    return result / 360;
                default:
                    return GetActAct(startDate, endDate);
            }
        }

        private static decimal GetActAct(DateTime startDate, DateTime endDate)
        {
            decimal nbDaysInPeriod = (decimal)(endDate - startDate).Days;
            if (startDate.Year == endDate.Year || (endDate.Year - 1 == startDate.Year && (startDate.Month > endDate.Month || startDate.Month == endDate.Month && (startDate.Day >= endDate.Day))))
            {
                decimal dayNum = 365;
                if (startDate.Year == endDate.Year && DateTime.IsLeapYear(startDate.Year))
                {
                    dayNum++;
                }
                else
                {

                    if (endDate.Day == 29 && endDate.Month == 2)
                    {
                        dayNum++;
                    }
                    else
                    {
                        if (DateTime.IsLeapYear(startDate.Year))
                        {
                            var feb = new DateTime(startDate.Year, 2, 29);
                            if (startDate <= feb && feb <= endDate) dayNum++;
                        }
                        else
                        {
                            if (DateTime.IsLeapYear(endDate.Year))
                            {
                                var feb = new DateTime(endDate.Year, 2, 29);
                                if (startDate <= feb && feb <= endDate) dayNum++;
                            }
                        }
                    }
                }
            }
            else
            {
                int nbYears = endDate.Year - startDate.Year + 1;
                decimal dayNum = nbYears * 365;
                for (var i = 0; i < nbYears; i++)
                {
                    if (DateTime.IsLeapYear(startDate.Year + i)) dayNum++;
                }
                dayNum /= nbYears;
                return nbDaysInPeriod / dayNum;
            }
            return nbDaysInPeriod / 365;
        }

        private decimal CalculateAPR(List<InstallmentPlanRow> rows)
        {
            decimal aprInterestRate = Napr;
            decimal diff1 = 0;
            decimal diff2 = 0;
            decimal rate1 = Napr;
            decimal rate2 = Napr + 1;
            bool changeSign = false;
            int maxIteration = 100;
            int i = 0;
            foreach(InstallmentPlanRow planRow in rows)
            {
                planRow.DiscountedNetCashFlow = (decimal)Math.Round((Math.Pow((double)(1 + (rate1 / 100)), -(double)planRow.YearFrac) * (double)planRow.NetCashFlow), 2);
                diff1 = diff1 + planRow.DiscountedNetCashFlow;
            }
            if (Math.Sign((double)rows[0].NetCashFlow) == Math.Sign(diff1))
            {
                throw new System.Exception("Annual percentage rate is in infinite.");
            }
            while (!changeSign && i < maxIteration)
            {
                i++;
                diff2 = 0;
                foreach (InstallmentPlanRow planRow in rows)
                {
                    planRow.DiscountedNetCashFlow = (decimal)Math.Round((Math.Pow((double)(1 + (rate2 / 100)), -(double)planRow.YearFrac) * (double)planRow.NetCashFlow), 2);
                    diff2 = diff2 + planRow.DiscountedNetCashFlow;
                }
                if ((Math.Sign(diff1) != Math.Sign(diff2)))
                {
                    changeSign = true;
                }
                else if (Math.Abs(diff2) > Math.Abs(diff1))
                    rate2 = rate2 - 1;
                else
                {
                    rate2 = rate2 + 1;
                    diff1 = diff2;
                }
                aprInterestRate = rate2;
            }
            decimal nextRate = 0;
            while (Math.Abs(Math.Round(diff1, 4)) > 0 && i < maxIteration)
            {
                decimal nextDiff = 0;
                i++;
                nextRate = (rate1 + rate2) / 2;

                foreach (InstallmentPlanRow planRow in rows)
                {
                   planRow.DiscountedNetCashFlow = (decimal)Math.Round((Math.Pow((double)(1 + (nextRate / 100)), -(double)planRow.YearFrac) * (double)planRow.NetCashFlow), 2);
                   nextDiff = nextDiff + planRow.DiscountedNetCashFlow;
                }

                if ((Math.Sign(diff1) == Math.Sign(nextDiff)))
                {
                    diff1 = nextDiff;
                    rate1 = nextRate;
                }
                else
                {
                    diff2 = nextDiff;
                    rate2 = nextRate;
                }
                aprInterestRate = nextRate;
            }
            if (i > maxIteration)
            {
                throw new System.Exception("Maximal number of iterations exited in annual percentage rate calculation.");
            }

            aprInterestRate = Math.Round(aprInterestRate, 4);
            Eapr = aprInterestRate;
            InstallmentPlan = rows;
            return aprInterestRate;

        }
    }
}
