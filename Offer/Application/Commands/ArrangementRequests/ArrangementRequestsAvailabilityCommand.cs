using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using MediatR;
using Offer.Domain.AggregatesModel.ApplicationAggregate.ArrangementRequestModel;

namespace Offer.API.Application.Commands
{
    public class ArrangementRequestsAvailabilityCommand : IRequest<bool?>
        {
            public long ApplicationId { get; set; }
            public string ApplicationNumber
            {
                get
                {
                    var result = "0000000000" + ApplicationId;
                    return result.Substring(result.Length - 10);
                }
            }
            [Required]
            public List<ArrangementRequestsAvailability> Availabilities { get; set; }
        }
}
