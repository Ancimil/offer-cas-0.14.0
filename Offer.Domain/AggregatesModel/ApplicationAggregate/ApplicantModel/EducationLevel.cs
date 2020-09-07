using System.Runtime.Serialization;

namespace Offer.Domain.AggregatesModel.ApplicationAggregate
{
    public enum EducationLevel
    {

        [EnumMember(Value = "no-formal-education")]
        NoFormalEducation,


        [EnumMember(Value = "primary")]
        Primary,


        [EnumMember(Value = "lower-secondary")]
        LowerSecondary,


        [EnumMember(Value = "upper-secondary")]
        UpperSecondary,


        [EnumMember(Value = "bachelor-degree")]
        BachelorDegree,


        [EnumMember(Value = "master-degree")]
        MasterDegree,


        [EnumMember(Value = "doctorate")]
        Doctorate,


        [EnumMember(Value = "bachelor")]
        Bachelor,


        [EnumMember(Value = "not-disclosed")]
        NotDisclosed,
    }
}
