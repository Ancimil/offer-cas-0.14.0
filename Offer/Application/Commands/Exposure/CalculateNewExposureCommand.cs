using MediatR;
using MicroserviceCommon.Models;

namespace Offer.API.Application.Commands
{
    public class CalculateNewExposureCommand : IRequest<CommandStatus<Currency>>
    {
        public long ApplicationNumber { get; set; }
        public bool RetrieveCurrentExposure { get; set; }
    }
}
