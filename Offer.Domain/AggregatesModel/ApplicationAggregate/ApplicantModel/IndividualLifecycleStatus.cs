using System.Runtime.Serialization;
using MicroserviceCommon.Contracts;
using System.ComponentModel;

namespace Offer.Domain.AggregatesModel.ApplicationAggregate
{
    [Enumeration("individual-lifecycle-status", "Individual Lifecycle Status", "Individual Lifecycle Status")]
    public enum IndividualLifecycleStatus
    {
        [EnumMember(Value = "living")]
        [Description("Living")]
        Living,

        [EnumMember(Value = "deceased")]
        [Description("Deceased")]
        Deceased,

        [EnumMember(Value = "missing")]
        [Description("Missing")]
        Missing,

        [EnumMember(Value = "unknown")]
        [Description("Unknown")]
        Unknown
    }
}
