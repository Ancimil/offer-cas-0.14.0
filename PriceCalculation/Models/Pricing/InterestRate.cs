using System.ComponentModel.DataAnnotations;

namespace PriceCalculation.Models.Pricing
{
    public class InterestRate
    {
        [MaxLength(256)]
        public string BaseRateId { get; set; }
        public decimal? BaseRateValue { get; set; }
        public decimal? SpreadRateValue { get; set; }
    }
}
