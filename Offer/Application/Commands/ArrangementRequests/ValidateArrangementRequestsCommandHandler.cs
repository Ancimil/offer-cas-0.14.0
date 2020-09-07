using AutoMapper;
using MediatR;
using MicroserviceCommon.Application.Commands;
using MicroserviceCommon.Infrastructure.Idempotency;
using Offer.Domain.AggregatesModel.ApplicationAggregate;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Offer.API.Application.Commands.ArrangementRequests
{
    public class ValidateArrangementRequestsCommandHandler :
        IRequestHandler<ValidateArrangementRequestsCommand, ValidateArrangementRequestResponse>
    {
        private readonly IArrangementRequestRepository _requestRepository;

        public ValidateArrangementRequestsCommandHandler(
            IArrangementRequestRepository requestRepository)
        {
            _requestRepository = requestRepository ?? throw new ArgumentNullException(nameof(requestRepository));
        }

        public Task<ValidateArrangementRequestResponse> Handle(
            ValidateArrangementRequestsCommand message, CancellationToken cancellationToken)
        {
            var results = _requestRepository.ValidateArrangementRequests(message.ApplicationId);
            // results = results.Where(i => i.Enabled).ToList();
            var items = Mapper.Map<List<ArrangementRequestValidation>, List<ArrangementRequestValidationData>>(results);
            return Task.FromResult(new ValidateArrangementRequestResponse
            {
                Items = items
            });
        }
    }

    public class ValidateArrangementRequestsCommandIdentifiedHandler : IdentifiedCommandHandler<ValidateArrangementRequestsCommand, ValidateArrangementRequestResponse>
    {
        public ValidateArrangementRequestsCommandIdentifiedHandler(IMediator mediator, IRequestManager requestManager) : base(mediator, requestManager)
        {

        }

        protected override ValidateArrangementRequestResponse CreateResultForDuplicateRequest()
        {
            return null;
        }
    }
}
