using MicroserviceCommon.Domain.SeedWork;
using Microsoft.Extensions.Logging;
using Offer.Domain.AggregatesModel.ApplicationAggregate;
using Offer.Domain.Repository;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Offer.Infrastructure.Repositories
{
    public class ArrangementRequestSnapshotRepository : IArrangementRequestSnapshotRepository
    {
        private readonly OfferDBContext _context;
        private readonly ILogger<ArrangementRequestSnapshotRepository> _logger;
        public IUnitOfWork UnitOfWork
        {
            get
            {
                return _context;
            }
        }

        public ArrangementRequestSnapshotRepository(OfferDBContext context,
            ILogger<ArrangementRequestSnapshotRepository> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Task<bool> SetSnapshotsForApplication(long applicationId, List<ArrangementRequest> arrangmentRequests)
        {
            throw new NotImplementedException();
        }

        public List<ArrangementRequest> GetArrangementRequestsSnapshots(long applicationId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> RemoveSnapshotsForApplication(long applicationId, List<ArrangementRequest> arrangmentRequests)
        {
            throw new NotImplementedException();
        }
        //public async Task<bool> SetSnapshotsForApplication(long applicationId, List<ArrangementRequest> arrangmentRequests)
        //{
        //    var productCodes = arrangmentRequests.Select(a => a.ProductCode).Distinct().ToList();
        //    var snapshots = _context.ArrangementRequestSnapshots
        //        .Where(s => s.ApplicationId == applicationId && productCodes.Contains(s.ProductCode))
        //        .ToList();
        //    foreach (var item in arrangmentRequests)
        //    {
        //        if (item is CreditCardFacilityRequest)
        //        {
        //             var x = 5;
        //        }
        //        item.ApplicationId = 0;
        //        item.ArrangementRequestId = 0;
        //        var snapshot = snapshots.FirstOrDefault(s => s.ProductCode == item.ProductCode);
        //        if (snapshot == null)
        //        {
        //            snapshot = new ArrangementRequestSnapshot
        //            {
        //                ApplicationId = applicationId,
        //                ArrangementRequest = item,
        //                ProductCode = item.ProductCode
        //            };
        //        }
        //        else
        //        {
        //            snapshot.ArrangementRequest = item;
        //        }
        //        await _context.ArrangementRequestSnapshots.AddAsync(snapshot);
        //    }
        //    await _context.SaveChangesAsync();
        //    return true;
        //}

        //public List<ArrangementRequest> GetArrangementRequestsSnapshots(long applicationId)
        //{
        //    var snapshots = _context.ArrangementRequestSnapshots
        //        .Where(s => s.ApplicationId == applicationId)
        //        .Select(s => s.ArrangementRequest)
        //        .ToList();
        //    return snapshots;
        //}

        //public async Task<bool> RemoveSnapshotsForApplication(long applicationId, List<ArrangementRequest> arrangmentRequests)
        //{
        //    var productCodes = arrangmentRequests.Select(a => a.ProductCode).Distinct().ToList();
        //    var forDelete = _context.ArrangementRequestSnapshots
        //        .Where(r => r.ApplicationId == applicationId && productCodes.Contains(r.ProductCode))
        //        .ToList();
        //    _context.ArrangementRequestSnapshots.RemoveRange(forDelete);
        //    await _context.SaveChangesAsync();
        //    return true;
        //}
    }
}
