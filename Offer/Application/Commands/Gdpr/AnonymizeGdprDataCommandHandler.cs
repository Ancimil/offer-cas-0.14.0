using System;
using System.Threading.Tasks;
using MediatR;
using MicroserviceCommon.Application.Commands;
using MicroserviceCommon.Infrastructure.Idempotency;
using Microsoft.Extensions.Logging;
using Asseco.EventBus.Abstractions;
using Offer.Domain.AggregatesModel.ApplicationAggregate;
using MicroserviceCommon.ApiUtil;
using System.Threading;
using AuditClient;
using AuditClient.Model;

namespace Offer.API.Application.Commands
{
    public class AnonymizeGdprDataCommandHandler : IRequestHandler<AnonymizeGdprDataCommand, string>
    {
        private readonly IMediator _mediator;
        private readonly ApiEndPoints _apiEndPoints;
        private readonly ILogger<AnonymizeGdprDataCommand> _logger;
        private readonly IApplicationRepository _applicationRepository;
        private readonly IAuditClient _auditClient;
        private readonly IEventBus _eventBus;

        public AnonymizeGdprDataCommandHandler(IMediator mediator, ApiEndPoints apiEndPoints,
            ILogger<AnonymizeGdprDataCommand> logger, IEventBus eventBus, IApplicationRepository applicationRepository, IAuditClient auditClient)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _apiEndPoints = apiEndPoints ?? throw new ArgumentNullException(nameof(apiEndPoints));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            this._applicationRepository = applicationRepository ?? throw new ArgumentNullException(nameof(applicationRepository));
            _auditClient = auditClient ?? throw new ArgumentNullException(nameof(auditClient));
        }

        public async Task<string> Handle(AnonymizeGdprDataCommand message, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(message.Username) && string.IsNullOrEmpty(message.CustomerNumber))
            {
                throw new ArgumentException("Neither username nor customer number are not specified or they are an empty strings");
            }

            var matcher = new PartyMatcher(message.Username, message.CustomerNumber);
            var data = _applicationRepository.AnonymizeGdprData(matcher, message.Fake);
            if (!message.Fake)
            {
                await _applicationRepository.UnitOfWork.SaveEntitiesAsync();
                try
                {
                    await _auditClient.WriteLogEntry(AuditLogEntryAction.Execute, AuditLogEntryStatus.Success, "gdpr", message.CustomerNumber, "Anonymize gdpr data", data);

                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Audit error in AnonymizeGdprDataCommandHandler");
                }
            }
            return data;
        }
    }

    public class AnonymizeGdprDataIdentifiedCommandHandler : IdentifiedCommandHandler<AnonymizeGdprDataCommand, string>
    {
        public AnonymizeGdprDataIdentifiedCommandHandler(IMediator mediator, IRequestManager requestManager) : base(mediator, requestManager)
        {
        }

        protected override string CreateResultForDuplicateRequest()
        {
            return null;
        }
    }
}
