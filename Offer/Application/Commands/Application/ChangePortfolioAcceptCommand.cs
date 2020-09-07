using MediatR;
using MicroserviceCommon.Models;

namespace Offer.API.Application.Commands.Application
{
    public class ChangePortfolioAcceptCommand : IRequest<CommandStatus>
    {
        public long PortfolioChangeRequestId { get; set; }
        public long ApplicationNumber { get; set; }
        public string FinalValue { get; set; }
        public bool AuditLog { get; set; }
    }
}
