using MediatR;
using MicroserviceCommon.Models;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Offer.API.Application.Commands.Application
{
    public class PutExtendedPartyCommand : IRequest<CommandStatus>
    {
        public long ApplicationNumber { get; set; }
        public IDictionary<string, IDictionary<string, JToken>> Extended { get; set; }
    }
}
