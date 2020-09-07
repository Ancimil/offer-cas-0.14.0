using System.Runtime.Serialization;

namespace PriceCalculation.Models.Lifecycle
{
    public enum RelativeDateKind
    {
        [EnumMember(Value = "on-lifecycle-event")]
        OnLifecycleEvent,

        [EnumMember(Value = "on-specific-day-of-month")]
        OnSpecificDayOfMonth,

        [EnumMember(Value = "on-specific-day-of-month-after-lifecycle-event")]
        OnSpecificDayOfMonthAfterLifecycleEvent,

        [EnumMember(Value = "on-last-day-of-month")]
        OnLastDayOfMonth,

        [EnumMember(Value = "on-last-day-of-month-after-lifecycle-event")]
        OnLastDayOfMonthAfterLifecycleEvent
    }

    public class RelativeDate
    {
        public RelativeDateKind Kind { get; set; }
        public LifecycleEvent BaseEvent { get; set; }
        public int OffsetDays { get; set; }
        public int DayOfMonth { get; set; }
    }
}
