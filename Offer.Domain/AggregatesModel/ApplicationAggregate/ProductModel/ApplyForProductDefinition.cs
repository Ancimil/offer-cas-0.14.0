using PriceCalculation.Models.Pricing;
using PriceCalculation.Models.Product;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Offer.Domain.AggregatesModel.ApplicationAggregate.ProductModel
{
    public class ApplyForProductDefinition
    {
        public string MobilePhone { get; set; }
        public string EmailAddress { get; set; }
        [Required]
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public string GivenName { get; set; }
        public string Surname { get; set; }
        public string ParentName { get; set; }
        public IdentificationKind IdentificationNumberKind { get; set; }
        public string IdentificationNumber { get; set; }
        public string Currency { get; set; }
        public decimal Amount { get; set; }
        public decimal Annuity { get; set; }
        public string Term { get; set; }
        public decimal InterestRate { get; set; }
        public decimal DownpaymentAmount { get; set; }
        public decimal InvoiceAmount { get; set; }
        public string Username { get; set; }
        public string CountryCode { get; set; }
        public string PrefferedCulture { get; set; }
        public string CustomerNumber { get; set; }
        public string DrawdownPeriod { get; set; }
        public DateTime? DrawdownPeriodStartDate { get; set; }
        public string GracePeriod { get; set; }
        public DateTime? GracePeriodStartDate { get; set; }
        public string RepaymentPeriod { get; set; }
        public DateTime? RepaymentPeriodStartDate { get; set; }
        public DateTime? MaturityDate { get; set; }
        public Conditions Conditions { get; set; }
        public string Channel { get; set; }
        // Don't take this parameter in constructor because we don't trust client's information
        public bool IsCustomer { get; set; }
        public PartyKind PartyKind { get; set; }
        public List<ProductOption> ProductOptions { get; set; }
        public bool IsRefinancing { get; set; }
        public decimal RevolvingPercentage { get; set; }
        public decimal DownpaymentPercentage { get; set; }
        public override string ToString()
        {
            return "Product Code: " + ProductCode + ", Email Address: " + EmailAddress + ", Amount: " + Amount + Currency + ", Annuity: " + Annuity + ", Term: " + Term;
        }
    }
}
