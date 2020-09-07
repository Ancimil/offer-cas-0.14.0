using MicroserviceCommon.Contracts;
using PriceCalculation.Models.Pricing;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace Offer.Domain.AggregatesModel.ApplicationAggregate
{
    public class TermDepositRequest : DepositRequest
    {
        public TermDepositRequest() : base()
        {
            SavingsPlan = new SavingsPlan();
        }
        public decimal Amount { get; set; }
        public DateTime? MaturityDate { get; set; }
        public string Term { get; set; }

        public RolloverOptions RolloverOption { get; set; }
        public Int32 MaxRollovers { get; set; }
        public SavingsPlan SavingsPlan { get; set; }
        [Column("InterestCapOnRollover")]
        public RolloverOptions InterestCapitalizationOnRollover { get; set; }

        public override void MergePriceCalculationResults(PriceCalculationResult result)
        {
            base.MergePriceCalculationResults(result);
            Napr = result.Napr ?? 0;
        }
    }

    [Enumeration("rollover-options", "Rollover Options", "Rollover Options")]
    public enum RolloverOptions
    {
        [EnumMember(Value = "not-allowed")]
        [Description("Not Allowed")]
        NotAllowed,

        [EnumMember(Value = "allowed-on-request")]
        [Description("Allowed on Request")]
        AllowedOnRequest,

        [EnumMember(Value = "automatic")]
        [Description("Automatic")]
        Automatic
    }
}
