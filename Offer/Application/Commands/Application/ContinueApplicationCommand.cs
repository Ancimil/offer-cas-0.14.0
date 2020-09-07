using MediatR;
using MicroserviceCommon.Models;
using Offer.Domain.AggregatesModel.ApplicationAggregate;
using System.Collections.Generic;

namespace Offer.API.Application.Commands.Application
{
    public class ContinueApplicationCommand : IRequest<CommandStatus>
    {
        public long ApplicationNumber { get; set; }
        public string Username { get; set; }
        public List<ArrangementRequest> ArrangementRequests { get; set; }
        public bool AuditLog { get; set; }
    }
}
