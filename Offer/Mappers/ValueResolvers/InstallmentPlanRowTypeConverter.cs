using AutoMapper;
using Offer.Domain.AggregatesModel.ApplicationAggregate;
using System;

namespace Offer.API.Mappers.ValueResolvers
{
    public class InstallmentPlanRowTypeConverter : ITypeConverter<string, InstallmentPlanRow>
    {
        public InstallmentPlanRow Convert(string source, InstallmentPlanRow destination, ResolutionContext context)
        {
            throw new NotImplementedException();
        }
    }
}
