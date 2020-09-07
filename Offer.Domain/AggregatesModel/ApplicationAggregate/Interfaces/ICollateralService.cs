using MicroserviceCommon.Models;
using System.Threading.Tasks;

namespace Offer.Domain.AggregatesModel.ApplicationAggregate
{
    public interface ICollateralService
    {
        Task<CollateralCodeList> GetCollateralCodesForArrangementCode(string collateralArrangementCode);
        Task<ClassificationSchema> GetCollateralClassificationSchema(string classficationSchemaId);
    }
}
