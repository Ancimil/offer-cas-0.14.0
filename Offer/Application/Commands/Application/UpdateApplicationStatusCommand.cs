using MediatR;
using Offer.Domain.AggregatesModel.ApplicationAggregate;

namespace Offer.API.Application.Commands
{
    public class UpdateApplicationStatusCommand : IRequest<bool>
    {

        public long ApplicationId { get; set; }
        public ApplicationStatus? Status { get; set; } = null;
        public StatusInformation StatusInformation { get; set; }
        public string Phase { get; set; }

    }
}
