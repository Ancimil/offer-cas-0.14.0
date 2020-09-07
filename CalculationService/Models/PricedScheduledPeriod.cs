using CalculationService.Services;
using System;

namespace CalculationService.Models
{
    public class PricedScheduledPeriod
    {
        public string PeriodType { get; set; } // corresponds to classification value of product/classification-schemes/scheduling-periods
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public double Percentage { get; set; }
        public SimpleUnitOfTime UnitOfTime { get; set; }
    }
}
