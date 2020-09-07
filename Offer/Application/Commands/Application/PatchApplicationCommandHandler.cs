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
using Offer.Infrastructure;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Offer.API.Application.Commands.Application
{
    public class PatchApplicationCommandHandler : IRequestHandler<PatchApplicationCommand, CommandStatus>
    {
        private readonly IApplicationRepository _applicationRepository;
        private readonly IEventBus _eventBus;
        private readonly ILogger<PatchApplicationCommand> _logger;
        private readonly MessageEventFactory _messageEventFactory;
        private readonly IAuditClient _auditClient;

        public PatchApplicationCommandHandler(
            IApplicationRepository applicationRepository,
            IEventBus eventBus,
            ILogger<PatchApplicationCommand> logger,
            MessageEventFactory messageEventFactory,
            IAuditClient auditClient)
        {
            this._applicationRepository = applicationRepository;
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _messageEventFactory = messageEventFactory;
            _auditClient = auditClient ?? throw new ArgumentNullException(nameof(auditClient));
        }

        public async Task<CommandStatus> Handle(PatchApplicationCommand request, CancellationToken cancellationToken)
        {
            var application = await this._applicationRepository.GetAsync(request.ApplicationNumber);
            if(application == null)
            {
                return new CommandStatus { CommandResult = StandardCommandResult.NOT_FOUND };
            }
            application.OrganizationUnitCode = request.OrganizationUnitCode;
            _applicationRepository.Update(application);
            await _applicationRepository.UnitOfWork.SaveChangesAsync();
            var msgBuilder = _messageEventFactory.CreateBuilder("offer", "offer-patched")
                            .AddHeaderProperty("application-number", request.ApplicationNumber.GetApplicationNumber());
            _logger.LogInformation("Sending message {BrokerMessageName} event to broker on topic {BrokerTopicName} for application: {ApplicationNumber}", "offer-patched", "offer", request.ApplicationNumber);
            _eventBus.Publish(msgBuilder.Build());

            if (request.AuditLog)
            {
                await _auditClient.WriteLogEntry(AuditLogEntryAction.Patch, AuditLogEntryStatus.Success, "organization-unit-code", request.ApplicationNumber.ToString(), "Organization unit code updated", application.OrganizationUnitCode);
            }

            return new CommandStatus { CommandResult = StandardCommandResult.OK };

        }

        public class PatchApplicationIdentifiedCommandHandler : IdentifiedCommandHandler<PatchApplicationCommand, CommandStatus>
        {
            public PatchApplicationIdentifiedCommandHandler(IMediator mediator, IRequestManager requestManager) : base(mediator, requestManager)
            {
            }

            protected override CommandStatus CreateResultForDuplicateRequest()
            {
                return new CommandStatus { CommandResult = StandardCommandResult.OK };
            }
        }
    }
}
