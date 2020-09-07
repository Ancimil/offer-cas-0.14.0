using Newtonsoft.Json;
using System;

namespace PriceCalculation.Models.MarketRates
{
    public class RateValue
    {
        public DateTime EffectiveDate { get; set; }
        [JsonIgnore]
        public Rate Rate { get; set; }
        public string RateCode { get; set; }
        public decimal Value { get; set; }
        public DateTime? PublishingDate { get; set; }
    }
}