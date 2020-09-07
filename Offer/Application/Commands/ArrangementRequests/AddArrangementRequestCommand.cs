using MediatR;
using Offer.Domain.AggregatesModel.ApplicationAggregate;

namespace Offer.API.Application.Commands.ArrangementRequests
{
    public class AddArrangementRequestCommand : IRequest<bool?>
    {
        public long ApplicationNumber { get; set; }
        public ArrangementRequest ArrangementRequest { get; set; }
    }
}
