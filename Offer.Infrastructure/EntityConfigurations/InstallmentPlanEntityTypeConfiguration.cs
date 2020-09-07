using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Offer.Domain.AggregatesModel.ApplicationAggregate;

namespace Offer.Infrastructure.EntityConfigurations
{
    class InstallmentPlanEntityTypeConfiguration : IEntityTypeConfiguration<InstallmentPlan>
    {
        public void Configure(EntityTypeBuilder<InstallmentPlan> builder)
        {
            builder.ToTable("installment_plans");

            builder.HasKey(a => new { a.ArrangementRequestId, a.InstallmentPlanId });
            builder.HasMany(b => b.InstallmentPlanRows).WithOne().HasForeignKey(p => new { p.ArrangementRequestId, p.InstallmentPlanId });

        }
    }
}
