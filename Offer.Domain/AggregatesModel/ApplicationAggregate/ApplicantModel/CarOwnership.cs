using System.Runtime.Serialization;

namespace Offer.Domain.AggregatesModel.ApplicationAggregate
{
    public enum CarOwnership
    {
        [EnumMember(Value = "owns")]
        Owns,

        [EnumMember(Value = "does-not-own")]
        DoesNotOwn,

        [EnumMember(Value = "not-disclosed")]
        NotDisclosed
    }
}
