using System;

namespace Offer.Domain.AggregatesModel.CreditBureauModel
{
    public class Latency
    {
        public DateTime CreationTime { get; set; }
        public DateTime CreditBureauEntryDate { get; set; }
        public Decimal MaxAmount { get; set; }
        public Int32 NumberOfDays { get; set; }
        public String Ordinal { get; set; }
        public Boolean UnderComplaint { get; set; }
        public String Status { get; set; }

    }
}
