using JsonSubTypes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Offer.Domain.AggregatesModel.ApplicationAggregate.AlternativeOfferModel;
using Offer.Domain.AggregatesModel.ApplicationAggregate.ProductModel;
using Offer.Domain.Calculations;
using Offer.Domain.Utils;
using PriceCalculation.Calculations;
using PriceCalculation.Models.LeadModel;
using PriceCalculation.Models.Pricing;
using PriceCalculation.Models.Product;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using static Offer.Domain.Calculations.InstallmentPlanCalculation;

namespace Offer.Domain.AggregatesModel.ApplicationAggregate
{
    [JsonConverter(typeof(JsonSubtypes), "arrangement-kind")]
    [JsonSubtypes.KnownSubType(typeof(CurrentAccountRequest), "current-account")]
    [JsonSubtypes.KnownSubType(typeof(DemandDepositRequest), "demand-deposit")]
    [JsonSubtypes.KnownSubType(typeof(TermDepositRequest), "term-deposit")]
    [JsonSubtypes.KnownSubType(typeof(ElectronicAccessArrangementRequest), "electronic-access-arrangement")]
    [JsonSubtypes.KnownSubType(typeof(CardAccessArrangementRequest), "card-access-arrangement")]
    [JsonSubtypes.KnownSubType(typeof(SecuritiesArrangementRequest), "securities-arrangement")]
    [JsonSubtypes.KnownSubType(typeof(OtherProductArrangementRequest), "other-product-arrangement")]
    [JsonSubtypes.KnownSubType(typeof(TermLoanRequest), "term-loan")]
    [JsonSubtypes.KnownSubType(typeof(OverdraftFacilityRequest), "overdraft-facility")]
    [JsonSubtypes.KnownSubType(typeof(CreditFacilityRequest), "credit-facility")]
    [JsonSubtypes.KnownSubType(typeof(CreditCardFacilityRequest), "credit-card-facility")]
    [JsonSubtypes.KnownSubType(typeof(AbstractArrangementRequest), "abstract")]
    [JsonSubtypes.KnownSubType(typeof(CreditLineRequest), "credit-line")]

    public class ArrangementRequest : ICalculationHelpers
    {
        [Required]
        public int ArrangementRequestId { get; set; } // generated as application number + seq. num

        [JsonIgnore]
        public Application Application { get; set; }

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
        }

        [Required]
        [MaxLength(128)]
        public string ProductCode { get; set; }
        public string ParentProductCode { get; set; }
        [Required]
        [MaxLength(128)]
        public string ProductName { get; set; }
        [Required]
        public ArrangementKind? ArrangementKind { get; set; }
        [MaxLength(128)]
        public string ArrangementNumber { get; set; } // generated later, by CBS
        public bool? Enabled { get; set; } = true;

        [JsonIgnore]
        public virtual ProductSnapshotDb ProductSnapshotDb { get; set; }

        [JsonIgnore]
        public string _ProductSnapshot { get; set; }

        [JsonIgnore]
        public string _ProductSnapshotHash { get; set; }

        [NotMapped]
        public ProductSnapshot ProductSnapshot
        {
            get { return ProductSnapshotDb == null ? null : ProductSnapshotDb.ProductSnapshot; }
            set
            {
                ProductSnapshotDb = new ProductSnapshotDb
                {
                    Hash = OfferUtility.CreateMD5(JsonConvert.SerializeObject(value)),
                    _ProductSnapshot = JsonConvert.SerializeObject(value)
                };
                _ProductSnapshotHash = ProductSnapshotDb.Hash;
            }
        }
        public string _Extended
        {
            get
            {
                return Extended == null ? null : JsonConvert.SerializeObject(Extended);
            }
            set
            {
                Extended = value == null ? null : JsonConvert.DeserializeObject<IDictionary<string, IDictionary<string, JToken>>>(value);
            }

        }
        [NotMapped]
        public IDictionary<string, IDictionary<string, JToken>> Extended { get; set; }
        [JsonIgnore]
        public string _Conditions
        {
            get
            {
                return Conditions == null ? null : JsonConvert.SerializeObject(Conditions);
            }
            set
            {
                Conditions = value == null ? null : JsonConvert.DeserializeObject<Conditions>(value);
            }
        }

        [NotMapped]
        public Conditions Conditions
        {
            get; set;
        }

        [JsonIgnore]
        public string _Accounts { get; set; }

        [NotMapped]
        public List<ArrangementAccountInfo> Accounts
        {
            get { return _Accounts == null ? null : JsonConvert.DeserializeObject<List<ArrangementAccountInfo>>(_Accounts); }
            set { _Accounts = JsonConvert.SerializeObject(value); }
        }

        public DateTime? CalculationDate { get; set; }

        public int NumberOfInstallments { get; set; }

        [JsonIgnore]
        public string _InstallmentPlan { get; set; }

        [NotMapped]
        public List<InstallmentPlanRow> InstallmentPlan
        {
            get { return _InstallmentPlan == null ? null : JsonConvert.DeserializeObject<List<InstallmentPlanRow>>(_InstallmentPlan); }
            set { _InstallmentPlan = JsonConvert.SerializeObject(value); }
        }
        [NotMapped]
        public decimal TotalRepayment {
            get
            {
                decimal total = 0;
                if (InstallmentPlan != null)
                {
                    foreach(var row in InstallmentPlan)
                    {
                        total += row.Annuity;
                    }
                }                
                return total;
            }
        }
        public RepaymentType? RepaymentType { get; set; }
        public int InstallmentScheduleDayOfMonth { get; set; }
        [JsonIgnore]
        public string _Campaign { get; set; }

        [NotMapped]
        public LeadModel Campaign
        {
            get
            {
                var campaign = _Campaign == null ? null : JsonConvert.DeserializeObject<LeadModel>(_Campaign);
                return campaign;
            }
            set { _Campaign = JsonConvert.SerializeObject(value); }
        }

        [JsonIgnore]
        public string _Options { get; set; }

        [NotMapped]
        public List<ProductOption> Options
        {
            get
            {
                var options = _Options == null ? null : JsonConvert.DeserializeObject<List<ProductOption>>(_Options);
                return options;
            }
            set { _Options = JsonConvert.SerializeObject(value); }
        }

        [JsonIgnore]
        public string _BundleInfo { get; set; }

        [NotMapped]
        public BundledProductInfo BundleInfo
        {
            get
            {
                var info = _BundleInfo == null ? null : JsonConvert.DeserializeObject<BundledProductInfo>(_BundleInfo);
                return info;
            }
            set { _BundleInfo = JsonConvert.SerializeObject(value); }
        }

        [JsonIgnore]
        public string _Periods { get; set; }

        [NotMapped]
        public List<ScheduledPeriod> Periods
        {
            get { return _Periods == null ? null : JsonConvert.DeserializeObject<List<ScheduledPeriod>>(_Periods); }
            set { _Periods = JsonConvert.SerializeObject(value); }
        }

        [NotMapped]
        public decimal TotalDisbursement { get; set; }
        [NotMapped]
        public decimal TotalAnnuity { get; set; }
        [NotMapped]
        public decimal TotalPrincipal { get; set; }
        [NotMapped]
        public decimal TotalInterest { get; set; }
        [NotMapped]
        public decimal TotalExpenses { get; set; }
        [NotMapped]
        public decimal TotalExpensesInDomesticCurrency { get; set; }
        [NotMapped]
        public decimal TotalCashCollateral { get; set; }
        [NotMapped]
        public decimal TotalNetCashFlow { get; set; }
        [NotMapped]
        public decimal TotalDiscountedNetCashFlow { get; set; }
        [NotMapped]
        public decimal TotalRepaymentAmount { get; set; }
        [NotMapped]
        public decimal TotalFeeAmount { get; set; }
        [NotMapped]
        public string CalculationMethod { get; set; }
        [NotMapped]
        public string InterestRateVariability { get; set; }
        public bool OverrideProductLimits { get; set; }

        [NotMapped]
        [JsonIgnore]
        public bool SerializeTotals { get; set; }
        

        [JsonIgnore]
        public string _RequestedValues { get; set; }

        [NotMapped]
        public RequestedValues RequestedValues
        {
            get { return _RequestedValues == null ? null : JsonConvert.DeserializeObject<RequestedValues>(_RequestedValues); }
            set
            {
                _RequestedValues = JsonConvert.SerializeObject(value);
            }
        }

        [JsonIgnore]
        public string _ApprovedLimits { get; set; }

        [NotMapped]
        public ApprovedLimits ApprovedLimits
        {
            get { return _ApprovedLimits == null ? null : JsonConvert.DeserializeObject<ApprovedLimits>(_ApprovedLimits); }
            set
            {
                _ApprovedLimits = JsonConvert.SerializeObject(value);
            }
        }

        [JsonIgnore]
        public string _AcceptedValues { get; set; }

        [NotMapped]
        public AcceptedValues AcceptedValues
        {
            get { return _AcceptedValues == null ? null : JsonConvert.DeserializeObject<AcceptedValues>(_AcceptedValues); }
            set
            {
                _AcceptedValues = JsonConvert.SerializeObject(value);
            }
        }

        public bool IsAbstractOrigin { get; set; } = false;
        public bool? IsOptional { get; set; } = true;


        [NotMapped]
        [JsonIgnore]
        public bool SerializeApplicationNumber { get; set; } = true;
        public bool ShouldSerializeApplicationNumber()
        {
            return SerializeApplicationNumber;
        }
        [NotMapped]
        [JsonIgnore]
        public bool SerializeArrangementRequestId { get; set; } = true;
        public bool ShouldSerializeArrangementRequestId()
        {
            return SerializeArrangementRequestId;
        }

        #region Should Serialize Totals
        public bool ShouldSerializeTotalDisbursement()
        {
            return SerializeTotals;
        }
        public bool ShouldSerializeTotalAnnuity()
        {
            return SerializeTotals;
        }
        public bool ShouldSerializeTotalPrincipal()
        {
            return SerializeTotals;
        }
        public bool ShouldSerializeTotalInterest()
        {
            return SerializeTotals;
        }
        public bool ShouldSerializeTotalExpenses()
        {
            return SerializeTotals;
        }
        public bool ShouldSerializeTotalExpensesInDomesticCurrency()
        {
            return SerializeTotals;
        }
        public bool ShouldSerializeTotalCashCollateral()
        {
            return SerializeTotals;
        }
        public bool ShouldSerializeTotalNetCashFlow()
        {
            return SerializeTotals;
        }
        public bool ShouldSerializeTotalDiscountedNetCashFlow()
        {
            return SerializeTotals;
        }
        public bool ShouldSerializeTotalRepaymentAmount()
        {
            return SerializeTotals;
        }
        public bool ShouldSerializeTotalFeeAmount()
        {
            return SerializeTotals;
        }
        public bool ShouldSerializeCalculationMethod()
        {
            return SerializeTotals;
        }
        public bool ShouldSerializeInterestRateVariability()
        {
            return SerializeTotals;
        }
        #endregion

        public virtual void CalculateOffer(Application application, OfferPriceCalculation priceCalculator, string conversionMethod)
        {
            // To be implemented for each arrangement request subtype
        }

        public virtual void CalculateOffer(PriceCalculationParameters calculationParameters, OfferPriceCalculation priceCalculator, string conversionMethod)
        {
            // To be implemented for each arrangement request subtype
        }

        public virtual bool IsFinanceService()
        {
            return false;
        }

        public virtual PriceCalculationParameters GetPriceCalculationParameters(Application application = null)
        {
            var parameters = application?.GetPriceCalculationParameters(ProductCode) ?? new PriceCalculationParameters();
            parameters.InterestRates = Utility.MergeRates(ProductSnapshot?.Conditions?.InterestRates, Conditions?.InterestRates);
            parameters.Fees = Utility.MergeFees(ProductSnapshot?.Conditions?.Fees, Conditions?.Fees);
            parameters.OtherConditions = ProductSnapshot?.Conditions?.Other;
            parameters.PartOfBundle = application?.OriginatesBundle != null && application.OriginatesBundle.Value ?
                application.ProductCode : ParentProductCode;
            parameters.Campaign = Campaign;
            parameters.Options = Options;
            parameters.ScheduledPeriods = Periods;
            return parameters;
        }

        public virtual void MergePriceCalculationResults(PriceCalculationResult result)
        {
            /*Conditions = Conditions ?? new Conditions();
            Conditions.InterestRates = result.InterestRates;
            Conditions.Fees = result.Fees;
            Conditions.Other = result.OtherConditions;
*/

            Conditions = Conditions ?? new Conditions
            {
                InterestRates = new List<InterestRateCondition>()
            };
            AddCustomVariationsToInterests(result.InterestRates);
            //if (result.InterestRates != null)
            //{
            //    Conditions.InterestRates.AddRange(result.InterestRates);
            //}
            Conditions.Fees = result.Fees;
            Conditions.Other = result.OtherConditions;
            /*if (result.Fees != null)
            {
                Conditions.Fees.AddRange(result.Fees);
            }
            if (result.OtherConditions != null)
            {
                Conditions.Other.AddRange(result.OtherConditions);
            }*/

        }

        private void AddCustomVariationsToInterests(List<InterestRateCondition> interestRates)
        {
            if (Conditions.InterestRates == null || Conditions.InterestRates.Count() == 0)
            {
                Conditions.InterestRates = interestRates;
                return;
            }
            if (interestRates == null || interestRates.Count() == 0)
            {
                return;
            }

            var includedCustomRates = new List<InterestRateCondition>();
            foreach (var rate in interestRates)
            {
                var customRate = Conditions.InterestRates.FirstOrDefault(r => 
                    r.Kind == rate.Kind &&
                    r.Currencies.Intersect(rate.Currencies).Count() == rate.Currencies.Count() &&
                    r.Periods == rate.Periods);
                if (customRate == null)
                {
                    continue;
                }
                else if (customRate.Variations == null || customRate.Variations.Count() == 0)
                {
                    includedCustomRates.Add(customRate);
                    continue;
                }
                rate.Variations = rate.Variations ?? new List<InterestRateVariation>();
                rate.Variations.AddRange(customRate.Variations.Except(rate.Variations));
                includedCustomRates.Add(customRate);
            }
            var notIncludedRates = Conditions.InterestRates.Where(r => !includedCustomRates.Contains(r)).ToList();
            //includedCustomRates.AddRange(notIncludedRates);
            interestRates.AddRange(notIncludedRates);
            Conditions.InterestRates = interestRates;
        }

        public ArrangementRequest Clone()
        {
            return (ArrangementRequest) MemberwiseClone();
        }

        //[JsonIgnore]
        //public string _CreditLineLimits { get; set; }

        //[NotMapped]
        //public CreditLineLimits CreditLineLimits
        //{
        //    get
        //    {
        //        var info = _CreditLineLimits == null ? null : JsonConvert.DeserializeObject<CreditLineLimits>(_CreditLineLimits);
        //        return info;
        //    }
        //    set { _CreditLineLimits = JsonConvert.SerializeObject(value); }
        //}

    }


    public class AbstractArrangementRequest : ArrangementRequest
    {

    }
}

