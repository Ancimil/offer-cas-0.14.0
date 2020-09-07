using Asseco.EventBus.Abstractions;
using MediatR;
using MicroserviceCommon.Application.Commands;
using MicroserviceCommon.Extensions.Broker;
using MicroserviceCommon.Infrastructure.Idempotency;
using Microsoft.Extensions.Logging;
using Offer.Domain.AggregatesModel.ApplicationAggregate;
using System;
using System.Threading.Tasks;
using MicroserviceCommon.Models;
using Offer.Infrastructure;
using System.Threading;
using AuditClient;
using AuditClient.Model;

namespace Offer.API.Application.Commands
{
    public class CancelApplicationCommandHandler : IRequestHandler<CancelApplicationCommand, CommandStatus>
    {
        private readonly IApplicationRepository _applicationRepository;
        private readonly IMediator _mediator;
        private readonly IEventBus _eventBus;
        private readonly ILogger<UpdateApplicationStatusCommand> _logger;
        private readonly MessageEventFactory _messageEventFactory;
        private readonly IAuditClient _auditClient;

        public CancelApplicationCommandHandler(
            IMediator mediator,
            IApplicationRepository applicationRepository,
            IEventBus eventBus,
            ILogger<UpdateApplicationStatusCommand> logger,
            MessageEventFactory messageEventFactory,
            IAuditClient auditClient
            )
        {
            this._applicationRepository = applicationRepository;
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _messageEventFactory = messageEventFactory;
            _auditClient = auditClient;
        }

        public async Task<CommandStatus> Handle(CancelApplicationCommand message, CancellationToken cancellationToken)
        {
            var application = await this._applicationRepository.GetAsync(message.ApplicationNumber);
            if (application == null)
            {
                return new CommandStatus { CommandResult = StandardCommandResult.NOT_FOUND };
            }
            var statusInfo = new StatusInformation
            {
                Title = "Canceled",
                Description = "Your request is cancelled.",
                Html = ""
            };
            application.Status = ApplicationStatus.Canceled;
            application.StatusInformation = statusInfo;
            application.CancelationReason = message.CancelationReason;
            application.CancelationComment = message.CancelationComment;
            _applicationRepository.Update(application);
            await _applicationRepository.UnitOfWork.SaveEntitiesAsync();
            try
            {
                await _auditClient.WriteLogEntry(AuditLogEntryAction.Cancel, AuditLogEntryStatus.Success, "application", application.ApplicationNumber, "Canceled application", new { });

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Audit error in CancelApplicationCommandHandler");
            }
            
            var msgBuilder = _messageEventFactory.CreateBuilder("offer", "offer-canceled")
                            .AddHeaderProperty("application-number", message.ApplicationNumber.GetApplicationNumber())
                            .AddBodyProperty("cancelation-reason", application.CancelationReason);
            var commercialDetails = _applicationRepository.GetCommercialDetails(message.ApplicationNumber);
            if (application.LeadId != null)
            {
                var stat = Enum.GetName(typeof(ApplicationStatus), application.Status);
                msgBuilder.AddBodyProperty("status", stat)
                          .AddBodyProperty("lead-id", application.LeadId);
            }
            if (commercialDetails != null)
            {
                foreach (var key in commercialDetails.Keys)
                {
                    msgBuilder.AddBodyProperty(key, commercialDetails[key]);
                }
            }
            _logger.LogInformation("Sending message {BrokerMessageName} event to broker on topic {BrokerTopicName} for application: {ApplicationNumber}", "offer-canceled", "offer", message.ApplicationNumber);
            _eventBus.Publish(msgBuilder.Build());
            return new CommandStatus { CommandResult = StandardCommandResult.OK };
        }
    }
    public class CancelApplicationIdentifiedCommandHandler : IdentifiedCommandHandler<CancelApplicationCommand, CommandStatus>
    {
        public CancelApplicationIdentifiedCommandHandler(IMediator mediator, IRequestManager requestManager) : base(mediator, requestManager)
        {
        }

        protected override CommandStatus CreateResultForDuplicateRequest()
        {
            return new CommandStatus { CommandResult = StandardCommandResult.OK };
        }
    }
}