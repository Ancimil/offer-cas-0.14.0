using AuditClient;
using MediatR;
using MicroserviceCommon.Application.Commands;
using MicroserviceCommon.Infrastructure.Idempotency;
using MicroserviceCommon.Services;
using Microsoft.Extensions.Logging;
using Offer.Domain.AggregatesModel.ApplicationAggregate;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Offer.API.Application.Commands.ArrangementRequests.Collateral
{
    public class UpdateCollateralRequirementCommandHandler : IRequestHandler<UpdateCollateralRequirementCommand, bool?>
    {
        private readonly IArrangementRequestRepository _arrangementRequestRepository;
        private readonly IMediator _mediator;
        private readonly ILogger<UpdateCollateralRequirementCommand> _logger;
        private readonly IAuditClient _auditClient;

        public UpdateCollateralRequirementCommandHandler(IMediator mediator,
            IArrangementRequestRepository arrangementRequestRepository,
            IConfigurationService configurationService,
        ILogger<UpdateCollateralRequirementCommand> logger,
        IAuditClient auditClient)
        {
            this._arrangementRequestRepository = arrangementRequestRepository;
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _auditClient = auditClient ?? throw new ArgumentNullException(nameof(auditClient));
        }

        public async Task<bool?> Handle(UpdateCollateralRequirementCommand message, CancellationToken cancellationToken)
        {
            var longApplicationNumber = long.Parse(message.ApplicationNumber);
            var collateralRequirement =  AutoMapper.Mapper.Map<CollateralRequirement>(message);
            _arrangementRequestRepository.UpdateCollateralRequirement(collateralRequirement);
            return await _arrangementRequestRepository.UnitOfWork.SaveEntitiesAsync();
        }
    }

    public class UpdateCollateralRequirementCommandIdentifiedHandler : IdentifiedCommandHandler<UpdateCollateralRequirementCommand, bool?>
    {
        public UpdateCollateralRequirementCommandIdentifiedHandler(IMediator mediator, IRequestManager requestManager) : base(mediator, requestManager)
        {

        }

        protected override bool? CreateResultForDuplicateRequest()
        {
            return true;
        }
    }
}
