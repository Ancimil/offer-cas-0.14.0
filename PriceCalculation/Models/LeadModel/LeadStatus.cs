using MicroserviceCommon.Contracts;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace PriceCalculation.Models.LeadModel
{
    [Enumeration("lead-status", "Lead Status", "Lead Status")]
    public enum LeadStatus
    {
        [EnumMember(Value = "open")]
        [Description("Open")]
        Open,
        [EnumMember(Value = "work-in-progress")]
        [Description("Work In Progress")]
        WorkInProgress,
        [EnumMember(Value = "disqualified")]
        [Description("Disqualified")]
        Disqualified,
        [EnumMember(Value = "Qualified")]
        [Description("Qualified")]
        Qualified,
        [EnumMember(Value = "contacted")]
        [Description("Contacted")]
        Contacted,
        [EnumMember(Value = "canceled")]
        [Description("Canceled")]
        Canceled,
        [EnumMember(Value = "converted-to-opportunity")]
        [Description("Converted To Opportunity")]
        ConvertedToOpportunity,
        [EnumMember(Value = "converted-to-offer")]
        [Description("Converted To Offer")]
        ConvertedToOffer,
        [EnumMember(Value = "converted-to-contract")]
        [Description("Converted To Contract")]
        ConvertedToContract
    }
}