using MicroserviceCommon.Domain.SeedWork;
using Newtonsoft.Json.Linq;
using Offer.Domain.AggregatesModel.ApplicationAggregate.AlternativeOfferModel;
using Offer.Domain.AggregatesModel.ApplicationAggregate.ArrangementRequestModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Offer.Domain.AggregatesModel.ApplicationAggregate
{
    public interface IArrangementRequestRepository : IRepository<Application>
    { 

        List<ArrangementRequest> GetArrangementRequests(long applicationNumber, string include, string trim, bool includePotential = false);
        Task<ArrangementRequest> UpdateArrangementRequest(ArrangementRequest arrangementRequest);
        ArrangementRequest GetArrangementRequest(long applicationNumber, int arrangementRequestId, string include, string trim);
        Task<List<ArrangementRequest>> GetArrangementRequestsByProductCode(long applicationNumber, string productCode, string include = null, string trim = null);
        ArrangementRequest GetArrangementRequest(long applicationNumber, int arrangementRequestId);
        bool? DeleteArrangementRequest(long applicationNumber, int arrangementRequestId);
        bool AddArrangementRequest(ArrangementRequest request);
        CollateralRequirement AddCollateralRequirement(CollateralRequirement requirement);
        List<CollateralRequirement> GetCollateralRequirementsForArrangementRequest(long applicationNumber, int arrangementRequestId);
        CollateralRequirement GetCollateralRequirementById(long applicationNumber, int arrangementRequestId, long collateralRequirementId);
        CollateralRequirement UpdateCollateralRequirement(CollateralRequirement requirement);
        Task<bool?> DeleteCollateralRequirement(long applicationNumber, int arrangementRequestId, long collateralRequirementId);
        Task<ArrangementRequest> SetApprovedLimits(long applicationNumber, int arrangementRequestId, ApprovedLimits approvedlimits, Application application);
        Task<ArrangementRequest> SetAcceptedValues(long applicationNumber, int arrangementRequestId, AcceptedValues command, Application application);
        List<CollateralRequirementValidation> ValidateCollateralRequirement(long applicationNumber, int arrangementRequestId);
        List<ArrangementRequest> GetBundledRequests(ArrangementRequest arrangementRequest, bool includeSingletons = false);
        Task<bool?> DeleteArrangementRequests(long applicationId, List<ArrangementRequest> arrangementRequests);
        List<ArrangementRequestValidation> ValidateArrangementRequests(long applicationId);
        List<BundleComponentInfo> GetBundledComponentsForApplication(long applicationId);
        Task<bool> UpdateArrangementRequestsAvailability(Application application, List<ArrangementRequestsAvailability> arrangementRequestUpserts);
        Task<bool> AddArrangementRequestsToApplication(Application application, List<ArrangementRequest> arrangementRequests);
        List<ArrangementRequest> GetAvailableProducts(long applicationId);
        Task<bool?> SetArragementRequestAvailability(long applicationId, int arrangementRequestId, bool enabled);
        Task<bool?> SetCreditLineUsers(long applicationNumber, int arrangementRequestId, CreditLineLimits approvedlimits, Application application);
        Task<bool?> SetCreditLineProductCodes(long applicationNumber, int arrangementRequestId, CreditLineLimits approvedlimits, Application application);
        Task<bool?> SetCreditLineProductKinds(long applicationNumber, int arrangementRequestId, CreditLineLimits approvedlimits, Application application);
        IDictionary<string, IDictionary<string, JToken>> GetExtendedArrangementData(long applicationNumber, int arrangementRequestId);
        IDictionary<string, JToken> GetExtendedArrangementDataSection(long applicationNumber, int arrangementRequestId, string sectionName);
        Task<bool?> DeleteExtendedDataSection(long applicationNumber, int arrangementRequestId, string sectionName);
    }
}
