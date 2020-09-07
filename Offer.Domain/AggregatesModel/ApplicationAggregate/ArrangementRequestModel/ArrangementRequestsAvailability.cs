using System.ComponentModel.DataAnnotations;

namespace Offer.Domain.AggregatesModel.ApplicationAggregate.ArrangementRequestModel
{
    public class ArrangementRequestsAvailability
    {
        [Required]
        public long ArrangementRequestId { get; set; }
        public bool Enabled { get; set; } = false;
    }
}
