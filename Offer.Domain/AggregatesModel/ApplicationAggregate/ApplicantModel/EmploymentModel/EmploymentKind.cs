using System.Runtime.Serialization;
using System.ComponentModel;
using MicroserviceCommon.Contracts;

namespace Offer.Domain.AggregatesModel.ApplicationAggregate
{
    [Enumeration("employment-kind", "Employment Kind", "Employment Kind")]
    public enum EmploymentKind
    {
        [EnumMember(Value = "permanent")]
        [Description("Permanent")]
        Permanent,

        [EnumMember(Value = "temporary")]
        [Description("Temporary")]
        Temporary
    }
}
