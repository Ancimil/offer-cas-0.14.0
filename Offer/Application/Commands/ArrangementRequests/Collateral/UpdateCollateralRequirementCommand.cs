using MediatR;
using Offer.Domain.AggregatesModel.ApplicationAggregate;
using System.Collections.Generic;

namespace Offer.API.Application.Commands.ArrangementRequests.Collateral
{
    public class UpdateCollateralRequirementCommand : IRequest<bool?>
    {
        public long CollateralRequirementId { get; set; }
        public long ApplicationId { get; set; }
        public string ApplicationNumber
        {
            get
            {
                var result = "0000000000" + ApplicationId;
                return result.Substring(result.Length - 10);
            }
            set
            {
                ApplicationId = long.Parse(value);
            }
        }

        public int ArrangementRequestId { get; set; }
        public string CollateralArrangementCode { get; set; }
        public bool FromModel { get; set; }
        public decimal MinimalCoverage { get; set; }
        public decimal MinimalCoverageInLoanCurrency { get; set; }
        public decimal ActualCoverage { get; set; }
        public List<SecuredDealLink> SecuredDealLinks { get; set; }
    }
}
