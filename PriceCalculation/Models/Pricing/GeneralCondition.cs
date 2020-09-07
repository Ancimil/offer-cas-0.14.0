using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.ComponentModel.DataAnnotations;

namespace PriceCalculation.Models.Pricing
{
    public enum GeneralConditionCategories
    {
        [EnumMember(Value = "general")]
        General,

        [EnumMember(Value = "repayment")]
        Repayment,

        [EnumMember(Value = "withdraval")]
        Withdraval,

        [EnumMember(Value = "termination")]
        Termination,

        [EnumMember(Value = "collaterals")]
        Collaterals,

        [EnumMember(Value = "servicing")]
        Servicing,
        [EnumMember(Value = "dependencies")]
        Dependencies,

        [EnumMember(Value = "relationship")]
        Relationship
    }

    public class GeneralCondition
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public GeneralConditionCategories Category { get; set; }
        public string Description { get; set; }
        [MaxLength(256)]
        public string ContractSection { get; set; }
    }
}
