using MediatR;
using Offer.Domain.AggregatesModel.ApplicationAggregate;

namespace Offer.API.Application.Commands
{
    public class PullPartyDataCommand : IRequest<ApplicationView>
    {
        public long ApplicationNumber { get; set; }

        public bool AuditLog { get; set; }

        public PullPartyDataCommand(long applicationNumber)
        {
            ApplicationNumber = applicationNumber;
        }

        public PullPartyDataCommand(long applicationNumber, bool auditLog)
        {
            ApplicationNumber = applicationNumber;
            AuditLog = auditLog;
        }
    }
}
