using FinancialCalculations;
using Offer.Domain.Calculations;
using PriceCalculation.Models.Lifecycle;
using PriceCalculation.Models.Pricing;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Offer.Domain.AggregatesModel.ApplicationAggregate
{
    public partial class OverdraftFacilityRequest
    {
        public override void CalculateOffer(PriceCalculationParameters calculationParameters, OfferPriceCalculation priceCalculator, string feeCurrencyConversionMethod)
        {
            var ignore = priceCalculator.CalculatePrice(this, calculationParameters).Result;

            // isCompound i calendarBasis izvuci tokom CalculatePrice da dodatno opisu Napr
            bool isCompound = false;
            CalendarBasisKind calendarBasis = CalendarBasisKind.CalendarActActISDA;
            double yearlyRate = Convert.ToDouble(Napr) / 100;

            var ir = Conditions.InterestRates.FirstOrDefault(it => it.Kind == InterestRateKinds.RegularInterest);
            if (ir != null)
            {
                isCompound = ir.IsCompound;
                calendarBasis = ir.CalendarBasis;
                yearlyRate = Convert.ToDouble(ir.CalculatedRate);
                Napr = Convert.ToDecimal(yearlyRate);
            }


            var startDate = calculationParameters.RequestDate ?? DateTime.Today;

            List<InstallmentPlanRow> rows = new List<InstallmentPlanRow>();
            List<FeeCondition> rowFeeList = new List<FeeCondition>();
            List<FeeCondition> fees = Conditions.Fees;
            fees = FeeCalculation.PrepareFees(fees, Currency, feeCurrencyConversionMethod);

            decimal runningAmount = Math.Round(Amount, 2);
            DateTime runningDate = startDate;
            if (CalculationDate > DateTime.Today)
            {
                CalculationDate = runningDate;
            }
            else
            {
                runningDate = CalculationDate ?? DateTime.Today;
            }

            // SLOBA: Fee scheduling, and scheduling in general has to be improved in advanced calculator.
            decimal totalFeeAmount = 0;
            if (fees != null)
            {
                List<FeeCondition> feeList = fees.FindAll(f => f.Frequency == FeeConditionFrequency.EventTriggered && f.EffectiveDate <= DateTime.Today && f.Currencies.Contains(Currency));
                totalFeeAmount = Math.Round(FeeCalculation.CalculateFee(Amount, feeList), 4);
            }
            NumberOfInstallments = 1;

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
            int i = -1;

            InstallmentPlanRow rowInterest;
            var intrStartDate = startDate;

            runningDate = runningDate.AddMonths(1);
            while (runningDate <= MaturityDate.Value)
            {
                i++;


                var interest = Math.Round(InterestCalculation.CalculateInterest(intrStartDate, runningDate, runningDate, Convert.ToDouble(yearlyRate), 'Y', isCompound, calendarBasis, Convert.ToDouble(runningAmount))
                    .Sum(it => it.Interest), 2);

                rowInterest = new InstallmentPlanRow();

                rowInterest.Ordinal = i + 1;
                rowInterest.Date = runningDate;
                rowInterest.ActivityKind = ActivityKind.Repayment;
                rowInterest.Description = "Interest and fee repayment";
                rowInterest.InterestRepayment = Convert.ToDecimal(interest);
                rowInterest.PrincipalRepayment = 0;
                rowInterest.Annuity = rowInterest.InterestRepayment;
                rowInterest.StartingBalance = Math.Round(runningAmount, 2);
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
                rowInterest.YearFrac = Convert.ToDecimal(InterestCalculation.YearFrac(startDate, runningDate)); //, calendarBasis);
                rows.Add(rowInterest);
                intrStartDate = runningDate;
                runningDate = runningDate.AddMonths(1);
            }

            i++;

            rowInterest = new InstallmentPlanRow();

            rowInterest.Ordinal = i + 1;
            rowInterest.Date = runningDate;
            rowInterest.ActivityKind = ActivityKind.Repayment;
            rowInterest.Description = "Principal repayment";
            rowInterest.InterestRepayment = 0;
            rowInterest.PrincipalRepayment = Amount;
            rowInterest.Annuity = Amount;
            rowInterest.StartingBalance = Amount;
            rowInterest.OutstandingBalance = 0;

            #region Fee calculation
            rowInterest.Fee = 0;
            #endregion
            rowInterest.NetCashFlow = rowInterest.Disbursement - rowInterest.PrincipalRepayment - rowInterest.InterestRepayment - rowInterest.Fee - rowInterest.OtherExpenses;
            rowInterest.YearFrac = Convert.ToDecimal(InterestCalculation.YearFrac(startDate, runningDate)); //, calendarBasis);
            rows.Add(rowInterest);
            NumberOfInstallments = 1;

            // arrangementRequest.InstallmentPlan = rows;
            CalculateAPR(rows);
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
            foreach (InstallmentPlanRow planRow in rows)
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
