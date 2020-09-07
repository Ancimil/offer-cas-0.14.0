using MediatR;

namespace Offer.API.Application.Commands.ArrangementRequests.Collateral
{
    public class DeleteSecuredDealLinkCommand : IRequest<bool>
    {
        public string ApplicationNumber { get; set; }
        public int ArrangementRequestId { get; set; }
        public int CollateralRequirementId { get; set; }
        public string ArrangementNumber { get; set; }
    }
}
