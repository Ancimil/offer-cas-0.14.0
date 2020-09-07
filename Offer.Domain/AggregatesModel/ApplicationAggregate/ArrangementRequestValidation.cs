using System.Collections.Generic;

namespace Offer.Domain.AggregatesModel.ApplicationAggregate
{
    public class BundleComponentInfo
    {
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public int MinimalNumberOfInstances { get; set; }
        public int MaximalNumberOfInstances { get; set; }
        public ArrangementKind Kind { get; set; }
    }
    public class ArrangementRequestValidation : BundleComponentInfo
    {
        public ProductSnapshot ProductSnapshot { get; set; }
        public int Count { get; set; }
        public int ParentCount { get; set; }
        public bool IsValid { get; set; }
        public bool IsAbstractOrigin { get; set; }
        public bool Enabled { get; set; }
        public List<ArrangementRequest> ArrangementRequests { get; set; }
    }
}
