using Asseco.EventBus.Abstractions;
using MediatR;
using MicroserviceCommon.Application.Commands;
using MicroserviceCommon.Extensions.Broker;
using MicroserviceCommon.Infrastructure.Idempotency;
using MicroserviceCommon.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Offer.Domain.AggregatesModel.ApplicationAggregate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Offer.API.Application.Commands.ArrangementRequests
{
    public class PutExtendedPartyCommandHandler : IRequestHandler<PutExtendedPartyCommand, CommandStatus>
    {
        private readonly IApplicationRepository _applicationRepository;
        private readonly IArrangementRequestRepository _arrangementRequestRepository;
        private readonly IMediator _mediator;
        private readonly IEventBus _eventBus;
        private readonly ILogger<PutExtendedPartyCommand> _logger;
        private readonly MessageEventFactory _messageEventFactory;

        public PutExtendedPartyCommandHandler(IApplicationRepository applicationRepository, 
            IArrangementRequestRepository arrangementRequestRepository, 
            IMediator mediator, IEventBus eventBus, 
            ILogger<PutExtendedPartyCommand> logger, 
            MessageEventFactory messageEventFactory)
        {
            _applicationRepository = applicationRepository;
            _arrangementRequestRepository = arrangementRequestRepository;
            _mediator = mediator;
            _eventBus = eventBus;
            _logger = logger;
            _messageEventFactory = messageEventFactory;
        }

        public async Task<CommandStatus> Handle(PutExtendedPartyCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var arrangementRequest = _arrangementRequestRepository.GetArrangementRequest(request.ApplicationNumber, request.ArrangementRequestId);
                if (arrangementRequest == null)
                {
                    return await Task.FromResult(new CommandStatus { CommandResult = StandardCommandResult.NOT_FOUND });
                }
                arrangementRequest.Extended = arrangementRequest.Extended ?? new Dictionary<string, IDictionary<string, JToken>>();
                foreach (var section in request.Extended.Keys.Except(arrangementRequest.Extended?.Keys))
                {
                    arrangementRequest.Extended[section] = request.Extended[section];
                }
                arrangementRequest.Extended = request.Extended;
                await _arrangementRequestRepository.UpdateArrangementRequest(arrangementRequest);

                return await Task.FromResult(new CommandStatus { CommandResult = StandardCommandResult.OK });
            }
            catch (Exception e)
            {
                return await Task.FromResult(new CommandStatus { CommandResult = StandardCommandResult.INTERNAL_ERROR, Exception = e });
            }
        }

        public class PutExtendedArrangementIdentifiedCommandHandler : IdentifiedCommandHandler<PutExtendedPartyCommand, CommandStatus>
        {
            public PutExtendedArrangementIdentifiedCommandHandler(IMediator mediator, IRequestManager requestManager) : base(mediator, requestManager)
            {
            }
            protected override CommandStatus CreateResultForDuplicateRequest()
            {
                return new CommandStatus { CommandResult = StandardCommandResult.OK };
            }
        }
    }
}
