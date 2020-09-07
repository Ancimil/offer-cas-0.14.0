using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace Offer.Domain.Services
{
    public interface IArrangementService
    {
        Task<JArray> GetArrangements(string customerNumber, string activeStatuses = null, string activeRoles = null, string arrangementType = null);
    }
}
