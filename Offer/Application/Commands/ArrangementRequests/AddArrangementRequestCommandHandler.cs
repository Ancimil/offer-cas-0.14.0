using Asseco.EventBus.Abstractions;
using MediatR;
using MicroserviceCommon.Application.Commands;
using MicroserviceCommon.Extensions.Broker;
using MicroserviceCommon.Infrastructure.Idempotency;
using Offer.Domain.AggregatesModel.ApplicationAggregate;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Offer.API.Application.Commands.ArrangementRequests
{
    public class AddArrangementRequestCommandHandler : IRequestHandler<AddArrangementRequestCommand, bool?>
    {
        private readonly IApplicationRepository _applicationRepository;
        private readonly IArrangementRequestRepository _requestRepository;
        private readonly MessageEventFactory _messageEventFactory;
        private readonly IEventBus _eventBus;

        public AddArrangementRequestCommandHandler(
            IApplicationRepository applicationRepository,
            IArrangementRequestRepository requestRepository,
            MessageEventFactory messageEventFactory,
            IEventBus eventBus)
        {
            _applicationRepository = applicationRepository ?? throw new ArgumentNullException(nameof(applicationRepository));
            _requestRepository = requestRepository ?? throw new ArgumentNullException(nameof(requestRepository));
            _messageEventFactory = messageEventFactory ?? throw new ArgumentNullException(nameof(messageEventFactory));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        }

        public async Task<bool?> Handle(AddArrangementRequestCommand message, CancellationToken cancellationToken)
        {
            var request = message.ArrangementRequest;
            if (request == null)
            {
                return null;
            }

            var application = await _applicationRepository.GetAsync(message.ApplicationNumber,
                "involved-parties,documents,arrangement-requests");

            if (application == null)
            {
                return null;
            }

            if (string.IsNullOrEmpty(application.ProductCode))
            {
                application.ProductCode = request.ProductCode;
                application.ProductName = request.ProductName;
            }

            await _requestRepository.AddArrangementRequestsToApplication(application, new List<ArrangementRequest>() { request });
            var messageObj = _messageEventFactory.CreateBuilder("offer", "product-selection-changed")
                                .AddHeaderProperty("application-number", application.ApplicationNumber);
            messageObj = messageObj.AddBodyProperty("product-code", application.ProductCode)
                                   .AddBodyProperty("product-name", application.ProductName);
            _eventBus.Publish(messageObj.Build());
            return true;
        }
    }

    public class AddArrangementRequestCommandIdentifiedHandler : IdentifiedCommandHandler<AddArrangementRequestCommand, bool?>
    {
        public AddArrangementRequestCommandIdentifiedHandler(IMediator mediator, IRequestManager requestManager) : base(mediator, requestManager)
        {

        }

        protected override bool? CreateResultForDuplicateRequest()
        {
            return true;
        }
    }
}
