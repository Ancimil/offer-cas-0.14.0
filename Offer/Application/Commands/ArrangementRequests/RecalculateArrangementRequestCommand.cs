using MediatR;

namespace Offer.API.Application.Commands.ArrangementRequests
{
    public class RecalculateArrangementRequestCommand : IRequest<bool?>
    {
        public int ArrangementRequestId { get; set; }
        public long ApplicationNumber { get; set; }
    }
}
