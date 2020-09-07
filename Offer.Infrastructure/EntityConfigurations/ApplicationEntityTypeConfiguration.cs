using Microsoft.EntityFrameworkCore;
using Offer.Domain.AggregatesModel.ApplicationAggregate;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Offer.Infrastructure.EntityConfigurations
{
    public class ApplicationEntityTypeConfiguration : IEntityTypeConfiguration<Application>
    {
        public void Configure(EntityTypeBuilder<Application> builder)
        {
            builder.ToTable("applications");

            builder.HasKey(a => a.ApplicationId);
            builder.Property(a => a.ApplicationId)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn)
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn);
            builder.Property(p => p._Extended).HasColumnName("Extended");
            builder.Property(x => x._AvailableProducts).HasColumnName("AvailableProducts");
            // Parties
            builder.HasMany(b => b.InvolvedParties).WithOne().HasForeignKey(p => p.ApplicationId);

            //builder.HasMany(b => b.Documents).WithOne().HasForeignKey(p => p.ApplicationId);
            //builder.HasMany(b => b.ArrangementRequests).WithOne().HasForeignKey(p => p.ApplicationId);
            
            builder.HasMany(b => b.Questionnaires).WithOne().HasForeignKey(p => p.ApplicationId);

            builder.OwnsOne(b => b.StatusInformation);

            builder.Property(x => x.OrganizationUnitCode).HasMaxLength(64);

            builder.Property(p => p.OriginatesBundle).HasDefaultValue(false);

            builder.HasOne(x => x.OrganizationUnit).WithMany(x => x.Applications).HasForeignKey(x => x.OrganizationUnitCode);
        }
    }
}
