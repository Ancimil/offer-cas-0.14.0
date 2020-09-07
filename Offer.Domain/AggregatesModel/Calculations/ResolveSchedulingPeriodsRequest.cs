using PriceCalculation.Models.Lifecycle;
using System;
using System.Collections.Generic;

namespace Offer.Domain.AggregatesModel.Calculations
{
    public class ResolveSchedulingPeriodsRequest
    {
        public List<SchedulingPeriod> SchedulingPeriods { get; set; }
        public string Term { get; set; }
        public DateTime? CalculationDate { get; set; } = DateTime.Now;
        public DateTime? RequestDate { get; set; }
        public DateTime? ApprovalDate { get; set; }
        public DateTime? SigningDate { get; set; }
        public DateTime? DisbursmentDate { get; set; }
        public DateTime? FirstInstallmentDate { get; set; }
        public DateTime? MaturityDate { get; set; }
    }
}
