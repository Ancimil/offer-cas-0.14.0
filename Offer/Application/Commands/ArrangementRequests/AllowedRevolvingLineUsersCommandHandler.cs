using System;
using System.Threading.Tasks;
using MediatR;
using MicroserviceCommon.Application.Commands;
using MicroserviceCommon.Infrastructure.Idempotency;
using Offer.Domain.AggregatesModel.ApplicationAggregate;
using Microsoft.Extensions.Logging;
using System.Threading;
using Asseco.EventBus.Abstractions;
using MicroserviceCommon.Extensions.Broker;

namespace Offer.API.Application.Commands
{

    public class AllowedRevolvingLineUsersCommandHandler : IRequestHandler<AllowedRevolvingLineUsersCommand, bool?>
    {
        private readonly IArrangementRequestRepository _arrangementRequestRepository;
        private readonly IApplicationRepository _applicationRepository;
        protected readonly IEventBus _eventBus;
        protected readonly MessageEventFactory _eventFactory;
        private readonly ILogger<AllowedRevolvingLineUsersCommandHandler> _logger;

        public AllowedRevolvingLineUsersCommandHandler(
            IArrangementRequestRepository arrangementRequestRepository,
            IApplicationRepository applicationRepository,
            IEventBus eventBus,
            MessageEventFactory eventFactory,
            ILogger<AllowedRevolvingLineUsersCommandHandler> logger)
        {
            _arrangementRequestRepository = arrangementRequestRepository ?? throw new ArgumentNullException(nameof(arrangementRequestRepository));
            _applicationRepository = applicationRepository ?? throw new ArgumentNullException(nameof(applicationRepository));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _eventFactory = eventFactory ?? throw new ArgumentNullException(nameof(eventFactory));
            _logger = logger;
        }

        public async Task<bool?> Handle(AllowedRevolvingLineUsersCommand message, CancellationToken cancellationToken)
        {
            var application = await _applicationRepository.GetAsync(message.ApplicationId);
            if (application != null)
            {
                CreditLineLimits cll = new CreditLineLimits();
                cll.AllowedRevolvingLineUsers = message.AllowedRevolvingLineUsers;
                var res = _arrangementRequestRepository.SetCreditLineUsers
                     (message.ApplicationId, message.ArrangementRequestId, cll, application);

                var appNumberString = "0000000000" + message.ApplicationId;
                appNumberString = appNumberString.Substring(appNumberString.Length - 10);
                _logger.LogDebug("Before publish ", appNumberString);
                var messageObj = _eventFactory.CreateBuilder("offer", "credit-line-limits-added")
                    .AddBodyProperty("application-number", appNumberString)
                    .AddHeaderProperty("application-number", appNumberString)
                    .Build();
                _eventBus.Publish(messageObj);
                _logger.LogDebug("After publish ", appNumberString);
                return await res;
            }
            return null;
        }
    }

    public class AllowedRevolvingLineUsersCommandHandlerIdentifiedHandler : IdentifiedCommandHandler<AllowedRevolvingLineUsersCommand, bool?>
    {
        public AllowedRevolvingLineUsersCommandHandlerIdentifiedHandler(IMediator mediator, IRequestManager requestManager) : base(mediator, requestManager)
        {

        }
    }
}
