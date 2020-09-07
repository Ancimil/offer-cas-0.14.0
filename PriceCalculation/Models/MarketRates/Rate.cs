using Newtonsoft.Json;

namespace PriceCalculation.Models.MarketRates
{

    public class Rate
    {
        public string Code { get; set; }
        public string Title { get; set; }
        // ISO 8601 duration
        public string Frequency { get; set; }
        [JsonIgnore]
        public RateList RateList;
        [JsonIgnore]
        public string ListCode;
    }
}