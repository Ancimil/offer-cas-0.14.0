using Asseco.EventBus.Abstractions;
using AuditClient;
using AuditClient.Model;
using MediatR;
using MicroserviceCommon.Application.Commands;
using MicroserviceCommon.Extensions.Broker;
using MicroserviceCommon.Infrastructure.Idempotency;
using Microsoft.Extensions.Logging;
using Offer.Domain.AggregatesModel.ApplicationAggregate;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Offer.API.Application.Commands.ArrangementRequests
{
    public class DeleteArrangementRequestCommandHandler : IRequestHandler<DeleteArrangementRequestCommand, bool?>
    {
        private readonly IArrangementRequestRepository _arrangementRequestRepository;
        private readonly IApplicationRepository _applicationRepository;
        private readonly MessageEventFactory _messageEventFactory;
        private readonly IEventBus _eventBus;
        private readonly IMediator _mediator;
        private readonly ILogger<DeleteArrangementRequestCommand> _logger;
        private readonly IAuditClient _auditClient;

        public DeleteArrangementRequestCommandHandler(IMediator mediator, IArrangementRequestRepository arrangementRequestRepository,
             ILogger<DeleteArrangementRequestCommand> logger, IAuditClient auditClient, IApplicationRepository applicationRepository, MessageEventFactory messageEventFactory,
             IEventBus eventBus)
        {
            _arrangementRequestRepository = arrangementRequestRepository;
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _auditClient = auditClient ?? throw new ArgumentNullException(nameof(auditClient));
            _applicationRepository = applicationRepository ?? throw new ArgumentNullException(nameof(applicationRepository));
            _messageEventFactory = messageEventFactory ?? throw new ArgumentNullException(nameof(messageEventFactory));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        }

        public async Task<bool?> Handle(DeleteArrangementRequestCommand message, CancellationToken cancellationToken)
        {
            ArrangementRequest arrangementRequest;
            arrangementRequest = _arrangementRequestRepository.GetArrangementRequest(message.ApplicationNumber,
            message.ArrangementRequestId, "product-snapshot", null);
            if (arrangementRequest == null)
            {
                return null;
            }
            var requestsToDelete = _arrangementRequestRepository.GetBundledRequests(arrangementRequest, false);
            requestsToDelete.Add(arrangementRequest);
            var result = await _arrangementRequestRepository.DeleteArrangementRequests(message.ApplicationNumber, requestsToDelete);
            var application = await _applicationRepository.GetAsync(message.ApplicationNumber);
            var messageObj = _messageEventFactory.CreateBuilder("offer", "product-selection-changed")
                                .AddHeaderProperty("application-number", application.ApplicationNumber);
            messageObj = messageObj.AddBodyProperty("product-code", application.ProductCode)
                                   .AddBodyProperty("product-name", application.ProductName);
            _eventBus.Publish(messageObj.Build());

            await _auditClient.WriteLogEntry(AuditLogEntryAction.Delete, AuditLogEntryStatus.Success, "arrangement", message.ApplicationNumber.ToString(), "Arrangement deleted " + arrangementRequest.ArrangementRequestId, new { });
            return result;
        }
    }

    public class DeleteArrangementRequestCommandIdentifiedHandler : IdentifiedCommandHandler<DeleteArrangementRequestCommand, bool?>
    {
        public DeleteArrangementRequestCommandIdentifiedHandler(IMediator mediator, IRequestManager requestManager) : base(mediator, requestManager)
        {

        }

        protected override bool? CreateResultForDuplicateRequest()
        {
            return true;
        }
    }
}
