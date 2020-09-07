using PriceCalculation.Models.Pricing;
using PriceCalculation.Models.Product;
using System;
using System.Collections.Generic;
using static Offer.Domain.Calculations.InstallmentPlanCalculation;

namespace Offer.Domain.Utils
{
    public class ArrangementRequestInitializationParameters
    {
        public decimal? Amount { get; set; }
        public decimal? Annuity { get; set; }
        public string Term { get; set; }
        public string Currency { get; set; }
        public decimal? InterestRate { get; set; }
        public decimal? DownpaymentAmount { get; set; }
        public string DrawdownPeriod { get; set; }
        public DateTime? DrawdownPeriodStartDate { get; set; }
        public string GracePeriod { get; set; }
        public DateTime? GracePeriodStartDate { get; set; }
        public string RepaymentPeriod { get; set; }
        public DateTime? RepaymentPeriodStartDate { get; set; }
        public DateTime? MaturityDate { get; set; }
        public decimal? DownpaymentPercentage { get; set; }
        public string ProductCode { get; set; }
        public Conditions Conditions { get; set; }
        public List<ProductOption> ProductOptions { get; set; }
        public string CustomerNumber { get; set; }
        public bool? IsRefinancing { get; set; }
        public bool? IsAbstractOrigin { get; set; }
        public decimal? InvoiceAmount { get; set; }
        public List<string> BundledComponents { get; set; }
        public decimal? RevolvingPercentage { get; set; }
        public List<ScheduledPeriod> ScheduledPeriods { get; set; }
        public RepaymentType? RepaymentType { get; set; }
        public int InstallmentScheduleDayOfMonth { get; set; }
        public int MinimumDaysForFirstInstallment { get; set; }
        public DateTime? CalculationDate { get; set; } = DateTime.UtcNow;
    }
}
