using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Offer.Domain.AggregatesModel.ApplicationAggregate;

namespace Offer.Infrastructure.EntityConfigurations
{
    public class PartyEntityTypeConfiguration : IEntityTypeConfiguration<Party>
    {
        public void Configure(EntityTypeBuilder<Party> builder)
        {
            builder.ToTable("parties");
            builder.HasKey(a => new { a.ApplicationId, a.PartyId });
            builder.Property(b => b._ProductUsageInfo).HasColumnName("ProductUsageInfo");
            builder.Property(b => b._Extended).HasColumnName("Extended");
            builder.OwnsOne(b => b.ContactAddress, cb =>
            {
                cb.OwnsOne(c => c.Coordinates,
                    co =>
                    {
                        co.Property(p => p.Lat).HasColumnName("ContactAddr_Coordinates_Lat");
                        co.Property(p => p.Long).HasColumnName("ContactAddr_Coordinates_Long");
                    });
                cb.Property(p => p.Kind).HasColumnName("ContactAddr_Kind");
                cb.Property(p => p.Formatted).HasColumnName("ContactAddr_Formatted");
                cb.Property(p => p.Street).HasColumnName("ContactAddr_Street");
                cb.Property(p => p.StreetNumber).HasColumnName("ContactAddr_StreetNumber");
                cb.Property(p => p.PostalCode).HasColumnName("ContactAddr_PostalCode");
                cb.Property(p => p.Locality).HasColumnName("ContactAddr_Locality");
                cb.Property(p => p.Country).HasColumnName("ContactAddr_Country");
                cb.Property(p => p.AddressCode).HasColumnName("ContactAddr_AddressCode");
            });
            builder.OwnsOne(b => b.LegalAddress, cb =>
            {
                cb.OwnsOne(c => c.Coordinates,
                    co =>
                    {
                        co.Property(p => p.Lat).HasColumnName("LegalAddr_Coordinates_Lat");
                        co.Property(p => p.Long).HasColumnName("LegalAddr_Coordinates_Long");
                    });
                cb.Property(p => p.Kind).HasColumnName("LegalAddr_Kind");
                cb.Property(p => p.Formatted).HasColumnName("LegalAddr_Formatted");
                cb.Property(p => p.Street).HasColumnName("LegalAddr_Street");
                cb.Property(p => p.StreetNumber).HasColumnName("LegalAddr_StreetNumber");
                cb.Property(p => p.PostalCode).HasColumnName("LegalAddr_PostalCode");
                cb.Property(p => p.Locality).HasColumnName("LegalAddr_Locality");
                cb.Property(p => p.Country).HasColumnName("LegalAddr_Country");
                cb.Property(p => p.AddressCode).HasColumnName("LegalAddr_AddressCode");
            });
            builder.HasOne(p => p.Application).WithMany(x => x.InvolvedParties).HasForeignKey(p => p.ApplicationId);
        }
    }
}
