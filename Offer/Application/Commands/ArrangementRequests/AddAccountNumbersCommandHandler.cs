using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MediatR;
using MicroserviceCommon.Application.Commands;
using MicroserviceCommon.Infrastructure.Idempotency;
using Offer.Domain.AggregatesModel.ApplicationAggregate;
using Microsoft.Extensions.Logging;
using System.Threading;

namespace Offer.API.Application.Commands
{
    public class AddAccountNumbersCommandHandler : IRequestHandler<AddAccountNumbersCommand, bool?>
    {
        private readonly IArrangementRequestRepository _arrangementRequestRepository;
        private readonly IMediator _mediator;
        private readonly ILogger<AddAccountNumbersCommand> _logger;

        public AddAccountNumbersCommandHandler(IMediator mediator, IArrangementRequestRepository arrangementRequestRepository,
            ILogger<AddAccountNumbersCommand> logger)
        {
            this._arrangementRequestRepository = arrangementRequestRepository;
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<bool?> Handle(AddAccountNumbersCommand message, CancellationToken cancellationToken)
        {
            ArrangementRequest arrangementRequest = _arrangementRequestRepository.GetArrangementRequest(message.ApplicationNumber, message.ArrangementRequestId);
            if (arrangementRequest == null)
            {
                return null;
            }
            List<ArrangementAccountInfo> arrangementAccounts;
            if (arrangementRequest.Accounts == null)
                arrangementAccounts = new List<ArrangementAccountInfo>();
            else
                arrangementAccounts = arrangementRequest.Accounts;
            foreach (AccountNumbersData accountData in message.AccountNumbers)
            {
                if (accountData != null)
                {
                    _logger.LogInformation("Adding account number {accountNumbers} for {applicationNumber}", accountData.AccountNumber, message.ApplicationNumber);
                    ArrangementAccountInfo newAccount = new ArrangementAccountInfo
                    {
                        AccountNumber = accountData.AccountNumber,
                        RoleKind = accountData.RoleKind
                    };
                    arrangementAccounts.Add(newAccount);
                }
            }
            arrangementRequest.Accounts = arrangementAccounts;
            await _arrangementRequestRepository.UpdateArrangementRequest(arrangementRequest);

            return await _arrangementRequestRepository.UnitOfWork.SaveEntitiesAsync();
        }
    }

    public class AddAccountNumbersCommandHandlerIdentifiedHandler : IdentifiedCommandHandler<AddAccountNumbersCommand, bool?>
    {
        public AddAccountNumbersCommandHandlerIdentifiedHandler(IMediator mediator, IRequestManager requestManager) : base(mediator, requestManager)
        {

        }

        //protected override bool CreateResultForDuplicateRequest()
        //{
        //    return true;
        //}
    }
}
