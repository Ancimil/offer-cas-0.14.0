using System.Collections.Generic;

namespace PriceCalculation.Models.Pricing
{
    public class PriceCalculationResult
    {

        public List<InterestRateCondition> InterestRates { get; set; }
        public List<FeeCondition> Fees { get; set; }
        public List<GeneralCondition> OtherConditions { get; set; }
        public decimal? Napr { get; set; }
        public bool ResultChanged { get; set; }
    }
}
