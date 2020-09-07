using System.Threading.Tasks;

namespace Offer.Domain.AggregatesModel.ApplicationAggregate
{
    public interface IMasterPartyDataService
    {
        Task<Party> GetPartyData(Party party);
    }
}
