using Microsoft.EntityFrameworkCore;
using Offer.Domain.AggregatesModel.ApplicationAggregate;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Offer.Infrastructure.EntityConfigurations
{
    public class ArrangementRequestEntityTypeConfiguration : IEntityTypeConfiguration<ArrangementRequest>
    {
        public void Configure(EntityTypeBuilder<ArrangementRequest> builder)
        {
            builder.ToTable("arrangement_requests");

            builder.HasKey(a => new { a.ApplicationId, a.ArrangementRequestId });
            builder.Property(b => b._Accounts).HasColumnName("Accounts");
            builder.Property(b => b._InstallmentPlan).HasColumnName("InstallmentPlan");
            builder.Property(b => b._ProductSnapshot).HasColumnName("ProductSnapshot");
            builder.Property(b => b._ProductSnapshotHash).HasColumnName("ProductSnapshotHash");
            builder.Property(b => b._BundleInfo).HasColumnName("BundleInfo");
            builder.Property(b => b._Extended).HasColumnName("Extended");
            builder.Property(b => b._Conditions).HasColumnName("ArrangementRequestCondition");
            builder.HasDiscriminator<string>("Discriminator")
              .HasValue<FinanceServiceArrangementRequest>(nameof(FinanceServiceArrangementRequest))
              .HasValue<ElectronicAccessArrangementRequest>(nameof(ElectronicAccessArrangementRequest))
              .HasValue<OtherProductArrangementRequest>(nameof(OtherProductArrangementRequest))
              .HasValue<SecuritiesArrangementRequest>(nameof(SecuritiesArrangementRequest))
              .HasValue<DepositRequest>(nameof(DepositRequest))
              .HasValue<CurrentAccountRequest>(nameof(CurrentAccountRequest))
              .HasValue<DemandDepositRequest>(nameof(DemandDepositRequest))
              .HasValue<TermDepositRequest>(nameof(TermDepositRequest))
              .HasValue<CreditCardFacilityRequest>(nameof(CreditCardFacilityRequest))
              .HasValue<CreditFacilityRequest>(nameof(CreditFacilityRequest))
              .HasValue<OverdraftFacilityRequest>(nameof(OverdraftFacilityRequest))
              .HasValue<TermLoanRequest>(nameof(TermLoanRequest))
              .HasValue<CreditLineRequest>(nameof(CreditLineRequest));
            builder.Property("Discriminator").HasMaxLength(256);
            builder.Property(r => r.Enabled).HasDefaultValue(true);
            builder.Property(r => r.IsOptional).HasDefaultValue(true);
            builder.HasOne(c => c.ProductSnapshotDb)
            .WithMany(e => e.ArrangementRequests)
            .HasForeignKey(d => d._ProductSnapshotHash);

        }
    }
}
