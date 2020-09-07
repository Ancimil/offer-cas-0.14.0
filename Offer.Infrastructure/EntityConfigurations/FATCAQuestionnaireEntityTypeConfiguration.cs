using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Offer.Domain.AggregatesModel.ApplicationAggregate;

namespace Offer.Infrastructure.EntityConfigurations
{
    class FATCAQuestionnaireEntityTypeConfiguration : IEntityTypeConfiguration<FATCAQuestionnaire>
    {

        public void Configure(EntityTypeBuilder<FATCAQuestionnaire> builder)
        {
            builder.Property(b => b._Entries).HasColumnName("Entries");
        }
    }
}
