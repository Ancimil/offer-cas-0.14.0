using MediatR;
using MicroserviceCommon.Models;

namespace Offer.API.Application.Commands.Application
{
    public class PatchApplicationCommand : IRequest<CommandStatus>
    {
        public long ApplicationNumber { get; set; }
        public string OrganizationUnitCode { get; set; }

        public bool AuditLog { get; set; }

        public PatchApplicationCommand(long applicationNumber, string organizationUnitCode)
        {
            ApplicationNumber = applicationNumber;
            OrganizationUnitCode = organizationUnitCode;
        }
    }
}
