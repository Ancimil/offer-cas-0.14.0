using System;

namespace PriceCalculation.Models.Pricing
{
    public class ScheduledPeriod
    {
        public string PeriodType { get; set; } // corresponds to classification value of product/classification-schemes/scheduling-periods
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
