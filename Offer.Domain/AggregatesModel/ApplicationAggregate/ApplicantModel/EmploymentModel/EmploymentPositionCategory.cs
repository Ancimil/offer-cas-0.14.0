using System.Runtime.Serialization;
using System.ComponentModel;
using MicroserviceCommon.Contracts;

namespace Offer.Domain.AggregatesModel.ApplicationAggregate
{
    [Enumeration("employment-position-category", "Employment Position Category", "Employment Position Category")]
    public enum EmploymentPositionCategory
    {
        [EnumMember(Value = "worker")]
        [Description("Worker")]
        Worker,

        [EnumMember(Value = "farmer")]
        [Description("Farmer")]
        Farmer,

        [EnumMember(Value = "low-manager")]
        [Description("Low Manager")]
        LowManager,

        [EnumMember(Value = "middle-manager")]
        [Description("Middle Manager")]
        MiddleManager,

        [EnumMember(Value = "high-manager")]
        [Description("High Manager")]
        HighManager,

        [EnumMember(Value = "executive-manager")]
        [Description("Executive Manager")]
        ExecutiveManager,

        [EnumMember(Value = "office-employee")]
        [Description("Office Employee")]
        OfficeEmployee,

        [EnumMember(Value = "government-employee")]
        [Description("Government Employee")]
        GovernmentEmployee,

        [EnumMember(Value = "teaching-employee")]
        [Description("Teaching Employee")]
        TeachingEmployee,

        [EnumMember(Value = "medical-employee")]
        [Description("Medical Employee")]
        MedicalEmployee
    }
}
