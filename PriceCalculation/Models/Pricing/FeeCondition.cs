using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using MicroserviceCommon.Models;
using PriceCalculation.Models.Lifecycle;
using Enumeration = MicroserviceCommon.Contracts.Enumeration;
using System.ComponentModel;

namespace PriceCalculation.Models.Pricing
{
    [Enumeration("fee-condition-kind", "Fee Condition Kind", "Fee Condition Kind")]
    public enum FeeConditionKind
    {
        [EnumMember(Value = "origination-fee")]
        [Description("Origination Fee")]
        OriginationFee,

        [EnumMember(Value = "management-fee")]
        [Description("Management Fee")]
        ManagementFee,

        [EnumMember(Value = "early-repayment-fee")]
        [Description("Early Repayment Fee")]
        EarlyRepaymentFee,

        [EnumMember(Value = "early-withdrawal-fee")]
        [Description("Early Withdrawal Fee")]
        EarlyWithdrawalFee,

        [EnumMember(Value = "service-fee")]
        [Description("Service Fee")]
        ServiceFee,

        [EnumMember(Value = "expense")]
        [Description("Expense")]
        Expense,

        [EnumMember(Value = "other")]
        [Description("Other")]
        Other
    }

    [Enumeration("fee-condition-frequency", "Fee Condition Frequency", "Fee Condition Frequency")]
    public enum FeeConditionFrequency
    {

        [EnumMember(Value = "event-triggered")]
        [Description("Event Triggered")]
        EventTriggered,

        [EnumMember(Value = "monthly")]
        [Description("Monthly")]
        Monthly,

        [EnumMember(Value = "quarterly")]
        [Description("Quarterly")]
        Quarterly,

        [EnumMember(Value = "semiyearly")]
        [Description("Semiyearly")]
        Semiyearly,

        [EnumMember(Value = "yearly")]
        [Description("Yearly")]
        Yearly
    }

    [Enumeration("fee-clalculation-basis", "Fee Calculation Basis", "Fee Calculation Basis")]
    public enum FeeCalculationBasis
    {
        // In case that Percentage is 0
        [EnumMember(Value = "none")]
        [Description("None")]
        None,

        // In case of loan, this is amount to be disbursed / investment amount
        [EnumMember(Value = "transaction-amount")]
        [Description("Transaction Amount")]
        TransactionAmount,

        [EnumMember(Value = "limit")]
        [Description("Limit")]
        Limit,

        [EnumMember(Value = "unutilized-limit")]
        [Description("Unutilized Limit")]
        UnutilizedLimit,

        [EnumMember(Value = "outstanding")]
        [Description("Outstanding")]
        Outstanding
    }

    [Enumeration("fee-calculation-order", "Fee Calculation Orded", "Fee Calculation Orded")]
    public enum FeeCalculationOrder
    {
        [EnumMember(Value = "prior-installment")]
        [Description("Prior Installment")]
        PriorInstallment,

        [EnumMember(Value = "after-installment")]
        [Description("After Installment")]
        AfterInstallment
    }

    public class FeeCondition : PricingCondition
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public FeeConditionKind Kind { get; set; }
        [MaxLength(128)]
        public string ServiceCode { get; set; }
        public string ServiceDescription { get; set; }
        public decimal Percentage { get; set; }
        public Currency FixedAmount { get; set; }
        public decimal CalculatedPercentage { get; set; }
        public decimal CalculatedFixedAmount { get; set; }
        public decimal CalculatedLowerLimit { get; set; }
        public decimal CalculatedUpperLimit { get; set; }
        public Currency LowerLimit { get; set; }
        public Currency UpperLimit { get; set; }
        public decimal? PercentageLowerLimit { get; set; }
        public decimal? PercentageUpperLimit { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public FeeConditionFrequency Frequency{ get; set; }
        public RelativeDate FirstFeeDate { get; set; }
        public List<FeeVariation> Variations { get; set; }
        public decimal FixedAmountInCurrency { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? PercentageLowerLimitApplied { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? PercentageUpperLimitApplied { get; set; }

        public override bool Equals(object obj)
        {
            var condition = obj as FeeCondition;
            return CorrespondsTo(condition) &&
                   CalculatedPercentage == condition.CalculatedPercentage &&
                   CalculatedFixedAmount == condition.CalculatedFixedAmount &&
                   CalculatedLowerLimit == condition.CalculatedLowerLimit &&
                   CalculatedUpperLimit == condition.CalculatedUpperLimit;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public bool CorrespondsTo(FeeCondition condition)
        {
            return condition != null &&
                   Kind == condition.Kind &&
                   Frequency == condition.Frequency &&
                   condition.Currencies != Currencies &&
                   Currencies.Count() == condition.Currencies.Count() &&
                   !Currencies.Except(condition.Currencies).Any();
        }

        public void AppendVariations(FeeCondition condition)
        {
            if (condition == null)
            {
                return;
            }
            if (Variations != null && condition.Variations != null)
            {
                Variations.AddRange(condition.Variations);
            }
            else if (condition.Variations != null)
            {
                Variations = condition.Variations.ToList();
            }
        }
    }

    public class FeeVariation : PriceVariation
    {
        public decimal FixedAmount { get; set; }
        public decimal LowerLimit { get; set; }
        public decimal UpperLimit { get; set; }
    }
}
