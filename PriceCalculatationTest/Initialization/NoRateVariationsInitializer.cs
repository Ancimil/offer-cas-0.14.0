using PriceCalculation.Models.Pricing;
using System;
using System.Collections.Generic;

namespace PriceCalculatationTest.Initialization
{
    public partial class PriceCalculationInitializer
    {
        private static PriceCalculationParameters NoVariationsSingleRate(decimal spread)
        {
            var p = Parameters;
            p.InterestRates = new List<InterestRateCondition>
            {
                new InterestRateCondition
                {
                    Currencies = new List<string>{"RSD"},
                    Kind = InterestRateKinds.RegularInterest,
                    EffectiveDate = DateTime.Today,
                    Rate = new InterestRate
                    {
                        SpreadRateValue = spread
                    },

                }
            };
            return p;
        }
    }
}
