using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Offer.Domain.AggregatesModel.ApplicationAggregate;
using System;
using System.Collections.Generic;
using System.Text;

namespace Offer.Infrastructure.EntityConfigurations
{
    public class ProductSnapshotEntityTypeConfiguration : IEntityTypeConfiguration<ProductSnapshotDb>
    {
        public void Configure(EntityTypeBuilder<ProductSnapshotDb> builder)
        {
            builder.ToTable("product_snapshots");

            builder.HasKey(a => a.Hash);
            builder.Property(p => p._ProductSnapshot).HasColumnName("ProdctSnapshot");
            builder.HasMany(c => c.ArrangementRequests)
            .WithOne(e => e.ProductSnapshotDb)
            .HasForeignKey(d => d._ProductSnapshotHash);
        }
    }
}
