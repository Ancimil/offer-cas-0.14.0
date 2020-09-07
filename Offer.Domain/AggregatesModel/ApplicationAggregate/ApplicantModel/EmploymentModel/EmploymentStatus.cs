using System.Runtime.Serialization;
using System.ComponentModel;

namespace Offer.Domain.AggregatesModel.ApplicationAggregate
{
    [MicroserviceCommon.Contracts.Enumeration("employment-status", "Employment Status", "Enumeration that distinguishes between party kinds.")]
    public enum EmploymentStatus
    {
        [EnumMember(Value = "employed")]
        [Description("Employed")] 
        Employed,

        [EnumMember(Value = "not-employed")]
        [Description("Not Employed")]
        NotEmployed,

        [EnumMember(Value = "retired")]
        [Description("Retired")]
        Retired,

        [EnumMember(Value = "shop-owner")]
        [Description("Shop Owner")]
        ShopOwner,

        [EnumMember(Value = "farmer")]
        [Description("Farmer")]
        Farmer,

        [EnumMember(Value = "leave-of-absence")]
        [Description("Leave Of Absence")]
        LeaveOfAbsence,

        [EnumMember(Value = "part-time-job")]
        [Description("Part Time Job")]
        PartTimeJob

    }
}
