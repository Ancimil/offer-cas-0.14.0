using AuditClient;
using AuditClient.Model;
using MediatR;
using MicroserviceCommon.Application.Commands;
using MicroserviceCommon.Infrastructure.Idempotency;
using MicroserviceCommon.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Offer.Domain.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Offer.API.Application.Commands
{
    public class PutExtendedPartyCommandHandler : IRequestHandler<PutExtendedPartyCommand, CommandStatus>
    {
        private readonly IInvolvedPartyRepository _involvedPartyRepository;
        private readonly IAuditClient _auditClient;
        private readonly ILogger<PutExtendedPartyCommand> _logger;

        public PutExtendedPartyCommandHandler(
            ILogger<PutExtendedPartyCommand> logger,
            IInvolvedPartyRepository involvedPartyRepository,
            IAuditClient auditClient)
        {
            _logger = logger;
            _involvedPartyRepository = involvedPartyRepository;
            _auditClient = auditClient;
        }

        public async Task<CommandStatus> Handle(PutExtendedPartyCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var party = await _involvedPartyRepository.GetParty(request.ApplicationNumber, request.PartyId);
                if(party == null)
                {
                    return new CommandStatus { CommandResult = StandardCommandResult.NOT_FOUND };
                }
                party.Extended = party.Extended ?? new Dictionary<string, IDictionary<string, JToken>>();
                foreach (var section in request.Extended.Keys.Except(party.Extended?.Keys))
                {
                    party.Extended[section] = request.Extended[section];
                }
                party.Extended = request.Extended;
                _involvedPartyRepository.Update(party);

                try
                {
                    await _auditClient.WriteLogEntry(AuditLogEntryAction.Update, AuditLogEntryStatus.Success, "party", request.ApplicationNumber.ToString(), "Extended party section updated for " + party.PartyId, new { });

                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Audit error in UpdateApplicationStatusCommandHandler");
                }

                return new CommandStatus { CommandResult = StandardCommandResult.OK };
            }
            catch (Exception e)
            {
                return new CommandStatus { CommandResult = StandardCommandResult.INTERNAL_ERROR, Exception = e };
            }
        }

        public class PutExtendedPartyIdentifiedCommandHandler : IdentifiedCommandHandler<PutExtendedPartyCommand, CommandStatus>
        {
            public PutExtendedPartyIdentifiedCommandHandler(IMediator mediator, IRequestManager requestManager) : base(mediator, requestManager)
            {
            }
            protected override CommandStatus CreateResultForDuplicateRequest()
            {
                return new CommandStatus { CommandResult = StandardCommandResult.OK };
            }
        }
    }
}
