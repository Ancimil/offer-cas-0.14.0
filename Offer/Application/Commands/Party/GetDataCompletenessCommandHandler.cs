using AutoMapper;
using MediatR;
using MicroserviceCommon.Application.Commands;
using MicroserviceCommon.Infrastructure.Idempotency;
using Microsoft.Extensions.Logging;
using Offer.Domain.AggregatesModel.ApplicationAggregate;
using Offer.Domain.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Offer.API.Application.Commands
{
    public class GetDataCompletenessCommandHandler : IRequestHandler<GetDataCompletenessCommand, DataCompletenessResponse>
    {
        private readonly IInvolvedPartyRepository _involvedPartyRepository;
        private readonly IApplicationRepository _applicationRepository;
        private readonly ILogger<GetDataCompletenessCommand> _logger;

        public GetDataCompletenessCommandHandler(IInvolvedPartyRepository involvedPartyRepository, 
            ILogger<GetDataCompletenessCommand> logger,
            IApplicationRepository applicationRepository)
        {
            _involvedPartyRepository = involvedPartyRepository ?? throw new ArgumentNullException(nameof(involvedPartyRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _applicationRepository = applicationRepository ?? throw new ArgumentNullException(nameof(applicationRepository));
        }
        public async Task<DataCompletenessResponse> Handle(GetDataCompletenessCommand request, CancellationToken cancellationToken)
        {
            var party = await _involvedPartyRepository.GetParty(request.ApplicationNumber, request.PartyId);
            if (party == null)
            {
                return null;
            }
            var involvedParties = await _applicationRepository.GetInvolvedParties(request.ApplicationNumber);

            var dataCompleteness = Mapper.Map<Domain.AggregatesModel.ApplicationAggregate.Party, DataCompletenessResponse>(party);

            var representative = involvedParties.Where(i => i.PartyRole == PartyRole.CustomerRepresentative).FirstOrDefault();
            dataCompleteness.IdDataComplete = !string.IsNullOrEmpty(party._IdentificationDocument) ? true : false;
            if ((party is IndividualParty))
            {
                dataCompleteness.EmploymentDataStated = !string.IsNullOrEmpty((party as IndividualParty)._EmploymentData) ?  true : false;
                dataCompleteness.HouseholdInformationStated = !string.IsNullOrEmpty((party as IndividualParty)._HouseholdInfo) ? true : false;
                dataCompleteness.IncomeStated = (!string.IsNullOrEmpty((party as IndividualParty)._FinancialProfile) &&
                    (party as IndividualParty).FinancialProfile?.IncomeInfo.Count > 0) ? true : false;
            }
            return dataCompleteness;
        }

        public class GetDataCompletenessIdentifiedCommandHandler : IdentifiedCommandHandler<GetDataCompletenessCommand, DataCompletenessResponse>
        {
            public GetDataCompletenessIdentifiedCommandHandler(IMediator mediator, IRequestManager requestManager) : base(mediator, requestManager)
            {
            }
            protected override DataCompletenessResponse CreateResultForDuplicateRequest()
            {
                return new DataCompletenessResponse();
            }
        }
    }
}
