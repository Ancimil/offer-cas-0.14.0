using System;
using System.Threading.Tasks;
using MediatR;
using MicroserviceCommon.Application.Commands;
using MicroserviceCommon.Infrastructure.Idempotency;
using Offer.Domain.AggregatesModel.ApplicationAggregate;
using System.Threading;
using Asseco.EventBus.Abstractions;
using MicroserviceCommon.Extensions.Broker;

namespace Offer.API.Application.Commands
{
    public class ArrangementRequestsAvailabilityCommandHandler : IRequestHandler<ArrangementRequestsAvailabilityCommand, bool?>
    {
        private readonly IArrangementRequestRepository _arrangementRequestRepository;
        private readonly IApplicationRepository _applicationRepository;
        protected readonly IEventBus _eventBus;
        protected readonly MessageEventFactory _eventFactory;

        public ArrangementRequestsAvailabilityCommandHandler(
            IArrangementRequestRepository arrangementRequestRepository,
            IApplicationRepository applicationRepository,
            IEventBus eventBus,
            MessageEventFactory eventFactory)
        {
            _arrangementRequestRepository = arrangementRequestRepository ?? throw new ArgumentNullException(nameof(arrangementRequestRepository));
            _applicationRepository = applicationRepository ?? throw new ArgumentNullException(nameof(applicationRepository));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _eventFactory = eventFactory ?? throw new ArgumentNullException(nameof(eventFactory));
        }

        public async Task<bool?> Handle(ArrangementRequestsAvailabilityCommand message, CancellationToken cancellationToken)
        {
            var application = await _applicationRepository.GetAsync(message.ApplicationId);
            if (application == null)
            {
                return null;
            }
            var res = _arrangementRequestRepository.UpdateArrangementRequestsAvailability(application, message.Availabilities);
            var messageObj = _eventFactory.CreateBuilder("offer", "arrangement-request-availability-changed")
                .AddHeaderProperty("application-number", message.ApplicationNumber)
                .Build();
            _eventBus.Publish(messageObj);
            return await res;
        }
    }

    public class ArrangementRequestsAvailabilityCommandHandlerIdentifiedHandler : IdentifiedCommandHandler<ArrangementRequestsAvailabilityCommand, bool?>
    {
        public ArrangementRequestsAvailabilityCommandHandlerIdentifiedHandler(IMediator mediator, IRequestManager requestManager) : base(mediator, requestManager)
        {

        }
    }
}
