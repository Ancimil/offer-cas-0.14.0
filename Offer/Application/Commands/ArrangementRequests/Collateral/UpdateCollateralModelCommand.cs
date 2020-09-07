using MediatR;

namespace Offer.API.Application.Commands.ArrangementRequests.Collateral
{
    public class UpdateCollateralModelCommand : IRequest<bool?>
    {
        public int ArrangementRequestId { get; set; }
        public long ApplicationNumber { get; set; }
        public string CollateralModel { get; set; }
    }
}
