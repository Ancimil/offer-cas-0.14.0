using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Offer.Domain.AggregatesModel.ApplicationAggregate;



namespace Offer.Infrastructure.EntityConfigurations
{
    public class FinancialQuestionnaireEntityTypeConfiguration : IEntityTypeConfiguration<FinancialQuestionnaire>
    {
        public void Configure(EntityTypeBuilder<FinancialQuestionnaire> builder)
        {
            builder.Property(b => b._Entries).HasColumnName("Entries");
        }
    }
}
