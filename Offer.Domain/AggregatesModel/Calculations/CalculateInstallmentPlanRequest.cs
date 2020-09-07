using Offer.Domain.AggregatesModel.ApplicationAggregate;
using System;

namespace Offer.Domain.AggregatesModel.Calculations
{
    public class CalculateInstallmentPlanRequest
    {
        public double? Amount { get; set; }
        public string Term { get; set; }
        public DateTime? StartDate { get; set; }
        public double? InterestRate { get; set; }
        public double? OriginationFeePercentage { get; set; }
        public double? OriginationFeeAmount { get; set; }
        public double? FixedAnnuity { get; set; }
        public ArrangementRequest ArrangementRequest { get; set; }
    }
}
