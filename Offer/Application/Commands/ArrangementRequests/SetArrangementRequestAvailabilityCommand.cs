using System.ComponentModel.DataAnnotations;
using MediatR;

namespace Offer.API.Application.Commands
{
    public class SetArrangementRequestAvailabilityCommand : IRequest<bool?>
        {
            public int ArrangementRequestId { get; set; }
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
            public bool Enabled { get; set; }
        }
}
