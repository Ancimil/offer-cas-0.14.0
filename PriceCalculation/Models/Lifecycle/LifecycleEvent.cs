using System.Runtime.Serialization;

namespace PriceCalculation.Models.Lifecycle
{
    public enum LifecycleEvent
    {
        [EnumMember(Value = "request-date")]
        RequestDate,
        [EnumMember(Value = "contract-signed")]
        ContractSigned,
        [EnumMember(Value = "approved")]
        Approved,
        [EnumMember(Value = "funds-disbursed")]
        FundsDisbursed,
        [EnumMember(Value = "first-installment-date")]
        FirstInstallmentDate,
        [EnumMember(Value = "maturity-date")]
        MaturityDate
    }
}
