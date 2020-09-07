using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Offer.Domain.AggregatesModel.ApplicationAggregate.ArrangementRequestModel;

namespace Offer.Infrastructure.EntityConfigurations
{
    public class ArrangementRequestSnapshotEntityTypeConfiguration : IEntityTypeConfiguration<ArrangementRequestSnapshotOld>
    {
        public void Configure(EntityTypeBuilder<ArrangementRequestSnapshotOld> builder)
        {
            builder.ToTable("arrangement_requests_snapshots");
            builder.HasKey(a => new { a.ArrangementRequestSnapshotId });
            builder.Property(a => a.ArrangementRequestSnapshotId)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn)
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn);
            builder.Property(x => x._ArrangementRequest).HasColumnName("ArrangementRequest");
            builder.HasIndex(a => new { a.ApplicationId, a.ProductCode }).IsUnique();
        }
    }
}
