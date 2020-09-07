using MediatR;

namespace Offer.API.Application.Commands.ArrangementRequests.Collateral
{
    public class AddCollateralRequirementCommand : IRequest<bool?>
    {
        public long ApplicationNumber { get; set; }
        public int ArrangementRequestId { get; set; }
        public string CollateralArrangementCode { get; set; }
        public decimal MinimalCoverage { get; set; }
        public string CollateralOwner { get; set; }

        public AddCollateralRequirementCommand(long applicationNumber, int arrangementRequestId, string collateralArrangementCode, decimal minimalCoverage, string collateralOwner)
        {
            ApplicationNumber = applicationNumber;
            ArrangementRequestId = arrangementRequestId;
            CollateralArrangementCode = collateralArrangementCode;
            MinimalCoverage = minimalCoverage;
            CollateralOwner = collateralOwner;
        }
    }
}
