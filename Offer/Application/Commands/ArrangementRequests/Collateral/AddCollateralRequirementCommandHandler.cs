using AuditClient;
using AuditClient.Model;
using MediatR;
using MicroserviceCommon.Application.Commands;
using MicroserviceCommon.Infrastructure.Idempotency;
using Microsoft.Extensions.Logging;
using Offer.Domain.AggregatesModel.ApplicationAggregate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Offer.API.Application.Commands.ArrangementRequests.Collateral
{
    public class AddCollateralRequirementCommandHandler : IRequestHandler<AddCollateralRequirementCommand, bool?>
    {
        private readonly IArrangementRequestRepository _arrangementRequestRepository;
        private readonly IApplicationRepository _applicationRepository;
        private readonly IAuditClient _auditClient;
        private readonly ILogger<AddCollateralRequirementCommandHandler> _logger;

        public AddCollateralRequirementCommandHandler(
            IArrangementRequestRepository arrangementRequestRepository,
            IApplicationRepository applicationRepository,
            IAuditClient auditClient, ILogger<AddCollateralRequirementCommandHandler> logger)
        {
            this._arrangementRequestRepository = arrangementRequestRepository;
            this._applicationRepository = applicationRepository;
            _auditClient = auditClient;
            _logger = logger;
        }

        public async Task<bool?> Handle(AddCollateralRequirementCommand message, CancellationToken cancellationToken)
        {
            var arrangementRequest = _arrangementRequestRepository.GetArrangementRequest(message.ApplicationNumber, message.ArrangementRequestId, "collateral-requirements,product-snapshot", null);
            if (arrangementRequest == null)
            {
                return null;
            }

            if (arrangementRequest is FinanceServiceArrangementRequest finRequest)
            {
                var fromModel = false;
                var minimalCoverage = message.MinimalCoverage;
                var model = finRequest.ProductSnapshot.AvailableCollateralModelsData.Where(x => x.Code.Equals(finRequest.CollateralModel)).FirstOrDefault();
                if (model != null)
                {
                    var query = model.CollateralRequirements.Where(x => x.CollateralArrangementCode.Equals(message.CollateralArrangementCode));
                    fromModel = query.Any();
                    if (fromModel)
                    {
                        var collateralRequirement = query.FirstOrDefault();
                        //minimalCoverage = collateralRequirement.MinimalCoverage;
                    }
                }
                var minimalCoverageInLoanCurrency = finRequest.Amount * (minimalCoverage / 100);
                var collateralOwner = message.CollateralOwner;
                if (string.IsNullOrEmpty(collateralOwner))
                {
                    var involvedParties = await _applicationRepository.GetInvolvedParties(message.ApplicationNumber);
                    collateralOwner = involvedParties.Where(x => x.PartyRole.Equals(PartyRole.Customer)).FirstOrDefault().PartyId.ToString();
                }
                var requirement = new CollateralRequirement
                {
                    ApplicationId = message.ApplicationNumber,
                    ArrangementRequestId = message.ArrangementRequestId,
                    CollateralArrangementCode = message.CollateralArrangementCode,
                    MinimalCoverage = minimalCoverage,
                    MinimalCoverageInLoanCurrency = minimalCoverageInLoanCurrency,
                    ActualCoverage = 0,
                    FromModel = fromModel,
                    CollateralOwner = collateralOwner
                };

                if (finRequest.CollateralRequirements == null)
                {
                    finRequest.CollateralRequirements = new List<CollateralRequirement>();
                }
                requirement.CollateralRequirementId = (finRequest.CollateralRequirements.Count > 0) ? finRequest.CollateralRequirements.Max(x => x.CollateralRequirementId) + 1 : 1;
                _arrangementRequestRepository.AddCollateralRequirement(requirement);

                try
                {
                    await _auditClient.WriteLogEntry(AuditLogEntryAction.Add, AuditLogEntryStatus.Success, "collateral", message.ApplicationNumber.ToString(), "Collateral added", requirement);

                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Audit error in AddCollateralRequirementCommandHandler");
                }


            }
            else
            {
                return false;
            }

            


            return await _arrangementRequestRepository.UnitOfWork.SaveEntitiesAsync();
        }
    }

    public class AddCollateralRequirementCommandIdentifiedHandler : IdentifiedCommandHandler<AddCollateralRequirementCommand, bool?>
    {
        public AddCollateralRequirementCommandIdentifiedHandler(IMediator mediator, IRequestManager requestManager) : base(mediator, requestManager)
        {

        }

    }
}
