using AutoMapper;
using MediatR;
using MicroserviceCommon.Application.Commands;
using MicroserviceCommon.Infrastructure.Idempotency;
using Offer.Domain.AggregatesModel.ApplicationAggregate.ApplicantModel;
using Offer.Domain.Repository;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Offer.API.Application.Commands
{

    public class UpdateFinancialProfileCommandHandler : IRequestHandler<UpdateFinancialProfileCommand, FinancialProfile>
    {
        private readonly IInvolvedPartyRepository _involvedPartyRepository;

        public UpdateFinancialProfileCommandHandler(IInvolvedPartyRepository involvedPartyRepository)
        {
            _involvedPartyRepository = involvedPartyRepository ?? throw new ArgumentNullException(nameof(involvedPartyRepository));
        }

        public async Task<FinancialProfile> Handle(UpdateFinancialProfileCommand message, CancellationToken cancellationToken)
        {
            var data = Mapper.Map<UpdateFinancialProfileCommand, FinancialProfile>(message);
            return await _involvedPartyRepository.UpdatePartyFinancialProfile(message.ApplicationNumber, message.PartyId, data);
        }
    }
    public class UpdateFinancialProfileIdentifiedCommandHandler : IdentifiedCommandHandler<UpdateFinancialProfileCommand, FinancialProfile>
    {
        public UpdateFinancialProfileIdentifiedCommandHandler(IMediator mediator, IRequestManager requestManager) : base(mediator, requestManager)
        {
        }

        protected override FinancialProfile CreateResultForDuplicateRequest()
        {
            return null;
        }
    }

}
