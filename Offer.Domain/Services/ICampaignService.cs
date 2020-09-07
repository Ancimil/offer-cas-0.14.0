using PriceCalculation.Models.LeadModel;
using System.Threading.Tasks;

namespace Offer.Domain.Services
{
    public interface ICampaignService
    {
        Task<LeadList> GetCampaigns(string customerNumber);
    }
}
