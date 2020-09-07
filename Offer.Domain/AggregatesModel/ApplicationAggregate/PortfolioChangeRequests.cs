using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Offer.Domain.AggregatesModel.ApplicationAggregate
{
    public class PortfolioChangeRequests
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long PortfolioChangeRequestId { get; set; }
        public long ApplicationId { get; set; }
        [NotMapped]
        public string ApplicationNumber
        {
            get
            {
                var result = "0000000000" + ApplicationId;
                return result.Substring(result.Length - 10);
            }
        }
        public DateTime ChangeRequestTime { get; set; }
        public string InitialValue { get; set; }
        public string RequestedValue { get; set; }
        public string FinalValue { get; set; }
        public string RequestDescription { get; set; }
        [DefaultValue(ChangeRequestsKindApp.Unknown)]
        public ChangeRequestsKindApp Status { get; set; } = ChangeRequestsKindApp.Unknown;
    }
}
