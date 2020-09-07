using MediatR;
using MicroserviceCommon.Models;

namespace Offer.API.Application.Commands.Application
{
    public class ChangePortfolioCommand : IRequest<CommandStatus>
    {
        public long ApplicationNumber { get; set; }
        public string RequestedValue { get; set; }
        public string RequestDescription { get; set; }
        public bool AuditLog { get; set; }
    }
}
