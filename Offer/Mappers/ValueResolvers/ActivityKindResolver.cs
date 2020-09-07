using AutoMapper;
using Offer.Domain.AggregatesModel.ApplicationAggregate;
using System;

namespace Offer.API.Mappers.ValueResolvers
{
    public class ActivityKindResolver : IValueResolver<string, ActivityKind, ActivityKind>
    {
        public ActivityKind Resolve(string source, ActivityKind destination, ActivityKind destMember, ResolutionContext context)
        {
            if (Enum.TryParse<ActivityKind>(source, true, out ActivityKind parsed))
            {
                return parsed;
            }
            return ActivityKind.Repayment;
        }
    }
}
