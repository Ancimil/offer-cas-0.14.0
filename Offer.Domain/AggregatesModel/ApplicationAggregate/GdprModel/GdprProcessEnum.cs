using System.Runtime.Serialization;

namespace Offer.Domain.AggregatesModel.ApplicationAggregate
{
    public enum GdprProcessEnum
    {
        [EnumMember(Value = "export")]
        Export,

        [EnumMember(Value = "anonymize")]
        Anonymize,

        [EnumMember(Value = "correction")]
        Correction
    }
}
