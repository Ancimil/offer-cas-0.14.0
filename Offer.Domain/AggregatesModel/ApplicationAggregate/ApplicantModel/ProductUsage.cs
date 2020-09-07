namespace Offer.Domain.AggregatesModel.ApplicationAggregate
{
    public class ProductUsage
    {
        public long ApplicationId { get; set; } // references parent app number, part of key
        public long ProductUsageId { get; set; }
        public int ProductsUsed { get; set; }
        public int AccountsUsed { get; set; }
        public decimal AverageLiabilities { get; set; }
        public decimal AverageDeposits { get; set; }
        public decimal CreditLimitUtilization { get; set; }
        public decimal TotalArrears { get; set; }
        public bool ReceivesSalary { get; set; } // only individuals
        public bool UsesCurrentAccount { get; set; }
        public bool UsesCreditCard { get; set; }
        public bool UsesOverdraft { get; set; }
        public bool UsesMortgage { get; set; }
        public bool UsesConsumerCredit { get; set; } // only individuals, should be UsesConsumerLoans
        public int MaxHistoricalArrearDays { get; set; }
    }
}
