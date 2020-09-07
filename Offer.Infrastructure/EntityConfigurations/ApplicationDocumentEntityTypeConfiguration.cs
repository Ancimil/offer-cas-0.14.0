using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Offer.Domain.AggregatesModel.ApplicationAggregate;

namespace Offer.Infrastructure.EntityConfigurations
{
    public class ApplicationDocumentEntityTypeConfiguration : IEntityTypeConfiguration<ApplicationDocument>
    {
        public void Configure(EntityTypeBuilder<ApplicationDocument> builder)
        {
            builder.ToTable("application_documents");

            builder.HasKey(a => new { a.DocumentId });

            //builder.HasMany(b => b.Attachments).WithOne().HasForeignKey(p => p.DocumentId);
        }
    }
}
