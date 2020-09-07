using MicroserviceCommon.Models;
using PriceCalculation.Models.Lifecycle;
using PriceCalculation.Models.Pricing;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Offer.Domain.View.AllDataViews
{
    public class ConditionsAllDataView
    {
        public List<FeeConditionAllDataView> Fees { get; set; }
        public List<InterestRateConditionAllDataView> InterestRates { get; set; }
        public InterestRateConditionAllDataView MainInterestRate
        {
            get
            {
                return InterestRates.Where(r => r.IsMain).FirstOrDefault();
            }
        }
        public List<GeneralCondition> Other { get; set; }
        public int FeeCount
        {
            get
            {
                return Fees?.Count ?? 0;
            }
        }
        public int InterestRateCount
        {
            get
            {
                return InterestRates?.Count ?? 0;
            }
        }
        public int OtherConditionsCount
        {
            get
            {
                return Other?.Count ?? 0;
            }
        }
    }

    public abstract class PricingConditionAllDataView
    {
        public string Title { get; set; }
        public DateTime EffectiveDate { get; set; }
        public List<string> Currencies { get; set; }
        public string VariationsDefinition { get; set; }
        public string PricingRuleCurrency { get; set; }
    }

    public class FeeConditionAllDataView : PricingConditionAllDataView
    {
        public decimal CalculatedFixedAmount { get; set; }
        public decimal CalculatedLowerLimit { get; set; }
        public decimal CalculatedPercentage { get; set; }
        public decimal CalculatedUpperLimit { get; set; }
        public Currency FixedAmount { get; set; }
        public decimal FixedAmountInProductCurrency { get; set; }
        public FeeConditionFrequency Frequency { get; set; }
        public FeeConditionKind Kind { get; set; }
        public Currency LowerLimit { get; set; }
        public decimal Percentage { get; set; }
        public decimal? PercentageLowerLimit { get; set; }
        public bool? PercentageLowerLimitApplied { get; set; }
        public decimal? PercentageUpperLimit { get; set; }
        public bool? PercentageUpperLimitApplied { get; set; }
        public string ServiceCode { get; set; }
        public string ServiceDescription { get; set; }
        public Currency UpperLimit { get; set; }
        public List<FeeVariation> Variations { get; set; }
        public RelativeDate FirstFeeDate { get; set; }
    }

    public class InterestRateConditionAllDataView : PricingConditionAllDataView
    {
        public decimal? CalculatedLowerLimit { get; set; }
        public decimal CalculatedRate { get; set; }
        public decimal? CalculatedUpperLimit { get; set; }
        public CalendarBasisKind CalendarBasis { get; set; }
        public bool IsCompound { get; set; }
        public bool IsFixed { get; set; }
        public InterestRateKinds Kind { get; set; }
        public bool? LowerLimitApplied { get; set; }
        public string LowerLimitVariationsDefinition { get; set; }
        public InterestRate Rate { get; set; }
        public decimal? RateWithoutBundle { get; set; }
        public bool? UpperLimitApplied { get; set; }
        public string UpperLimitVariationsDefinition { get; set; }
        public List<InterestRateVariation> UpperLimitVariations { get; set; }
        public List<InterestRateVariation> LowerLimitVariations { get; set; }
        public InterestRate LowerLimit { get; set; }
        public InterestRate UpperLimit { get; set; }
        public List<InterestRateVariation> Variations { get; set; }
        public bool IsMain { get; set; } = false;
    }
}
