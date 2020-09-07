using System.Runtime.Serialization;

namespace PriceCalculation.Models.Lifecycle
{
    public enum CalendarBasisKind
    {
        [EnumMember(Value = "30a-360")]
        Calendar30A360,

        [EnumMember(Value = "30u-360")]
        Calendar30U360,

        [EnumMember(Value = "30e-360")]
        Calendar30E360,

        [EnumMember(Value = "30e-360-isda")]
        Calendar30E360ISDA,

        [EnumMember(Value = "act-act-icma")]
        CalendarActActICMA,

        [EnumMember(Value = "act-act-isda")]
        CalendarActActISDA,

        [EnumMember(Value = "act-365-fixed")]
        CalendarAct365Fixed,

        [EnumMember(Value = "act-360")]
        CalendarAct360,

        [EnumMember(Value = "act-364")]
        CalendarAct364,

        [EnumMember(Value = "act-365-l")]
        CalendarAct365L,

        [EnumMember(Value = "act-act-afb")]
        CalendarActActAFB,

        [EnumMember(Value = "30e-365")]
        Calendar30E365,

        [EnumMember(Value = "30u-365")]
        Calendar30U365,

        [EnumMember(Value = "30e-actual")]
        Calendar30EActual,

        [EnumMember(Value = "30u-actual")]
        Calendar30UActual

    }
}
