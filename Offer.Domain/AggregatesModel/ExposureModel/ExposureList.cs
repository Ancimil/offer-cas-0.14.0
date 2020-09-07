using System.Collections.Generic;

namespace Offer.Domain.AggregatesModel.ExposureModel
{
    public class ExposureList
    {
        public List<Exposure> Exposures { get; set; }
        public decimal TotalApprovedAmountInTargetCurrency { get; set; }
        public decimal TotalOutstandingAmountInTargetCurrency { get; set; }

    }
}
