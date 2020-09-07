using System.Runtime.Serialization;
using MicroserviceCommon.Contracts;
using System.ComponentModel;

namespace Offer.Domain.AggregatesModel.ApplicationAggregate
{
    [Enumeration("marital-status", "Marital Status", "Marital Status")]
    public enum MaritalStatus
    {
        [EnumMember(Value = "divorced")]
        [Description("Divorced")]
        Divorced,

        [EnumMember(Value = "common-law-marriage")]
        [Description("Common Law Marriage")]
        CommonLawMarriage,

        [EnumMember(Value = "married")]
        [Description("Married")]
        Married,

        [EnumMember(Value = "single")]
        [Description("Single")]
        Single,

        [EnumMember(Value = "widowed")]
        [Description("Widowed")]
        Widowed

    }
}
