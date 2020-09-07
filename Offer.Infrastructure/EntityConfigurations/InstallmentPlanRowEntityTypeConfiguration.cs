using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Offer.Domain.AggregatesModel.ApplicationAggregate;

namespace Offer.Infrastructure.EntityConfigurations
{
    class InstallmentPlanRowEntityTypeConfiguration : IEntityTypeConfiguration<InstallmentPlanRow>
    {
        public void Configure(EntityTypeBuilder<InstallmentPlanRow> builder)
        {
            builder.ToTable("installment_plan_rows");

            builder.HasKey(a => new {a.ArrangementRequestId, a.InstallmentPlanId, a.Ordinal });

        }

    }
}
