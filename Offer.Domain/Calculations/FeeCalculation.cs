using AssecoCurrencyConvertion;
using PriceCalculation.Models.Pricing;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace FinancialCalculations
{
    public static class FeeCalculation
    {
        public static List<FeeCondition> PrepareFees(List<FeeCondition> fees, string creditCurrency, string conversionMethod)
        {
            CurrencyConverter currencyConverter = new CurrencyConverter();
            foreach (FeeCondition fee in fees)
            {
                if (fee.PercentageLowerLimit.HasValue)
                {
                    if (fee.CalculatedPercentage < fee.PercentageLowerLimit.Value) fee.CalculatedPercentage = fee.PercentageLowerLimit.Value;
                }
                else if (fee.PercentageUpperLimit.HasValue)
                {
                    if (fee.CalculatedPercentage > fee.PercentageUpperLimit) fee.CalculatedPercentage = fee.PercentageUpperLimit.Value;
                }

                if (fee.FixedAmount != null)
                {
                    if (!fee.FixedAmount.Code.Equals(creditCurrency))
                    {
                        fee.FixedAmountInCurrency = currencyConverter.CurrencyConvert(fee.FixedAmount.Amount, fee.FixedAmount.Code, creditCurrency, DateTime.Today.ToString("o", CultureInfo.InvariantCulture), conversionMethod);
                    }
                    else
                    {
                        fee.FixedAmountInCurrency = fee.FixedAmount.Amount;
                    }
                }
            }
            return fees;
        }

        public static decimal CalculateFee(decimal creditAmount, List<FeeCondition> fees)
        {
            decimal totalFeeAmount = 0;
            CurrencyConverter currencyConverter = new CurrencyConverter();

            foreach (FeeCondition fee in fees)
            {
                decimal feeAmount = 0;
                if (fee.CalculatedPercentage > 0)
                {
                    feeAmount += (creditAmount * fee.CalculatedPercentage) / 100;
                    totalFeeAmount += feeAmount;
                }
                if (fee.FixedAmountInCurrency > 0)
                {
                    totalFeeAmount += fee.FixedAmountInCurrency;
                }

            }
            return totalFeeAmount;
        }

    }
}
