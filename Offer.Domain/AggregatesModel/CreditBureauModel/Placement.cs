using System;
using System.Collections.Generic;

namespace Offer.Domain.AggregatesModel.CreditBureauModel
{
    public class Placement
    {
        public decimal? ActualLatencyAmount { get; set; }
        public decimal? Annuity { get; set; }
        public decimal? CreditAmount { get; set; }
        public String CurrencyClause { get; set; }
        public decimal? DebtAmount { get; set; }
        public Boolean IsActive { get; set; }
        public Boolean? IsRefinancing { get; set; }
        public List<Latency> Latencies { get; set; }
        public Int32 LatencyDaysNumber { get; set; }
        public decimal? MaxLatencyAmount { get; set; }
        public String Ordinal { get; set; }
        public Int32 OrdinalNumber { get; set; }
        public String PlacementKind { get; set; }
        public decimal? RefinancingAmount { get; set; }
        public DateTime? RepaymentEndDate { get; set; }
        public DateTime? RepaymentStartDate { get; set; }
        public String Status { get; set; }
        public String Kind { get; set; }
        public String Term { get; set; }
    }
}
