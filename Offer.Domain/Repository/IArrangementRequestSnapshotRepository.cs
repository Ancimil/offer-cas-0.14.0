using MicroserviceCommon.Domain.SeedWork;
using Offer.Domain.AggregatesModel.ApplicationAggregate;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Offer.Domain.Repository
{
    public interface IArrangementRequestSnapshotRepository : IRepository<Application>
    {
        Task<bool> SetSnapshotsForApplication(long applicationId, List<ArrangementRequest> arrangmentRequests);
        List<ArrangementRequest> GetArrangementRequestsSnapshots(long applicationId);
        Task<bool> RemoveSnapshotsForApplication(long applicationId, List<ArrangementRequest> arrangmentRequests);
    }
}
