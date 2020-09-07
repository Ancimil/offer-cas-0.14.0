using System;
using System.ComponentModel.DataAnnotations;

namespace Offer.Domain.AggregatesModel.ApplicationAggregate
{
    public class Events
    {
        [Key]
        [Required]
        public int ArrangementRequestId { get; set; }

        public string ApplicationNumber { get; set; }

        public string Type { get; set; }

        public string Values { get; set; }

        public DateTime? EventDate { get; set; }

    }
}
