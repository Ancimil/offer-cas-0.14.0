using System;

namespace Offer.Domain.AggregatesModel.ExposureModel
{
    public class Balance
    {
        public string Currency { get; set; }
        public decimal Amount { get; set; }
        public decimal AmountLocal { get; set; }
        public string AccountNumber { get; set; }
        public string Direction { get; set; }
        public BalanceKinds BalanceKind { get; set; }
        public DateTime Calculated { get; set; }
    }
}
