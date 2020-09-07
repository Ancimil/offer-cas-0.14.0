using Offer.Domain.AggregatesModel.ApplicationAggregate;
using System.Threading.Tasks;

namespace Offer.Domain.Services
{
    public interface IProductService
    {
        Task<ProductSnapshot> GetProductData(string productCode, string include = null, string customerId = "");

    }
}
