using Asseco.EventBus.Abstractions;
using AuditClient;
using AuditClient.Model;
using MediatR;
using MicroserviceCommon.Application.Commands;
using MicroserviceCommon.Extensions.Broker;
using MicroserviceCommon.Infrastructure.Idempotency;
using MicroserviceCommon.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Offer.Domain.AggregatesModel.ApplicationAggregate;
using Offer.Domain.AggregatesModel.ApplicationAggregate.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Offer.API.Application.Commands.Application
{
    public class ChangePortfolioAcceptCommandHandler : IRequestHandler<ChangePortfolioAcceptCommand, CommandStatus>
    {
        private readonly IApplicationRepository _applicationRepository;
        IChangePortfolioRepository _changePortfolioRepository;
        private readonly IMediator _mediator;
        private readonly IEventBus _eventBus;
        private readonly ILogger<ChangePortfolioAcceptCommand> _logger;
        private readonly MessageEventFactory _messageEventFactory;
        private readonly IAuditClient _auditClient;

        public ChangePortfolioAcceptCommandHandler(
            IMediator mediator,
            IApplicationRepository applicationRepository,
            IChangePortfolioRepository changePortfolioRepository,
            IEventBus eventBus,
            ILogger<ChangePortfolioAcceptCommand> logger,
            MessageEventFactory messageEventFactory,
            IAuditClient auditClient)
        {
            this._applicationRepository = applicationRepository;
            this._changePortfolioRepository = changePortfolioRepository;
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _messageEventFactory = messageEventFactory;
            _auditClient = auditClient ?? throw new ArgumentNullException(nameof(auditClient)); ;
        }

        public async Task<CommandStatus> Handle(ChangePortfolioAcceptCommand request, CancellationToken cancellationToken)
        {
            var application = await this._applicationRepository.GetAsync(request.ApplicationNumber);
            if (application == null)
            {
                return new CommandStatus { CommandResult = StandardCommandResult.NOT_FOUND };
            }

            PortfolioChangeRequests postPortfolioChangeRequest = new PortfolioChangeRequests
            {
                PortfolioChangeRequestId = request.PortfolioChangeRequestId,
                ApplicationId = application.ApplicationId,
                FinalValue = request.FinalValue
            };
            var result = await _changePortfolioRepository.UpdatePortfolioChangeRequests(postPortfolioChangeRequest, request.AuditLog);

            return new CommandStatus { CommandResult = StandardCommandResult.OK };
        }

        public class ChangePortfilioAcceptIdentifiedCommandHandler : IdentifiedCommandHandler<ChangePortfolioAcceptCommand, CommandStatus>
        {
            public ChangePortfilioAcceptIdentifiedCommandHandler(IMediator mediator, IRequestManager requestManager) : base(mediator, requestManager)
            {
            }

            protected override CommandStatus CreateResultForDuplicateRequest()
            {
                return new CommandStatus { CommandResult = StandardCommandResult.OK };
            }
        }
        
    }
}
