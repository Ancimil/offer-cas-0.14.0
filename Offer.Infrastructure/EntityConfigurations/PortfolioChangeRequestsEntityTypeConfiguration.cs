using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Offer.Domain.AggregatesModel.ApplicationAggregate;

namespace Offer.Infrastructure.EntityConfigurations
{
    class PortfolioChangeRequestsEntityTypeConfiguration : IEntityTypeConfiguration<PortfolioChangeRequests>
    {
        public void Configure(EntityTypeBuilder<PortfolioChangeRequests> builder)
        {
            builder.ToTable("portfolio_change_requests");

            builder.HasKey(a => new { a.PortfolioChangeRequestId });
            builder.Property(a => a.PortfolioChangeRequestId)
                    .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn)
                    .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn);
        }
    }
}