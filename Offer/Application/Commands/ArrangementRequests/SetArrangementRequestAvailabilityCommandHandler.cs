using System;
using System.Threading.Tasks;
using MediatR;
using MicroserviceCommon.Application.Commands;
using MicroserviceCommon.Infrastructure.Idempotency;
using Offer.Domain.AggregatesModel.ApplicationAggregate;
using System.Threading;

namespace Offer.API.Application.Commands
{
    public class SetArrangementRequestAvailabilityCommandHandler : IRequestHandler<SetArrangementRequestAvailabilityCommand, bool?>
    {
        private readonly IArrangementRequestRepository _arrangementRequestRepository;

        public SetArrangementRequestAvailabilityCommandHandler(
            IArrangementRequestRepository arrangementRequestRepository)
        {
            _arrangementRequestRepository = arrangementRequestRepository ?? throw new ArgumentNullException(nameof(arrangementRequestRepository));
        }

        public async Task<bool?> Handle(SetArrangementRequestAvailabilityCommand message, CancellationToken cancellationToken)
        {
            var result = await _arrangementRequestRepository.SetArragementRequestAvailability(message.ApplicationId,
                message.ArrangementRequestId, message.Enabled);
            await _arrangementRequestRepository.UnitOfWork.SaveChangesAsync();
            return result;
        }
    }

    public class SetArrangementRequestAvailabilityCommandIdentifiedHandler : IdentifiedCommandHandler<SetArrangementRequestAvailabilityCommand, bool?>
    {
        public SetArrangementRequestAvailabilityCommandIdentifiedHandler(IMediator mediator, IRequestManager requestManager) : base(mediator, requestManager)
        {

        }

        //protected override bool CreateResultForDuplicateRequest()
        //{
        //    return true;
        //}
    }
}
