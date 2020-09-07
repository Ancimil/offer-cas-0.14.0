using MediatR;
using MicroserviceCommon.Application.Commands;
using MicroserviceCommon.Infrastructure.Idempotency;
using MicroserviceCommon.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Offer.Domain.AggregatesModel.ApplicationAggregate;
using Offer.Domain.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Offer.API.Application.Commands
{
    public class PatchInvolvedPartyCommandHandler : IRequestHandler<PatchInvolvedPartyCommand, CommandStatus>
    {
        private readonly IInvolvedPartyRepository _involvedPartyRepository;
        private readonly ILogger<PatchInvolvedPartyCommand> _logger;

        public PatchInvolvedPartyCommandHandler(IInvolvedPartyRepository involvedPartyRepository, ILogger<PatchInvolvedPartyCommand> logger)
        {
            _involvedPartyRepository = involvedPartyRepository ?? throw new ArgumentNullException(nameof(involvedPartyRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<CommandStatus> Handle(PatchInvolvedPartyCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var party = await _involvedPartyRepository.GetParty(request.ApplicationNumber, request.PartyId);
                if (party == null)
                {
                    return new CommandStatus { CommandResult = StandardCommandResult.NOT_FOUND };
                }
                foreach (PropertyInfo prop in request.GetType().GetProperties())
                {
                    if (prop.Name != "ApplicationNumber")
                    {
                        object value = prop.GetValue(request, null);
                        var checkEmptyString = !((value is string) && string.IsNullOrEmpty((value as string).ToString()));
                        if (value != null && party.GetType().GetProperty(prop.Name) != null)
                        {
                            party.GetType().GetProperty(prop.Name).SetValue(party, value);
                        }
                    }
                }

                _involvedPartyRepository.Update(party);

                return new CommandStatus { CommandResult = StandardCommandResult.OK };
            }
            catch (Exception e)
            {
                return new CommandStatus { CommandResult = StandardCommandResult.INTERNAL_ERROR, Exception = e };
            }
        }

        public class PatchInvolvedPartyIdentifiedCommandHandler : IdentifiedCommandHandler<PatchInvolvedPartyCommand, CommandStatus>
        {
            public PatchInvolvedPartyIdentifiedCommandHandler(IMediator mediator, IRequestManager requestManager) : base(mediator, requestManager)
            {
            }
            protected override CommandStatus CreateResultForDuplicateRequest()
            {
                return new CommandStatus { CommandResult = StandardCommandResult.OK };
            }
        }
    }
}
