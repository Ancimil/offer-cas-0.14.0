using System.Collections.Generic;
using MediatR;
using Offer.Domain.AggregatesModel.ApplicationAggregate;

namespace Offer.API.Application.Commands
{
    public class AccountNumbersData
    {
        public ArrangementAccountRoleKind RoleKind { get; set; }
            public string AccountNumber { get; set; }
        }

        public class AddAccountNumbersCommand : IRequest<bool?>
        {
            public int ArrangementRequestId { get; set; }
            public long ApplicationNumber { get; set; }
            public List<AccountNumbersData> AccountNumbers { get; set; }
        }
}
