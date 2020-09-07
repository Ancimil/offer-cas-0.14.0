using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Offer.Domain.AggregatesModel.ApplicationAggregate;
using Offer.Domain.AggregatesModel.ApplicationAggregate.Interfaces;
using Offer.Domain.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Offer.Infrastructure.Repositories
{
    public class ProductSnapshotRepository : IProductSnapshotRepository
    {
        private readonly OfferDBContext _context;
        public ProductSnapshotRepository(OfferDBContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }
        public Task<ProductSnapshotDb> GetProductSnapshot(string hashCode)
        {
            throw new NotImplementedException();
        }

        public async Task<ProductSnapshotDb> PostProductSnapshot(ProductSnapshot productSnapshot)
        {
            string hashCode = OfferUtility.CreateMD5(JsonConvert.SerializeObject(productSnapshot));
            var product = await _context.ProductSnapshots.Where(p => p.Hash.Equals(hashCode)).FirstOrDefaultAsync();
            if (product != null)
            {
                return product;
            }

            ProductSnapshotDb productSnapshotDb = new ProductSnapshotDb
            {
                Hash = hashCode,
                ProductSnapshot = productSnapshot
            };

            ProductSnapshotDb _savedRequest = _context.ProductSnapshots.Add(productSnapshotDb).Entity;
            await _context.SaveChangesAsync();
            return _savedRequest;

        }

        public Task<ProductSnapshotDb> UpdateProductSnapshot(ProductSnapshotDb productSnapshot)
        {
            throw new NotImplementedException();
        }
    }
}
