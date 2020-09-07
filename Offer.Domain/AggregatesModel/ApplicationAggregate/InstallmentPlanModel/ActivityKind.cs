using System.Runtime.Serialization;

namespace Offer.Domain.AggregatesModel.ApplicationAggregate
{
    public enum ActivityKind
    {
        // >>>>>>>>>>>>>>>>>>ne poklapaju se
        [EnumMember(Value = "disbursement")]
        Disbursement,

        [EnumMember(Value = "downpayment")]
        Downpayment,

        [EnumMember(Value = "repayment")]
        Repayment,

        [EnumMember(Value = "interest-payment")]
        InterestPayment,

        [EnumMember(Value = "fee-payment")]
        FeePayment,

        [EnumMember(Value = "cash-collateral-pledging")]
        CashCollateralPledging,

        [EnumMember(Value = "cash-collateral-release")]
        CashCollateralRelease,
    }
}
