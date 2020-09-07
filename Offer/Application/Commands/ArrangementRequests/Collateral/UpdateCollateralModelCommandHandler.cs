using AuditClient;
using AuditClient.Model;
using MediatR;
using MicroserviceCommon.Application.Commands;
using MicroserviceCommon.Infrastructure.Idempotency;
using Offer.Domain.AggregatesModel.ApplicationAggregate;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Offer.API.Application.Commands.ArrangementRequests.Collateral
{
    public class UpdateCollateralModelCommandHandler : IRequestHandler<UpdateCollateralModelCommand, bool?>
    {
        private readonly IArrangementRequestRepository _arrangementRequestRepository;
        private readonly IAuditClient _auditClient;

        public UpdateCollateralModelCommandHandler(
            IArrangementRequestRepository arrangementRequestRepository,
            IAuditClient auditClient)
        {
            this._arrangementRequestRepository = arrangementRequestRepository;
            _auditClient = auditClient;
        }

        public async Task<bool?> Handle(UpdateCollateralModelCommand message, CancellationToken cancellationToken)
        {
            var arrangementRequest = _arrangementRequestRepository.GetArrangementRequest(message.ApplicationNumber, message.ArrangementRequestId, "collateral-requirements,product-snapshot", null);
            if (arrangementRequest == null)
            {
                return null;
            }
            if (arrangementRequest is FinanceServiceArrangementRequest ara && ara.ProductSnapshot.AvailableCollateralModels.Contains(message.CollateralModel))
            {
                var selectedModel = ara.ProductSnapshot.AvailableCollateralModelsData.Where(m => m.Code.Equals(message.CollateralModel)).FirstOrDefault();
                if (selectedModel != null)
                {
                    ara.CollateralModel = message.CollateralModel;
                    if (ara.CollateralRequirements == null)
                    {
                        ara.CollateralRequirements = new List<CollateralRequirement>();
                    }
                    foreach (var newRequirement in selectedModel.CollateralRequirements)
                    {
                        var existingRequirement = ara.CollateralRequirements.Where(c => c.CollateralArrangementCode.Equals(newRequirement.CollateralArrangementCode)).Any();
                        if (!existingRequirement)
                        {
                            var req = new CollateralRequirement
                            {
                                ApplicationId = message.ApplicationNumber,
                                ArrangementRequestId = message.ArrangementRequestId,
                                CollateralArrangementCode = newRequirement.CollateralArrangementCode,
                                MinimalCoverage = newRequirement.MinimalCoverage,
                                MinimalCoverageInLoanCurrency = ara.Amount * (newRequirement.MinimalCoverage / 100),
                                ActualCoverage = 0,
                                CollateralRequirementId = (ara.CollateralRequirements.Count > 0) ? ara.CollateralRequirements.Max(x => x.CollateralRequirementId) + 1 : 1,
                                FromModel = true
                            };
                            ara.CollateralRequirements.Add(req);
                        }
                    }
                    List<CollateralRequirement> deletionList = new List<CollateralRequirement>();
                    foreach (var requirement in ara.CollateralRequirements)
                    {
                        var query = selectedModel.CollateralRequirements.Where(x => x.CollateralArrangementCode.Equals(requirement.CollateralArrangementCode));
                        requirement.FromModel = query.Any();
                        if (!requirement.FromModel && requirement.SecuredDealLinks == null) // not from model and empty
                        {
                            deletionList.Add(requirement);
                        }
                    }
                    foreach (var requirement in deletionList)
                    {
                        ara.CollateralRequirements.Remove(requirement);
                    }
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }

            await _arrangementRequestRepository.UpdateArrangementRequest(arrangementRequest);
            return await _arrangementRequestRepository.UnitOfWork.SaveEntitiesAsync();
        }
    }

    public class UpdateCollateralModelCommandIdentifiedHandler : IdentifiedCommandHandler<UpdateCollateralModelCommand, bool?>
    {
        public UpdateCollateralModelCommandIdentifiedHandler(IMediator mediator, IRequestManager requestManager) : base(mediator, requestManager)
        {

        }

        protected override bool? CreateResultForDuplicateRequest()
        {
            return true;
        }
    }
}
