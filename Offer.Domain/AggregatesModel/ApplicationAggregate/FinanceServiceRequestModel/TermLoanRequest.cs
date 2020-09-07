using MicroserviceCommon.Models;
using Newtonsoft.Json;
using PriceCalculation.Models.Pricing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Offer.Domain.AggregatesModel.ApplicationAggregate
{
    public partial class TermLoanRequest : FinanceServiceArrangementRequest
    {
        public decimal Annuity { get; set; }

        public decimal InvoiceAmount { get; set; }
        public decimal InvoiceAmountInLoanCurrency { get; set; } // currency as in ArrangementAmount
        public decimal DownpaymentAmount { get; set; }
        [Column("DownpaymentInLoanCurrency")]
        public decimal DownpaymentAmountInLoanCurrency { get; set; } // currency as in ArrangementAmount
        public decimal? DownpaymentPercentage { get; set; } = 0;

        [MaxLength(64)]
        public string GracePeriod { get; set; }
        public DateTime? GracePeriodStartDate { get; set; }

        [MaxLength(64)]
        public string DrawdownPeriod { get; set; }
        public DateTime? DrawdownPeriodStartDate { get; set; }
        [MaxLength(64)]
        public string RepaymentPeriod { get; set; }
        public DateTime? RepaymentPeriodStartDate { get; set; }

        [NotMapped]
        public List<DisbursementInfo> DisbursementsInfo { get; set; }

        public bool IsRefinancing { get; set; }

        [NotMapped]
        public decimal? TotalDisbursementAmount { get; set; }

        [Column("DisbursementsInfo")]
        [JsonIgnore]
        public string _DisbursementsInfo
        {
            get
            {
                return DisbursementsInfo == null ? null : JsonConvert.SerializeObject(DisbursementsInfo);
            }
            set
            {
                DisbursementsInfo = value == null ? null : JsonConvert.DeserializeObject<List<DisbursementInfo>>(value);
            }
        }

        public override void MergePriceCalculationResults(PriceCalculationResult result)
        {
            base.MergePriceCalculationResults(result);
            Napr = result.Napr ?? 0;
        }
    }

    public class DisbursementInfo
    {
        public Currency Amount { get; set; }
        // public bool? IsPayed { get; set; }
        public string BeneficiaryName { get; set; }
        public string AccountNumber { get; set; }
        public string InvoiceNumber { get; set; }
        public string ReferenceNumber { get; set; }
        public string ProductDescription { get; set; }
        public string CheckDigitModel { get; set; }
        public string LoanKind { get; set; }
        public DateTime? DisbursementDate { get; set; }
        public string Type { get; set; }
    }
}
