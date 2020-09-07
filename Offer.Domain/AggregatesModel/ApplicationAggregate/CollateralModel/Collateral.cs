using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace Offer.Domain.AggregatesModel.ApplicationAggregate
{
    public class CollateralRequirement
    {
        public long CollateralRequirementId { get; set; }

        //[JsonIgnore]
        //public Application Application { get; set; }

        [JsonIgnore]
        [Required]
        public long ApplicationId { get; set; } // references parent app number, part of key

        [NotMapped]
        public string ApplicationNumber
        {
            get
            {
                var result = "0000000000" + ApplicationId;
                return result.Substring(result.Length - 10);
            }
            set
            {
                ApplicationId = long.Parse(value);
            }
        }
        [JsonIgnore]
        public FinanceServiceArrangementRequest ArrangementRequest { get; set; }
        [Required]
        public int ArrangementRequestId { get; set; }
        public string CollateralArrangementCode { get; set; }
        public bool FromModel { get; set; }
        public decimal MinimalCoverage { get; set; }
        public decimal MinimalCoverageInLoanCurrency { get; set; }
        public decimal ActualCoverage { get; set; }

        [JsonIgnore]
        [Column("SecuredDealLink")]
        public string _SecuredDealLink { get; set; }

        [NotMapped]
        public List<SecuredDealLink> SecuredDealLinks
        {
            get { return _SecuredDealLink == null ? null : JsonConvert.DeserializeObject<List<SecuredDealLink>>(_SecuredDealLink); }
            set
            {
                _SecuredDealLink = JsonConvert.SerializeObject(value);
            }
        }
        public string CollateralOwner { get; set; } // party id
    }

    public class SecuredDealLink
    {
        public string ApplicationNumber { get; set; }
        public int ArrangementRequestId { get; set; }
        public string ArrangementNumber { get; set; }
        public decimal PledgedValueInCollateralCurrency { get; set; }
        public decimal PledgedValueInLoanCurrency { get; set; }
    }

    public class CollateralCodeList
    {
        public List<CollateralCodeModel> Items { get; set; }
    }

    public class CollateralCodeModel
    {
        public string Literal { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }

    public class CollateralArrangementCodeModel : CollateralCodeModel
    {
        public CollateralCodeList CollateralCodes { get; set; }
    }

    public class CollateralArrangementCodeList
    {
        public List<CollateralArrangementCodeModel> Items { get; set; }
    }

    public class CollateralRequirementValidation
    {
        public long ApplicationNumber { get; set; }
        public int ArrangementRequestId { get; set; }
        public long CollateralRequirementId { get; set; }    
        public string CollateralArrangementCode { get; set; }
        public CollateralValidationResult ValidationResult { get; set; }
    }


    public enum CollateralValidationResult
    {      
        [EnumMember(Value = "filled")]
        Filled,

        [EnumMember(Value = "not-filled")]
        NotFilled,

        [EnumMember(Value = "below-minimal-coverage")]
        BelowMinimalCoverage,

    }
}
