using Offer.Domain.AggregatesModel.ApplicationAggregate;

namespace Offer.Domain.AggregatesModel.ExposureModel
{
    public class Exposure
    {
        public string PartyId { get; set; }
        public ArrangementKind? ArrangementKind { get; set; }
        public string AccountNumber { get; set; }
        public string RiskCategory { get; set; }
        public string Currency { get; set; }
        public decimal AnnuityInSourceCurrency { get; set; }
        public decimal AnnuityInTargetCurrency { get; set; }
        public decimal ExposureApprovedInSourceCurrency { get; set; }
        public decimal ExposureApprovedInTargetCurrency { get; set; }
        public decimal ExposureOutstandingAmountInSourceCurrency { get; set; }
        public decimal ExposureOutstandingAmountInTargetCurrency { get; set; }

        public string CustomerName { get; set; }
        public string Term { get; set; }
        public bool? isBalance { get; set; }
    }
}
