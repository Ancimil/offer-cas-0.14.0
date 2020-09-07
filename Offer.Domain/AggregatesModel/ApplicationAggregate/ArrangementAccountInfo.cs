using MicroserviceCommon.Contracts;
using System.ComponentModel;
using System.Runtime.Serialization;


namespace Offer.Domain.AggregatesModel.ApplicationAggregate
{
    [Enumeration("arrangement-account-role-kind", "Arrangement Account Role Kinds", "Arrangement Account Role Kinds")]
    public enum ArrangementAccountRoleKind
    {
        [EnumMember(Value = "primary-account")]
        [Description("Primary Account")]
        PrimaryAccount,

        [EnumMember(Value = "settlement-account")]
        [Description("Settlement Account")]
        SettlementAccount,

        [EnumMember(Value = "sub-account")]
        [Description("Sub Account")]
        SubAccount,

        [EnumMember(Value = "insurance-account")]
        [Description("Insurance account")]
        InsuranceAccount,

        [EnumMember(Value = "commission-account")]
        [Description("Commission account")]
        CommissionAccount,

        [EnumMember(Value = "subvention-account")]
        [Description("Subvention account")]
        SubventionAccount,

        [EnumMember(Value = "beneficiary-account")]
        [Description("Beneficiary account")]
        BeneficiaryAccount,

        [EnumMember(Value = "other")]
        [Description("Other")]
        Other
    }

    public class ArrangementAccountInfo
    {
        public string AccountNumber { get; set; }
        public ArrangementAccountRoleKind RoleKind { get; set; }

    }
}
