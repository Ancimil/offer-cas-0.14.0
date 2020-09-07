using Asseco.EventBus.Abstractions;
using Asseco.EventBus.Events;
using AuditClient;
using AuditClient.Model;
using MediatR;
using MicroserviceCommon.API.ApiUtils;
using MicroserviceCommon.Application.Commands;
using MicroserviceCommon.Extensions.Broker;
using MicroserviceCommon.Infrastructure.Idempotency;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Offer.Domain.AggregatesModel.ApplicationAggregate;
using Offer.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Offer.API.Application.Commands
{
    public class UpdateApplicationStatusCommandHandler : IRequestHandler<UpdateApplicationStatusCommand, bool>
    {
        private readonly IApplicationRepository _applicationRepository;
        private readonly IEventBus _eventBus;
        private readonly ILogger<UpdateApplicationStatusCommand> _logger;
        private readonly MessageEventFactory _messageEventFactory;
        private readonly IAuditClient _auditClient;

        public UpdateApplicationStatusCommandHandler(
            IApplicationRepository applicationRepository,
            IEventBus eventBus,
            ILogger<UpdateApplicationStatusCommand> logger,
            MessageEventFactory messageEventFactory,
            IAuditClient auditClient
            )
        {
            this._applicationRepository = applicationRepository;
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _messageEventFactory = messageEventFactory;
            _auditClient = auditClient ?? throw new ArgumentNullException(nameof(auditClient)); ;
        }

        public async Task<bool> Handle(UpdateApplicationStatusCommand message, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Updating application status to {applicationStatus} for application {applicationNumber}", message.Status, message.ApplicationId.GetApplicationNumber());
            var application = _applicationRepository.UpdateStatus(message.ApplicationId, message.Status, message.StatusInformation, message.Phase);
            var messageBuilder = _messageEventFactory.CreateBuilder("offer", "status-updated")
                                 .AddHeaderProperty("application-number", message.ApplicationId.GetApplicationNumber())
                                 .AddHeaderProperty("username", application.Initiator.Contains("robot") ? "ALL" : application.Initiator);
            var commercialDetails = _applicationRepository.GetCommercialDetails(message.ApplicationId);
            if(message.Status != null && application.LeadId != null)
            {
                var stat = Enum.GetName(typeof(ApplicationStatus), application.Status);
                messageBuilder.AddBodyProperty("status", stat)
                              .AddBodyProperty("lead-id", application.LeadId);
            }

            if (commercialDetails != null)
            {
                foreach (var key in commercialDetails.Keys)
                {
                    messageBuilder.AddBodyProperty(key, commercialDetails[key]);
                }
            }
            _logger.LogInformation("Sending offer status updated event to broker on topic {topicName} for application: {applicationNumber}", "offer", message.ApplicationId.GetApplicationNumber());
            var result = await _applicationRepository.UnitOfWork.SaveEntitiesAsync();
            try
            {
                await _auditClient.WriteLogEntry(AuditLogEntryAction.Update, AuditLogEntryStatus.Success, "application", application.ApplicationNumber, "Application status has been updated", new { });

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Audit error in UpdateApplicationStatusCommandHandler");
            }

            _eventBus.Publish(messageBuilder.Build());
            return result;
        }
    }
    public class UpdateApplicationStatusIdentifiedCommandHandler : IdentifiedCommandHandler<UpdateApplicationStatusCommand, bool>
    {
        public UpdateApplicationStatusIdentifiedCommandHandler(IMediator mediator, IRequestManager requestManager) : base(mediator, requestManager)
        {
        }

        protected override bool CreateResultForDuplicateRequest()
        {
            return true;
        }
    }
}
