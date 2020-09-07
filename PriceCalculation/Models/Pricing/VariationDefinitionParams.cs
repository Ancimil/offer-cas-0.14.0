using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace PriceCalculation.Models.Pricing
{
    public class VariationDefinitionParams
    {
        [JsonProperty("channel")]
        public string Channel { get; set; }
        [JsonProperty("amount")]
        public decimal? Amount { get; set; }
        [JsonProperty("term")]
        public int? Term { get; set; }
        [JsonProperty("currency")]
        public string Currency { get; set; }
        [JsonProperty("riskScore")]
        public decimal? RiskScore { get; set; }
        [JsonProperty("debtToIncome")]
        public decimal? DebtToIncome { get; set; }
        [JsonProperty("creditRating")]
        public string CreditRating { get; set; }
        [JsonProperty("customerValue")]
        public decimal? CustomerValue { get; set; }
        [JsonProperty("customerSegment")]
        public string CustomerSegment { get; set; }
        [JsonProperty("collateralModel")]
        public string CollateralModel { get; set; }
        [JsonProperty("partOfBundle")]
        public string PartOfBundle { get; set; }
        [JsonProperty("amountInRuleCurrency")]
        public decimal? AmountInRuleCurrency { get; set; }
        [JsonProperty("hasCampaignIncluded")]
        public bool HasCampaignInculded { get; set; } = false;
        [JsonExtensionData]
        public IDictionary<string, JToken> AdditionalProperties { get; set; }
    }
}
