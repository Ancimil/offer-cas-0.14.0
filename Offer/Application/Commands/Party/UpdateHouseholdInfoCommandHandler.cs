using AutoMapper;
using MediatR;
using MicroserviceCommon.Application.Commands;
using MicroserviceCommon.Infrastructure.Idempotency;
using Microsoft.Extensions.Logging;
using Offer.Domain.AggregatesModel.ApplicationAggregate;
using Offer.Domain.Repository;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Offer.API.Application.Commands
{

    public class UpdateHouseholdInfoCommandHandler : IRequestHandler<UpdateHouseholdInfoCommand, Household>
    {
        private readonly ILogger<UpdateHouseholdInfoCommand> _logger;
        private readonly IInvolvedPartyRepository _involvedPartyRepository;

        public UpdateHouseholdInfoCommandHandler(ILogger<UpdateHouseholdInfoCommand> logger, IInvolvedPartyRepository involvedPartyRepository)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _involvedPartyRepository = involvedPartyRepository ?? throw new ArgumentNullException(nameof(involvedPartyRepository));
        }

        public async Task<Household> Handle(UpdateHouseholdInfoCommand message, CancellationToken cancellationToken)
        {
            var data = Mapper.Map<UpdateHouseholdInfoCommand, Household>(message);
            return await _involvedPartyRepository.UpdatePartyHouseholdInfo(message.ApplicationNumber, message.PartyId, data);
        }
    }
    public class UpdateHouseholdInfoIdentifiedCommandHandler : IdentifiedCommandHandler<UpdateHouseholdInfoCommand, Household>
    {
        public UpdateHouseholdInfoIdentifiedCommandHandler(IMediator mediator, IRequestManager requestManager) : base(mediator, requestManager)
        {
        }

        protected override Household CreateResultForDuplicateRequest()
        {
            return null;
        }
    }

}
