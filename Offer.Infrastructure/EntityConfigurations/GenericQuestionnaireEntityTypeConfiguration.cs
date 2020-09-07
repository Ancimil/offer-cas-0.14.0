using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Offer.Domain.AggregatesModel.ApplicationAggregate;

namespace Offer.Infrastructure.EntityConfigurations
{
    public class GenericQuestionnaireEntityTypeConfiguration : IEntityTypeConfiguration<GenericQuestionnaire>
    {
        public void Configure(EntityTypeBuilder<GenericQuestionnaire> builder)
        {
            builder.Property(b => b._Entries).HasColumnName("Entries");
        }
    }
}
