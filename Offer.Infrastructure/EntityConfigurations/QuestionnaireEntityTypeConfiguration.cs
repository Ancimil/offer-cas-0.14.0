using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Offer.Domain.AggregatesModel.ApplicationAggregate;

namespace Offer.Infrastructure.EntityConfigurations
{
    class QuestionnaireEntityTypeConfiguration : IEntityTypeConfiguration<Questionnaire>
    {
        public void Configure(EntityTypeBuilder<Questionnaire> builder)
        {
            builder.ToTable("questionnaires");
            builder.HasKey(a => new { a.ApplicationId, a.QuestionnaireId});
            builder.Property("Discriminator").HasMaxLength(256);
        }
    }
}
