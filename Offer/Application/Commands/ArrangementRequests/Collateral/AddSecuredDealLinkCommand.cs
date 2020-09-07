using MediatR;

namespace Offer.API.Application.Commands.ArrangementRequests.Collateral
{
    public class AddSecuredDealLinkCommand : IRequest<bool>
    {
        public string ApplicationNumber { get; set; }
        public int ArrangementRequestId { get; set; }
        public int CollateralRequirementId { get; set; }
        public string ArrangementNumber { get; set; }
        public string ArrangementCurrency { get; set; }
        public decimal PledgedValueInCollateralCurrency { get; set; }
    }
}
