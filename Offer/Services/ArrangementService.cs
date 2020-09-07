using MediatR;
using MicroserviceCommon.Application.Commands;
using Newtonsoft.Json.Linq;
using Offer.API.Application.Commands;
using Offer.Domain.Services;
using System;
using System.Threading.Tasks;

namespace Offer.API.Services
{
    public class ArrangementService : IArrangementService
    {
        private readonly IMediator _mediator;

        public ArrangementService(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }
        public async Task<JArray> GetArrangements(string customerNumber, string activeStatuses = null, string activeRoles = null, string arrangementType = null)
        {
            var getArrangements = new IdentifiedCommand<GetArrangementsCommand, JArray>(new GetArrangementsCommand
            {
                CustomerNumber = customerNumber
            }, new Guid());
            return await _mediator.Send(getArrangements);
        }
    }
}
