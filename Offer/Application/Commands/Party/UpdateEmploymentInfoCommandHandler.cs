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

    public class UpdateEmploymentInfoCommandHandler : IRequestHandler<UpdateEmploymentInfoCommand, EmploymentData>
    {
        private readonly ILogger<UpdateEmploymentInfoCommand> _logger;
        private readonly IInvolvedPartyRepository _involvedPartyRepository;

        public UpdateEmploymentInfoCommandHandler(ILogger<UpdateEmploymentInfoCommand> logger, IInvolvedPartyRepository involvedPartyRepository)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _involvedPartyRepository = involvedPartyRepository ?? throw new ArgumentNullException(nameof(involvedPartyRepository));
        }

        public async Task<EmploymentData> Handle(UpdateEmploymentInfoCommand message, CancellationToken cancellationToken)
        {
            var data = Mapper.Map<UpdateEmploymentInfoCommand, EmploymentData>(message);
            return await _involvedPartyRepository.UpdatePartyEmploymentInfo(message.ApplicationNumber, message.PartyId, data);
        }
    }
    public class UpdateEmploymentInfoIdentifiedCommandHandler : IdentifiedCommandHandler<UpdateEmploymentInfoCommand, EmploymentData>
    {
        public UpdateEmploymentInfoIdentifiedCommandHandler(IMediator mediator, IRequestManager requestManager) : base(mediator, requestManager)
        {
        }

        protected override EmploymentData CreateResultForDuplicateRequest()
        {
            return null;
        }
    }

}
