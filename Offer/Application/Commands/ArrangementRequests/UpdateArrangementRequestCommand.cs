using MediatR;
using MicroserviceCommon.Models;
using Offer.Domain.AggregatesModel.ApplicationAggregate;

namespace Offer.API.Application.Commands
{
    public class UpdateArrangementRequestCommand : IRequest<CommandStatus<bool>>
    {
        public long ApplicationNumber { get; set; }
        public int ArrangementRequestId { get; set; }
        public ArrangementRequest ArrangementRequest { get; set; }
    }
}
