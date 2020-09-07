using System.Runtime.Serialization;

namespace Offer.Domain.AggregatesModel.ApplicationAggregate
{
    public enum Gender
    {
        [EnumMember(Value = "female")]
        Female,
        
        [EnumMember(Value = "male")]
        Male,

        [EnumMember(Value = "unknown")]
        Unknown,
    }
}
