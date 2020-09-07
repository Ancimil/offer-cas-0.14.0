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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Offer.API.Application.Commands.Application
{
    public class PutExtendedPartyCommandHandler : IRequestHandler<PutExtendedPartyCommand, CommandStatus>
    {
        private readonly IApplicationRepository _applicationRepository;
        private readonly IMediator _mediator;
        private readonly IEventBus _eventBus;
        private readonly ILogger<PutExtendedPartyCommand> _logger;
        private readonly MessageEventFactory _messageEventFactory;
        private readonly IAuditClient _auditClient;

        public PutExtendedPartyCommandHandler(IApplicationRepository applicationRepository, IMediator mediator, IEventBus eventBus, 
            ILogger<PutExtendedPartyCommand> logger, MessageEventFactory messageEventFactory, IAuditClient auditClient)
        {
            _applicationRepository = applicationRepository;
            _mediator = mediator;
            _eventBus = eventBus;
            _logger = logger;
            _messageEventFactory = messageEventFactory;
            _auditClient = auditClient;
        }

        public async Task<CommandStatus> Handle(PutExtendedPartyCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var application = await _applicationRepository.GetAsync(request.ApplicationNumber);
                if (application == null)
                {
                    return new CommandStatus { CommandResult = StandardCommandResult.NOT_FOUND };
                }
                application.Extended = application.Extended ?? new Dictionary<string, IDictionary<string, JToken>>();
                foreach (var section in request.Extended?.Keys?.Except(application.Extended?.Keys))
                {
                    application.Extended[section] = request.Extended[section];
                }
                //application.Extended = request.Extended;
                _applicationRepository.Update(application);
                await _applicationRepository.UnitOfWork.SaveChangesAsync();

                await _auditClient.WriteLogEntry(AuditLogEntryAction.Put, AuditLogEntryStatus.Success, "application-extended-data", request.ApplicationNumber.ToString(), request.Extended);

                return new CommandStatus { CommandResult = StandardCommandResult.OK };
            }
            catch (Exception e)
            {
                return new CommandStatus { CommandResult = StandardCommandResult.INTERNAL_ERROR, Exception = e };
            }
        }

        public class PutExtendedDataIdentifiedCommandHandler : IdentifiedCommandHandler<PutExtendedPartyCommand, CommandStatus>
        {
            public PutExtendedDataIdentifiedCommandHandler(IMediator mediator, IRequestManager requestManager) : base(mediator, requestManager)
            {
            }
            protected override CommandStatus CreateResultForDuplicateRequest()
            {
                return new CommandStatus { CommandResult = StandardCommandResult.OK };
            }
        }
    }
}
