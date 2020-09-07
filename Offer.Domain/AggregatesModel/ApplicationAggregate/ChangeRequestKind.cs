using MicroserviceCommon.Contracts;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace Offer.Domain.AggregatesModel.ApplicationAggregate
{
    [Enumeration("change-requests-app", "Change Requests Kind App", "Change Requests Kind App")]
    public enum ChangeRequestsKindApp
    {
        [EnumMember(Value = "unknown")]
        [Description("Unknown")]
        Unknown,
        [EnumMember(Value = "update")]
        [Description("Update")]
        Update,
        [EnumMember(Value = "update-completed")]
        [Description("Update Completed")]
        UpdateCompleted
    }
}
