using Offer.Domain.AggregatesModel.ApplicationAggregate.ProductModel;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using MicroserviceCommon.Models;
using MicroserviceCommon.Models.Product;
using PriceCalculation.Models.Product;
using PriceCalculation.Models.LeadModel;
using PriceCalculation.Models.Lifecycle;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations.Schema;

namespace Offer.Domain.AggregatesModel.ApplicationAggregate
{
    public class ProductSnapshotDb
    {
        public string Hash { get; set; }
        [JsonIgnore]
        public string _ProductSnapshot { get; set; }

        [NotMapped]
        public ProductSnapshot ProductSnapshot
        {
            get { return _ProductSnapshot == null ? null : JsonConvert.DeserializeObject<ProductSnapshot>(_ProductSnapshot); }
            set
            {
                _ProductSnapshot = JsonConvert.SerializeObject(value);
            }
        }
        public ICollection<ArrangementRequest> ArrangementRequests { get; set; }
    }


    public class ProductSnapshot
    {
        public string ProductCode { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public ProductKinds Kind { get; set; }
        public ProductStatus Status { get; set; }
        public string MarketFeatures { get; set; }
        public string BenefitsInfo { get; set; }
        public string FamilyName { get; set; }
        public string PrimaryMarketSegmentName { get; set; }
        public bool IsPackage { get; set; }
        public string ImageUrl { get; set; }
        public string CoverImageUrl { get; set; }
        public string ApplicationProcessDescription { get; set; }
        public string CampaignCode { get; set; }
        public DateTime AvailabilityStart { get; set; }
        public string ProposalValidityPeriod { get; set; }
        public string OfferValidityPeriod { get; set; }
        public string PrimaryCurrency { get; set; }
        public List<string> AllowedCurrencies { get; set; }
        public Currency MaximalAmount { get; set; }
        public Currency MinimalAmount { get; set; }
        public bool IsRecommended { get; set; }
        public bool IsPreApproved { get; set; }
        public int InUse { get; set; }
        public bool IsStandalone { get; set; }
        public bool IsSingleton { get; set; }
        public int PreviouslyUsed { get; set; }
        public int ActiveRequests { get; set; }
        public string AccountTypeMapping { get; set; }
        public string SyncTimestamp { get; set; }
        public string TargetSegments { get; set; }
        public string TargetCustomerResidency { get; set; }
        public string TargetCustomerKind { get; set; }
        public string ChannelAvailability { get; set; }
        public string AvailableCollateralModels { get; set; }
        public string DefaultCollateralModel { get; set; }
        public string AvailableDiscountValues { get; set; }
        public bool SupportsAlternativeOffer { get; set; }
        public DefaultParameters DefaultParameters { get; set; }
        public List<SchedulingPeriod> Periods { get; set; }

        public List<OptionGroup> OptionGroups { get; set; }
        public string LoanPurposes { get; set; }
        public string Refinancing { get; set; }
        public string AvailableRevolvingPercentage { get; set; }
        public bool DisbursementInfoEntry { get; set; }
        public string Variants { get; set; }

        public List<BundledProductInfo> BundledProducts { get; set; }
        public string RelatedProducts { get; set; }
        public List<ProductDocumentation> RequiredDocumentation { get; set; }
        public List<ProductCollateralModel> AvailableCollateralModelsData { get; set; }

        public Dictionary<string, bool> BundleDefaults { get; set; }
        public string DocumentTemplateRules { get; set; }
        public string MinimalTerm { get; set; }
        public string MaximalTerm { get; set; }
        public decimal MinimalDownpaymentPercentage { get; set; }
        public decimal MinimalRepaymentPercentage { get; set; }
        public Currency MinimalRepaymentAmount { get; set; }
        
        public ProductConditions Conditions { get; set; }
        public LeadModel Campaign { get; set; }
        public bool IsRevolving { get; set; }
        public CreditLineProducts CreditLineProducts { get; set; }
        public int MinimumDaysForFirstInstallment { get; set; }
        public string DueDayOptions { get; set; }

    }

    public class ProductCollateralModel
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public List<ProductCollateralRequirement> CollateralRequirements { get; set; }
    }

    public class ProductCollateralRequirement
    {
        public string CollateralArrangementCode { get; set; }
        public decimal MinimalCoverage { get; set; }
        public int Quantity { get; set; }
        public string PartyRoles { get; set; }
    }


    public enum ProductStatus
    {
        [EnumMember(Value = "available")]
        Available,
        [EnumMember(Value = "no-longer-available")]
        NoLongerAvailable,
        [EnumMember(Value = "temporarly-unavailable")]
        TemporarlyUnavailable,
        [EnumMember(Value = "comming-soon")]
        CommingSoon
    }

    public enum ProductKinds
    {
        [EnumMember(Value = "current-account-product")]
        CurrentAccountProduct,
        [EnumMember(Value = "demand-deposit-product")]
        DemandDepositProduct,
        [EnumMember(Value = "term-deposit-product")]
        TermDepositProduct,
        [EnumMember(Value = "term-loan-product")]
        TermLoanProduct,
        [EnumMember(Value = "credit-facility-product")]
        CreditFacilityProduct,
        [EnumMember(Value = "credit-card-facility-product")]
        CreditCardFacilityProduct,
        [EnumMember(Value = "overdraft-facility-product")]
        OverdraftFacilityProduct,
        [EnumMember(Value = "credit-line-product")]
        CreditLineProduct,
        [EnumMember(Value = "card-access-product")]
        CardAccessProduct,
        [EnumMember(Value = "electronic-access-product")]
        ElectronicAccessProduct,
        [EnumMember(Value = "service")]
        Service,
        [EnumMember(Value = "abstract-product")]
        AbstractProduct,
    }
}
