using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using MicroserviceCommon.Contracts;
using System.ComponentModel;
using PriceCalculation.Models.Lifecycle;

namespace PriceCalculation.Models.Pricing
{
    [Enumeration("interest-rate-kind", "Interest Rate Kind", "Interest Rate Kind")]
    public enum InterestRateKinds
    {
        [EnumMember(Value = "regular-interest")]
        [Description("Regular Interest")]
        RegularInterest,

        [EnumMember(Value = "penalty-interest")]
        [Description("Penalty Interest")]
        PenaltyInterest,

        [EnumMember(Value = "early-withdrawal-interest")]
        [Description("Early Witdrawal Interest")]
        EarlyWithdrawalInterest
    }


    public class InterestRateCondition : PricingCondition
    {
        public decimal? CalculatedLowerLimit { get; set; }
        public decimal CalculatedRate { get; set; }
        public decimal? CalculatedUpperLimit { get; set; }
        public CalendarBasisKind CalendarBasis { get; set; }
        public bool IsCompound { get; set; }
        public bool IsFixed { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public InterestRateKinds Kind { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? LowerLimitApplied { get; set; }
        [MaxLength(1024)]
        public string LowerLimitVariationsDefinition { get; set; }
        public string LowerLimitVariationsDefinitionDMN { get; set; }
        public InterestRate Rate { get; set; }
        public decimal? RateWithoutBundle { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? UpperLimitApplied { get; set; }
        [MaxLength(1024)]
        public string UpperLimitVariationsDefinition { get; set; }
        public string UpperLimitVariationsDefinitionDMN { get; set; }
        public List<InterestRateVariation> UpperLimitVariations { get; set; }
        public List<InterestRateVariation> LowerLimitVariations { get; set; }
        public InterestRate LowerLimit { get; set; }
        public InterestRate UpperLimit { get; set; }
        public List<InterestRateVariation> Variations { get; set; }

        public override bool Equals(object obj)
        {
            var condition = obj as InterestRateCondition;
            return CorrespondsTo(condition) &&
                   CalculatedRate == condition.CalculatedRate;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public bool CorrespondsTo(InterestRateCondition condition)
        {
            return condition != null &&
                   Kind == condition.Kind &&
                   Currencies.Count() == condition.Currencies.Count() &&
                   !Currencies.Except(condition.Currencies).Any() &&
                   Periods == condition.Periods;
        }

        public void AppendVariations(InterestRateCondition condition)
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

            if (UpperLimitVariations != null && condition.UpperLimitVariations != null)
            {
                UpperLimitVariations.AddRange(condition.UpperLimitVariations);
            }
            else if (condition.UpperLimitVariations != null)
            {
                UpperLimitVariations = condition.UpperLimitVariations.ToList();
            }

            if (LowerLimitVariations != null && condition.LowerLimitVariations != null)
            {
                LowerLimitVariations.AddRange(condition.LowerLimitVariations);
            }
            else if (condition.LowerLimitVariations != null)
            {
                LowerLimitVariations = condition.LowerLimitVariations.ToList();
            }
        }
    }

    public class InterestRateVariation : PriceVariation, IEquatable<InterestRateVariation>
    {
        public bool Equals(InterestRateVariation variation)
        {
            return variation.Origin == Origin &&
                variation.VariationGroup == VariationGroup &&
                variation.BenefitId == BenefitId &&
                variation.BenefitSourceId == BenefitSourceId &&
                variation.Percentage == Percentage;
        }

        public override bool Equals(object obj) => Equals(obj as InterestRateVariation);
        public override int GetHashCode() => (VariationGroup, Origin, BenefitId, BenefitSourceId, Percentage).GetHashCode();
    }
}
