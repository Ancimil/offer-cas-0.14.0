using MediatR;
using MicroserviceCommon.Models;

namespace Offer.API.Application.Commands
{
    public class RetrieveCurrentExposureCommand : IRequest<CommandStatus>
    {
        public long ApplicationNumber { get; set; }
        public string ActiveStatuses { get; set; }
        public string ActiveRoles { get; set; }
        public string ArrangementType { get; set; }
        // public string CustomerNumber { get; set; }
        public string Username { get; set; }

        public RetrieveCurrentExposureCommand(long applicationNumber, string username = null, string activeStatuses = null, string activeRoles = null, string arrangementType = null)
        {
            ApplicationNumber = applicationNumber;
            ActiveStatuses = activeStatuses;
            ActiveRoles = activeRoles;
            ArrangementType = arrangementType;
            // CustomerNumber = customerNumber;
            Username = username;
        }
    }
}
