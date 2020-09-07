using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Offer.Domain.AggregatesModel.ApplicationAggregate;

namespace Offer.Infrastructure.EntityConfigurations
{
    public class OrganizationUnitEntityTypeConfiguration : IEntityTypeConfiguration<OrganizationUnit>
    {
        public void Configure(EntityTypeBuilder<OrganizationUnit> builder)
        {
            builder.HasKey(x => x.Code);
            builder.Property(x => x.NavigationCode).IsRequired();
            builder.Property(x => x.Code).HasMaxLength(64);
        }
    }
}
