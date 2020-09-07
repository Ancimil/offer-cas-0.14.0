using Newtonsoft.Json;

namespace Offer.Domain.AggregatesModel.ApplicationAggregate
{
    public class DecisionRequest
    {
        [JsonProperty("data")]
        public object Data { get; set; }
        [JsonProperty("rule-definition")]
        public string RuleDefinition { get; set; }
        [JsonProperty("rule-definition-key")]
        public string RuleDefinitionKey { get; set; }
    }
}
