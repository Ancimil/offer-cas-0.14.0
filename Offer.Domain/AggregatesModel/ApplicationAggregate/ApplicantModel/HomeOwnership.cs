using System.Runtime.Serialization;

namespace Offer.Domain.AggregatesModel.ApplicationAggregate
{
    public enum HomeOwnership
    {
        [EnumMember(Value = "owns")]
        Owns,

        [EnumMember(Value = "rents")]
        Rents,

        [EnumMember(Value = "lives-with-relatives")]
        LivesWithRelatives,

        [EnumMember(Value = "not-disclosed")]
        NotDisclosed,
    }
}
