using System.Collections.Generic;

namespace PriceCalculation.Models.Pricing
{
    public class PriceCalculationResponse
    {
        public List<FeeCondition> Fees { get; set; }
        public List<InterestRateCondition> InterestRates { get; set; }
        public decimal? Napr { get; set; }
        // public string Currency { get; set; }
        // public decimal Amount { get; set; }
        // public string Term { get; set; }
        // public string CustomerSegment { get; set; }
        // public string Channel { get; set; }
        // public decimal? RiskScore { get; set; }
        // public string PartOfBundle { get; set; }
        // public string CollateralModel { get; set; }
    }
}
