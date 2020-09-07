using MicroserviceCommon.Contracts;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace Offer.Domain.AggregatesModel.ExposureModel
{
    [Enumeration("balance-kinds", "Balance Kinds")]
    public enum BalanceKinds
    {
        [EnumMember(Value = "available")]
        [Description("Available balance")]
        Available,
        [EnumMember(Value = "current")]
        [Description("Current balance")]
        Current,
        [EnumMember(Value = "expected")]
        [Description("Expected balance")]
        Expected,
        [EnumMember(Value = "blocked")]
        [Description("Blocked balance")]
        Blocked,
        [EnumMember(Value = "outstanding")]
        [Description("Outstanding balance")]
        Outstanding,
        [EnumMember(Value = "advance")]
        [Description("Advance balance")]
        Advance,
        [EnumMember(Value = "opening")]
        [Description("Opening balance")]
        Opening,
        [EnumMember(Value = "closing")]
        [Description("Closing balance")]
        Closing
    }

}
