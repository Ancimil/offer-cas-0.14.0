using System.Collections.Generic;
using PriceCalculation.Models.Pricing;

namespace PriceCalculation.Models.Product
{
    public class ProductConditions
    {
        public List<FeeCondition> Fees { get; set; } = new List<FeeCondition>();
        public List<InterestRateCondition> InterestRates { get; set; } = new List<InterestRateCondition>();
        public List<GeneralCondition> Other { get; set; } = new List<GeneralCondition>();
    }
}
