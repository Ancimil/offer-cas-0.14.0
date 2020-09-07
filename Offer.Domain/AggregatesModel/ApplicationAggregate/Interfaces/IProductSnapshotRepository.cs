using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Offer.Domain.AggregatesModel.ApplicationAggregate.Interfaces
{
    public interface IProductSnapshotRepository
    {
        Task<ProductSnapshotDb> GetProductSnapshot(string hashCode);
        Task<ProductSnapshotDb> PostProductSnapshot(ProductSnapshot productSnapshot);
        Task<ProductSnapshotDb> UpdateProductSnapshot(ProductSnapshotDb productSnapshot);
    }
}
