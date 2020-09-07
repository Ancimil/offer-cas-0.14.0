using Asseco.EventBus.Abstractions;
using MediatR;
using MicroserviceCommon.Application.Commands;
using MicroserviceCommon.Extensions.Broker;
using MicroserviceCommon.Infrastructure.Idempotency;
using MicroserviceCommon.Services;
using Microsoft.Extensions.Logging;
using Offer.Domain.AggregatesModel.ApplicationAggregate;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Offer.API.Application.Commands.ArrangementRequests.Collateral
{
    public class DeleteSecuredDealLinkCommandCommandHandler : IRequestHandler<DeleteSecuredDealLinkCommand, bool>
    {
        private readonly IArrangementRequestRepository _arrangementRequestRepository;
        private readonly IEventBus _bus;
        private readonly IMediator _mediator;
        private readonly IConfigurationService _configurationService;
        private readonly ILogger<DeleteSecuredDealLinkCommand> _logger;
        private readonly MessageEventFactory _messageEventFactory;

        public DeleteSecuredDealLinkCommandCommandHandler(
            IMediator mediator,
            IArrangementRequestRepository arrangementRequestRepository,
            IEventBus bus,
            IConfigurationService configurationService,
            MessageEventFactory messageEventFactory,
            ILogger<DeleteSecuredDealLinkCommand> logger
           )
        {
            this._arrangementRequestRepository = arrangementRequestRepository;
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _configurationService = configurationService ?? throw new ArgumentNullException(nameof(configurationService));
            _messageEventFactory = messageEventFactory;
        }

        public async Task<bool> Handle(DeleteSecuredDealLinkCommand message, CancellationToken cancellationToken)
        {
            var longApplicationNumber = long.Parse(message.ApplicationNumber);
            CollateralRequirement requirement = _arrangementRequestRepository.GetCollateralRequirementById(longApplicationNumber, message.ArrangementRequestId, message.CollateralRequirementId);
            var result = false;
            try
            {
                var remainingDeals = requirement.SecuredDealLinks.Where
                    (x => !x.ArrangementRequestId.Equals(message.ArrangementRequestId) ||
                    !x.ApplicationNumber.Equals(message.ApplicationNumber) ||
                    !x.ArrangementNumber.Equals(message.ArrangementNumber)).ToList();
                _logger.LogDebug("Number of remaining secure deal links is {SecureDealLinkCount}", remainingDeals.Count());
                requirement.SecuredDealLinks = remainingDeals;
                _arrangementRequestRepository.UpdateCollateralRequirement(requirement);
                result = await _arrangementRequestRepository.UnitOfWork.SaveEntitiesAsync();

                var msgBuilder =
                    _messageEventFactory.CreateBuilder("offer", "secured-deal-link-changed")
                          .AddHeaderProperty("application-number", message.ApplicationNumber)
                          .AddHeaderProperty("username", "ALL");

                _bus.Publish(msgBuilder.Build());
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An error occurred while removing deal link from collateral requirement.");
            }
            return result;
        }
    }

    public class DeleteSecuredDealLinkCommandIdentifiedHandler : IdentifiedCommandHandler<DeleteSecuredDealLinkCommand, bool>
    {
        public DeleteSecuredDealLinkCommandIdentifiedHandler(IMediator mediator, IRequestManager requestManager) : base(mediator, requestManager)
        {

        }

        protected override bool CreateResultForDuplicateRequest()
        {
            return true;
        }
    }
}
