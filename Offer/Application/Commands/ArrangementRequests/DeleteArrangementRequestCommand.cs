using MediatR;

namespace Offer.API.Application.Commands.ArrangementRequests
{
    public class DeleteArrangementRequestCommand : IRequest<bool?>
    {
        public long ApplicationNumber { get; set; }
        public int ArrangementRequestId { get; set; }
    }
}
