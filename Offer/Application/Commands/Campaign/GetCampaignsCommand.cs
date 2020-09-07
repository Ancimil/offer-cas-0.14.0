using MediatR;
using PriceCalculation.Models.LeadModel;

namespace Offer.API.Application.Commands
{
    public class GetCampaignsCommand : IRequest<LeadList>
    {
        public string CustomerNumber { get; set; }

        public GetCampaignsCommand(string customerNumber)
        {
            CustomerNumber = customerNumber;
        }
    }
}
