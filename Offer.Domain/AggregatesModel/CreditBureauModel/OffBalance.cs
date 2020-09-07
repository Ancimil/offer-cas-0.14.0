using System;

namespace Offer.Domain.AggregatesModel.CreditBureauModel
{
    public class OffBalance
    {
        public string Ordinal { get; set; }
        public string PlacementKind { get; set; }

        public DateTime DateFirstCheckIn { get; set; }
        public decimal? DebtAmount { get; set; }
        public string CurrencyClause { get; set; } = "/";
        public string Status { get; set; }
        public Boolean UnderComplaint { get; set; }
        public DateTime CreditBureauEntryDate { get; set; }      

    }
}
