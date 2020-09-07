using MediatR;
using Offer.Domain.AggregatesModel.ApplicationAggregate;

namespace Offer.API.Application.Commands
{
    public class GetProductConditionsCommand : IRequest<bool>
    {
        public ArrangementRequest ArrangementRequest { get; set; }

        public GetProductConditionsCommand(ArrangementRequest arrangementRequest)
        {
            ArrangementRequest = arrangementRequest;
        }
    }
}
