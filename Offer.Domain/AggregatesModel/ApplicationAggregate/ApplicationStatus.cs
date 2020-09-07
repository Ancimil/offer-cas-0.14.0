using System.Runtime.Serialization;
using System.ComponentModel;
using MicroserviceCommon.Contracts;

namespace Offer.Domain.AggregatesModel.ApplicationAggregate
{
    [Enumeration("application-status", "Application Status", "Application Status description")]
    public enum ApplicationStatus
    {
        [EnumMember(Value = "draft")]
        [Description("Application is being prepareds")]
        Draft,

        [EnumMember(Value = "canceled")]
        [Description("Customer canceled at any point")]
        Canceled,


        [EnumMember(Value = "active")]
        [Description("In underwriting process")]
        Active,

        [EnumMember(Value = "rejected")]
        [Description("Negative underwriting decision")]
        Rejected,

        [EnumMember(Value = "approved")]
        [Description("Positive underwriting decision")]
        Approved,

        [EnumMember(Value = "accepted")]
        [Description("Customer accepted offer")]
        Accepted,

        [EnumMember(Value = "complete")]
        [Description("Arrangement(s) activated")]
        Complete,

        [EnumMember(Value = "offered")]
        [Description("Arrangement(s) offered")]
        Offered,

        [EnumMember(Value = "expired")]
        [Description("Arrangement(s) expired")]
        Expired,

        [EnumMember(Value = "proposed")]
        [Description("Arrangement(s) proposed")]
        Proposed
    }
}
