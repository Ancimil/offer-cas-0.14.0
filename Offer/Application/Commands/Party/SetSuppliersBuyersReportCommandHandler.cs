using MediatR;
using MicroserviceCommon.Application.Commands;
using MicroserviceCommon.Infrastructure.Idempotency;
using Offer.Domain.Repository;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Offer.API.Application.Commands
{

    // Extracted in separate command in order to split logic for finding and updating involved party
    // and leaving posibility to di it from http request
    public class SetSuppliersBuyersReportCommandHandler : IRequestHandler<SetSuppliersBuyersReportCommand, bool?>
    {
        private readonly IInvolvedPartyRepository _involvedPartyRepository;

        public SetSuppliersBuyersReportCommandHandler(IInvolvedPartyRepository involvedPartyRepository)
        {
            _involvedPartyRepository = involvedPartyRepository ?? throw new ArgumentNullException(nameof(involvedPartyRepository));
        }
        public async Task<bool?> Handle(SetSuppliersBuyersReportCommand message, CancellationToken cancellationToken)
        {
            return await _involvedPartyRepository.SetSuppliersBuyersReportForParty(message.ApplicationNumber,
                message.CustomerNumber, message.SuppliersBuyersReportId);
        }
    }
    public class SetSuppliersBuyersReportIdentifiedCommandHandler : IdentifiedCommandHandler<SetSuppliersBuyersReportCommand, bool?>
    {
        public SetSuppliersBuyersReportIdentifiedCommandHandler(IMediator mediator, IRequestManager requestManager) : base(mediator, requestManager)
        {
        }

        protected override bool? CreateResultForDuplicateRequest()
        {
            return false;
        }
    }

}
