using Newtonsoft.Json.Linq;
using PriceCalculation.Models.Product;
using System;
using System.Collections.Generic;

namespace PriceCalculation.Models.Pricing
{
    public class PriceCalculationParameters
    {
        public DateTime? RequestDate { get; set; }
        public List<InterestRateCondition> InterestRates { get; set; }
        public List<FeeCondition> Fees { get; set; }
        public List<GeneralCondition> OtherConditions { get; set; }
        public List<ScheduledPeriod> ScheduledPeriods { get; set; }
        public LeadModel.LeadModel Campaign { get; set; }
        public string Currency { get; set; }
        public decimal? Amount { get; set; }
        public string Term { get; set; }
        public string Channel { get; set; }
        public decimal? RiskScore { get; set; }
        public string CustomerSegment { get; set; }
        public string CollateralModel { get; set; }
        public string PartOfBundle { get; set; }
        public List<ProductOption> Options { get; set; }
        public string CreditRating { get; set; }
        public decimal? CustomerValue { get; set; }
        public decimal? DebtToIncome { get; set; }
        public Dictionary<string, JToken> AdditionalProperties { get; set; }
    }

    public class ResolvedRatesResult
    {
        public List<InterestRateCondition> Rates { get; set; }
        public decimal? Napr { get; set; }
        public bool ResultChanged { get; set; }
    }

    public class ResolvedFeesResult
    {
        public List<FeeCondition> Fees { get; set; }
        public bool ResultChanged { get; set; }
    }
}
