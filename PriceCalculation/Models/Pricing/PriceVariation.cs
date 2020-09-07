using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.ComponentModel.DataAnnotations;

namespace PriceCalculation.Models.Pricing
{
    public enum PriceVariationOrigins
    {
        [EnumMember(Value = "product")]
        Product,

        [EnumMember(Value = "product-options")]
        ProductOptions,

        [EnumMember(Value = "bundle")]
        Bundle,

        [EnumMember(Value = "campaign")]
        Campaign,

        [EnumMember(Value = "loyalty-scheme")]
        LoyaltyScheme,

        [EnumMember(Value = "collective-benefit")]
        CollectiveBenefit,

        [EnumMember(Value = "relationship-benefit")]
        RelationshipBenefit,

        [EnumMember(Value = "sales-discount")]
        SalesDiscount
    }
    public abstract class PriceVariation
    {
        [MaxLength(1024)]
        public string ApplicabilityExpression { get; set; }
        [MaxLength(256)]
        public string BenefitId { get; set; }
        [MaxLength(256)]
        public string BenefitSourceId { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public PriceVariationOrigins Origin { get; set; }
        public decimal Percentage { get; set; }
        [MaxLength(1024)]
        public string VariationDescription { get; set; }
        [MaxLength(1024)]
        public string VariationGroup { get; set; }
    }
}
