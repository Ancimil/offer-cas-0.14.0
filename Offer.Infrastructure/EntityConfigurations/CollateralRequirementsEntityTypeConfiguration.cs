using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Offer.Domain.AggregatesModel.ApplicationAggregate;

namespace Offer.Infrastructure.EntityConfigurations
{
    public class CollateralRequirementsEntityTypeConfiguration : IEntityTypeConfiguration<CollateralRequirement>
    {
        public void Configure(EntityTypeBuilder<CollateralRequirement> builder)
        {
            builder.ToTable("collateral_requirements");

            builder.HasKey(a => new { a.ApplicationId, a.ArrangementRequestId, a.CollateralRequirementId });
           
        }
    }
}
