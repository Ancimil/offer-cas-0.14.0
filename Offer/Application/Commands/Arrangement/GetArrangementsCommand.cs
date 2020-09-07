using MediatR;
using Newtonsoft.Json.Linq;

namespace Offer.API.Application.Commands
{
    public class GetArrangementsCommand : IRequest<JArray>
    {
        public string CustomerNumber { get; set; }
        public string ActiveStatuses { get; set; }
        public string ActiveRoles { get; set; }
        public string ArrangementType { get; set; }
    }
}
