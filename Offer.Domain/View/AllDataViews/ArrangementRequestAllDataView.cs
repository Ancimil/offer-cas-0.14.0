using JsonSubTypes;
using MicroserviceCommon.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Offer.Domain.AggregatesModel.ApplicationAggregate;
using Offer.Domain.AggregatesModel.ApplicationAggregate.AlternativeOfferModel;
using Offer.Domain.AggregatesModel.ApplicationAggregate.ProductModel;
using PriceCalculation.Calculations;
using PriceCalculation.Models.LeadModel;
using PriceCalculation.Models.Pricing;
using PriceCalculation.Models.Product;
using System;
using System.Collections.Generic;

namespace Offer.Domain.View.AllDataViews
{
    [JsonConverter(typeof(JsonSubtypes), "arrangement-kind")]
    [JsonSubtypes.KnownSubType(typeof(CurrentAccountRequestAllDataView), "current-account")]
    [JsonSubtypes.KnownSubType(typeof(DemandDepositRequestAllDataView), "demand-deposit")]
    [JsonSubtypes.KnownSubType(typeof(TermDepositRequestAllDataView), "term-deposit")]
    [JsonSubtypes.KnownSubType(typeof(ElectronicAccessArrangementRequestAllDataView), "electronic-access-arrangement")]
    [JsonSubtypes.KnownSubType(typeof(CardAccessArrangementRequestAllDataView), "card-access-arrangement")]
    [JsonSubtypes.KnownSubType(typeof(SecuritiesArrangementRequestAllDataView), "securities-arrangement")]
    [JsonSubtypes.KnownSubType(typeof(OtherProductArrangementRequestAllDataView), "other-product-arrangement")]
    [JsonSubtypes.KnownSubType(typeof(TermLoanRequestAllDataRequest), "term-loan")]
    [JsonSubtypes.KnownSubType(typeof(OverdraftFacilityRequestAllDataView), "overdraft-facility")]
    [JsonSubtypes.KnownSubType(typeof(CreditFacilityRequestAllDataView), "credit-facility")]
    [JsonSubtypes.KnownSubType(typeof(CreditCardFacilityRequestAllDataView), "credit-card-facility")]
    [JsonSubtypes.KnownSubType(typeof(AbstractArrangementRequestAllDataView), "abstract")]
    [JsonSubtypes.KnownSubType(typeof(CreditLineRequestAllDataView), "credit-line")]
    public class ArrangementRequestAllDataView
    {
        // Common
        public List<ArrangementAccountInfo> Accounts { get; set; }
        public int AccountsCount
        {
            get
            {
                return Accounts?.Count ?? 0;
            }
        }
        public ArrangementKind? ArrangementKind { get; set; }
        public string ArrangementNumber { get; set; }
        public int ArrangementRequestId { get; set; }
        public BundledProductInfo BundleInfo { get; set; }
        public bool IsAbstractOrigin { get; set; }
        public bool? IsOptional { get; set; }
        public string ParentProductCode { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public string ProductFamily { get; set; }

        public ConditionsAllDataView Conditions { get; set; }

        public AcceptedValues AcceptedValues { get; set; }

        public ApprovedLimits ApprovedLimits { get; set; }

        public CalculationAllDataView Calculation { get; set; }
        public LeadModelAllDataView Campaign { get; set; }
        public CurrentValues CurrentValues { get; set; } 
        public RequestedValues RequestedValues { get; set; }
        public List<ProductOption> Options { get; set; }
        public IDictionary<string, IDictionary<string, JToken>> Extended { get; set; }
    }

    public class CalculationAllDataView
    {
        public DateTime? CalculationDate { get; set; }
        public string CalculationMethod { get; set; }
        public List<InstallmentPlanRow> InstallmentPlan { get; set; }
        public string InterestRateVariability { get; set; }
        public int? NumberOfInstallments { get; set; }
        public bool? OverrideProductLimits { get; set; }
        public List<ScheduledPeriod> Periods { get; set; }
        public decimal? TotalAnnuity { get; set; }
        public decimal? TotalCashCollateral { get; set; }
        public decimal? TotalDisbursement { get; set; }
        public decimal? TotalDiscountedNetCashFlow { get; set; }
        public decimal? TotalExpenses { get; set; }
        public decimal? TotalExpensesInDomesticCurrency { get; set; }
        public decimal? TotalFeeAmount { get; set; }
        public decimal? TotalInterest { get; set; }
        public decimal? TotalNetCashFlow { get; set; }
        public decimal? TotalPrincipal { get; set; }
        public decimal? TotalRepaymentAmount { get; set; }
    }
    
    public class BenefitModelAllDataView
    {
        public decimal Value { get; set; }
        public string Kind { get; set; }
    }

    public class LeadModelAllDataView
    {
        public ICollection<BenefitModelAllDataView> Benefits { get; set; }
        public string CampaignCode { get; set; }
        public string Description { get; set; }
        public DateTime ExpirationDate { get; set; }
        public long LeadId { get; set; }
        public CommercialDetails InitialTerms { get; set; }
        public CommercialDetails ApprovedTerms { get; set; }
        public CommercialDetails ContractedTerms { get; set; }
        public string PartyId { get; set; }
        public bool PreApproved { get; set; }
        public string AssignedTo { get; set; }
        public bool SkipCb { get; set; }
        public bool SkipScoring { get; set; }
        public bool SkipEligibility { get; set; }
        public string ProductCode { get; set; }
        public LeadStatus LeadStatus { get; set; }
        public string ExternalLeadId { get; set; }
        public string OfferStatus { get; set; }
        public DateTime StatusChangeDate { get; set; }
        public string DisqualificationReason { get; set; }
        public string CancelationReason { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class CurrentValues
    {
        public decimal? Amount { get; set; }
        public decimal? Annuity { get; set; }
        public string Term { get; set; }
        public decimal? Napr { get; set; }
        public decimal? Eapr { get; set; }
        public decimal? DownpaymentAmount { get; set; }
        public decimal? DownpaymentPercentage { get; set; }
        public decimal? InvoiceAmount { get; set; }
        public decimal? AmountInDomesticCurrency { get; set; }
    }

    public class FinanceServiceDetails
    {
        public decimal? Amount { get; set; }
        public decimal? AmountInDomesticCurrency { get; set; }
        public decimal? Annuity { get; set; }
        public string Term { get; set; }
        public decimal? Napr { get; set; }
        public decimal? Eapr { get; set; }
        public decimal? DownpaymentAmount { get; set; }
        public decimal? DownpaymentPercentage { get; set; }
        public decimal? InvoiceAmount { get; set; }
        public int TermInMonths
        {
            get
            {
                try
                {
                    return Utility.GetMonthsFromPeriod(Term);
                }
                catch
                {
                    return 0;
                }
            }
        }
    }

    public class FinanceServiceRequestAllDataView : ArrangementRequestAllDataView
    {
        public string Currency { get; set; }
    }

    public class TermLoanRequestAllDataRequest : FinanceServiceRequestAllDataView
    {
        public decimal? TotalDisbursementAmount { get; set; }
        public List<DisbursementInfo> DisbursementsInfo { get; set; }
        public decimal DownpaymentAmountInLoanCurrency { get; set; }
        public decimal InvoiceAmountInLoanCurrency { get; set; }
        public bool IsRefinancing { get; set; }
        public FinanceServiceDetails FinanceServiceDetails { get; set; }
    }

    public class CreditLineRequestAllDataView : FinanceServiceRequestAllDataView
    {
        public CreditLineLimits CreditLineLimits { get; set; }
    }

    public class CreditCardFacilityRequestAllDataView : FinanceServiceRequestAllDataView
    {
        public Currency MinimalRepaymentAmount { get; set; }
        public decimal MinimalRepaymentPercentage { get; set; }
        public decimal RevolvingPercentage { get; set; }
    }

    public class CreditFacilityRequestAllDataView : FinanceServiceRequestAllDataView
    {
        public Currency MinimalRepaymentAmount { get; set; }
        public decimal MinimalRepaymentPercentage { get; set; }
    }

    public class DepositRequestAllDataView : ArrangementRequestAllDataView
    {
        public string Currency { get; set; }
        public decimal Eapr { get; set; }
        public decimal Napr { get; set; }
    }

    public class CurrentAccountRequestAllDataView : DepositRequestAllDataView { }

    public class DemandDepositRequestAllDataView : ArrangementRequestAllDataView { }

    public class TermDepositRequestAllDataView : ArrangementRequestAllDataView { }

    public class ElectronicAccessArrangementRequestAllDataView : ArrangementRequestAllDataView { }
    public class CardAccessArrangementRequestAllDataView : ArrangementRequestAllDataView { }
    public class SecuritiesArrangementRequestAllDataView : ArrangementRequestAllDataView { }
    public class OtherProductArrangementRequestAllDataView : ArrangementRequestAllDataView { }
    public class OverdraftFacilityRequestAllDataView : ArrangementRequestAllDataView { }
    public class AbstractArrangementRequestAllDataView : ArrangementRequestAllDataView { }
}
