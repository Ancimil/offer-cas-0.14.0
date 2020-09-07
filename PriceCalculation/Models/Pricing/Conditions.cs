using System.Collections.Generic;

namespace PriceCalculation.Models.Pricing
{
    public class Conditions
    {
        public List<FeeCondition> Fees { get; set; }
        public List<InterestRateCondition> InterestRates { get; set; }
        public List<GeneralCondition> Other { get; set; }
    }
}
