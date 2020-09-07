using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Offer.Domain.AggregatesModel.ApplicationAggregate;

namespace Offer.Infrastructure.EntityConfigurations
{
    class IndividualEntityTypeConfiguration : IEntityTypeConfiguration<IndividualParty>
    {
        public void Configure(EntityTypeBuilder<IndividualParty> builder)
        {
            builder.ToTable("individuals");
            builder.Property(b => b._EmploymentData).HasColumnName("EmploymentData");
            builder.Property(b => b._IdentificationDocument).HasColumnName("IdentificationDocument");
            builder.Property(b => b._HouseholdInfo).HasColumnName("HouseholdInfo");
        }
    }
}
