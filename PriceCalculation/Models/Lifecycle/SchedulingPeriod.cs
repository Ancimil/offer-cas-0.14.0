namespace PriceCalculation.Models.Lifecycle
{
    public class SchedulingPeriod
    {
        public string PeriodType { get; set; } // corresponds to classification value of product/classification-schemes/scheduling-periods
        public LifecycleEvent StartEvent { get; set; }
        public string StartOffset { get; set; }
        public string MinimalLength { get; set; }
        public string MaximalLength { get; set; }
    }
}
