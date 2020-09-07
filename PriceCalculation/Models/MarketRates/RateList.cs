using System.Collections.Generic;

namespace PriceCalculation.Models.MarketRates
{
    public class RateList
    {
        public string ListCode { get; set; }
        public string SyncSource { get; set; }
        public long SyncTimestamp { get; set; }
        public List<Rate> Rates { get; set; }
    }
}