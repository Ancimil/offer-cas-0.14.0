namespace Offer.Domain.AggregatesModel.ApplicationAggregate.AlternativeOfferModel
{
    public class AlternativeOffer
    {
        public decimal Amount { get; set; }
        public decimal Annuity { get; set; }
    }

    public class RequestedValues
    {
        public decimal Amount { get; set; }
        public decimal Annuity { get; set; }
        public string Term { get; set; }
        public decimal Napr { get; set; }
        public decimal? DownpaymentAmount { get; set; }
        public decimal? DownpaymentPercentage { get; set; }
        public decimal? InvoiceAmount { get; set; } 
    }

    public class ApprovedLimits
    {
        public decimal Amount { get; set; }
        public decimal Annuity { get; set; }
        public decimal? DownpaymentAmount { get; set; }
        public decimal? DownpaymentPercentage { get; set; }
        public decimal? InvoiceAmount { get; set; }
        public decimal MaximalAffordableAnnuity { get; set; }
        public decimal MaximalAmount { get; set; }
        public string MaximalTerm { get; set; }
        public decimal MinimalAmount { get; set; }
        public string MinimalTerm { get; set; }
        public string Passed { get; set; }
        public string Term { get; set; }
    }

    public class AcceptedValues
    {
        public decimal Amount { get; set; }
        public decimal Annuity { get; set; }
        public string Term { get; set; }
        public decimal Napr { get; set; }
        public decimal? DownpaymentAmount { get; set; }
        public decimal? DownpaymentPercentage { get; set; } 
        public decimal? InvoiceAmount { get; set; } 
    }
}
