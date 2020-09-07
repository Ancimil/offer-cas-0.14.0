namespace Offer.Domain.AggregatesModel.ApplicationAggregate.Reporting
{
    public class ApplicationsByStatus
    {
        public ApplicationStatus Status { get; set; }
        public int Count { get; set; }
    }
}
