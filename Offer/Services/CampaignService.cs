using MediatR;
using MicroserviceCommon.Application.Commands;
using Offer.API.Application.Commands;
using Offer.Domain.Services;
using PriceCalculation.Models.LeadModel;
using System;
using System.Threading.Tasks;

namespace Offer.API.Services
{
    public class CampaignService : ICampaignService
    {
        private readonly IMediator _mediator;

        public CampaignService(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }
        public async Task<LeadList> GetCampaigns(string customerNumber)
        {
            var getLeads = new IdentifiedCommand<GetCampaignsCommand, LeadList>(new GetCampaignsCommand(customerNumber), new Guid());
            return await _mediator.Send(getLeads);
        }
    }
}