using MediatR;
using MicroserviceCommon.Models;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace Offer.API.Application.Commands
{
    public class CancelApplicationCommand : IRequest<CommandStatus>
    {
        public long ApplicationNumber { get; set; }
        public string Kind { get; set; }
        [Required]
        public string CancelationReason { get; set; }
        public string CancelationComment { get; set; }

        public CancelApplicationCommand(long applicationNumber, string kind, string cancelationReason, string cancelationComment)
        {
            ApplicationNumber = applicationNumber;
            Kind = kind;
            CancelationReason = cancelationReason;
            CancelationComment = cancelationComment;
        }
    }
}
