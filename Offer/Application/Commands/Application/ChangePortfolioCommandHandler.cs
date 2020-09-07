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
using Offer.Infrastructure;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Offer.API.Application.Commands.Application
{
    public class ChangePortfolioCommandHandler : IRequestHandler<ChangePortfolioCommand, CommandStatus>
    {
        private readonly IApplicationRepository _applicationRepository;
        IChangePortfolioRepository _changePortfolioRepository;
        private readonly IMediator _mediator;
        private readonly IEventBus _eventBus;
        private readonly ILogger<ChangePortfolioCommand> _logger;
        private readonly MessageEventFactory _messageEventFactory;
        private readonly IAuditClient _auditClient;

        public ChangePortfolioCommandHandler(
            IMediator mediator,
            IApplicationRepository applicationRepository,
            IChangePortfolioRepository changePortfolioRepository,
            IEventBus eventBus,
            ILogger<ChangePortfolioCommand> logger,
            MessageEventFactory messageEventFactory,
            IAuditClient auditClient)
        {
            this._applicationRepository = applicationRepository;
            this._changePortfolioRepository = changePortfolioRepository;
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _messageEventFactory = messageEventFactory;
            _auditClient = auditClient ?? throw new ArgumentNullException(nameof(auditClient));
        }

        public async Task<CommandStatus> Handle(ChangePortfolioCommand request, CancellationToken cancellationToken)
        {
            var application = await this._applicationRepository.GetAsync(request.ApplicationNumber);
            if(application == null)
            {
                return new CommandStatus { CommandResult = StandardCommandResult.NOT_FOUND };
            }
                PortfolioChangeRequests portfolioChangeRequest = new PortfolioChangeRequests
                {
                    ApplicationId = application.ApplicationId,
                    ChangeRequestTime = DateTime.UtcNow,
                    InitialValue =  application.OrganizationUnitCode,
                    RequestedValue = request.RequestedValue,
                    RequestDescription = request.RequestDescription,
                    Status = ChangeRequestsKindApp.Update
                };
                var result = await _changePortfolioRepository.PostPortfolioChangeRequests(portfolioChangeRequest, request.AuditLog);
                var msgBuilder = _messageEventFactory.CreateBuilder("offer", "application-portfolio-change-requested")
                                            .AddHeaderProperty("application-number", request.ApplicationNumber.GetApplicationNumber())
                                            .AddBodyProperty("change-request-id", result.PortfolioChangeRequestId);
                _logger.LogInformation("Sennding message {BrokerMessageName} event to broker on topic {BrokerTopicName} for application: {ApplicationNumber}", "application-portfolio-change-requested", "offer", request.ApplicationNumber);
                    
                _eventBus.Publish(msgBuilder.Build());
                return new CommandStatus { CommandResult = StandardCommandResult.OK };
        }
        
        public class ChangePortfilioIdentifiedCommandHandler : IdentifiedCommandHandler<ChangePortfolioCommand, CommandStatus>
        {
            public ChangePortfilioIdentifiedCommandHandler(IMediator mediator, IRequestManager requestManager) : base (mediator, requestManager)
            {
            }

            protected override CommandStatus CreateResultForDuplicateRequest()
            {
                return new CommandStatus { CommandResult = StandardCommandResult.OK };
            }
        }

    }
}
