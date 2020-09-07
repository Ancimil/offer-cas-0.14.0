using System;
using System.Threading.Tasks;
using MediatR;
using MicroserviceCommon.Application.Commands;
using MicroserviceCommon.Infrastructure.Idempotency;
using Microsoft.Extensions.Logging;
using Asseco.EventBus.Abstractions;
using Offer.Domain.AggregatesModel.ApplicationAggregate;
using MicroserviceCommon.ApiUtil;
using MicroserviceCommon.Extensions.Broker;
using System.Threading;

namespace Offer.API.Application.Commands
{
    public class InitiateGdprProcessCommandHandler : IRequestHandler<InitiateGdprProcessCommand, string>
    {
        private readonly IMediator _mediator;
        private readonly ApiEndPoints _apiEndPoints;
        private readonly ILogger<InitiateGdprProcessCommand> _logger;
        private readonly IEventBus _eventBus;
        private readonly MessageEventFactory _messageEventFactory;

        public InitiateGdprProcessCommandHandler(
            IMediator mediator,
            ApiEndPoints apiEndPoints,
            ILogger<InitiateGdprProcessCommand> logger,
            IEventBus eventBus,
            MessageEventFactory messageEventFactory)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _apiEndPoints = apiEndPoints ?? throw new ArgumentNullException(nameof(apiEndPoints));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            this._messageEventFactory = messageEventFactory;
        }

        public Task<string> Handle(InitiateGdprProcessCommand message, CancellationToken cancellationToken)
        {
            var caseId = Guid.NewGuid().ToString();
            var msgBuilder = _messageEventFactory.CreateBuilder("offer", GetMessageNameForProcessKey(message.ProcessKey))
                            .AddBodyProperty("username", message.Username)
                            .AddBodyProperty("customer-number", message.CustomerNumber)
                            .AddBodyProperty("initiator", message.Initiator)
                            .AddBodyProperty("email", message.Email)
                            .AddBodyProperty("user-message", message.UserMessage)
                            .AddBodyProperty("case-id", caseId)
                            .AddHeaderProperty("case-id", caseId);
            
            _eventBus.Publish(msgBuilder.Build());

            return null;
        }

        private string GetMessageNameForProcessKey(GdprProcessEnum processKey)
        {
            switch (processKey)
            {
                case GdprProcessEnum.Export:
                    return "gdpr-export-initiated";
                case GdprProcessEnum.Anonymize:
                    return "gdpr-anonymize-initiated";
                case GdprProcessEnum.Correction:
                    return "gdpr-correction-initiated";
            }
            return "gdpr-export-initiated";
        }
    }

    public class InitiateGdprProcessIdentifiedCommandHandler : IdentifiedCommandHandler<InitiateGdprProcessCommand, string>
    {
        public InitiateGdprProcessIdentifiedCommandHandler(IMediator mediator, IRequestManager requestManager) : base(mediator, requestManager)
        {
        }

        protected override string CreateResultForDuplicateRequest()
        {
            return "";
        }
    }
}
