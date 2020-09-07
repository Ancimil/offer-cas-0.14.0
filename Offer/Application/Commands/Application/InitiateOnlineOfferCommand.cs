using MediatR;
using Newtonsoft.Json;
using Offer.Domain.AggregatesModel.ApplicationAggregate;
using PriceCalculation.Models.Pricing;
using PriceCalculation.Models.Product;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using static Offer.Domain.Calculations.InstallmentPlanCalculation;

namespace Offer.API.Application.Commands
{

    public enum CommandResult
    {
        INVALID_CALCULATION,
        LEAD_ALREADY_EXIST,
        OK
    }

    public class IntialOnlineOfferCommandResult
    {
        public string ApplicationNumber { get; set; }
        public CommandResult Result { get; set; }
    }

    public class InitiateOnlineOfferCommand : IRequest<IntialOnlineOfferCommandResult>
    {
        public string MobilePhone { get;  set; }
        public string EmailAddress { get; set; }
        public string ProductCode { get;  set; }
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
        public decimal DownpaymentAmount { get;  set; }
        public decimal InvoiceAmount { get;  set; }
        public string Username { get; set; }
        public string CountryCode { get; set; }
        public string PrefferedCulture { get; set; }
        public string CustomerNumber { get; set; }
        public Conditions Conditions { get; set; }
        public string Channel { get; set; }
        // Don't take this parameter in constructor because we don't trust client's information
        public bool IsCustomer { get; set; }
        public PartyKind PartyKind { get; set; }
        public List<ProductOption> ProductOptions { get; set; }
        public bool IsRefinancing { get; set; }
        public decimal RevolvingPercentage { get; set; }
        public decimal DownpaymentPercentage { get; set; }
        public string CollateralModel { get; set; }

        public List<ScheduledPeriod> ScheduledPeriods { get; set; }

        // DATES
        //public DateTime? CalculationDate { get; set; } = DateTime.Now;
        //public DateTime? RequestDate { get; set; } = DateTime.Now;
        public DateTime? ApprovalDate { get; set; }
        public DateTime? SigningDate { get; set; }
        public DateTime? DisbursmentDate { get; set; }
        public DateTime? FirstInstallmentDate { get; set; }
        public DateTime? MaturityDate { get; set; }
        public string GracePeriod { get; set; }
        public DateTime? GracePeriodStartDate { get; set; }
        public string DrawdownPeriod { get; set; }
        public DateTime? DrawdownPeriodStartDate { get; set; }
        public string RepaymentPeriod { get; set; }
        public DateTime? RepaymentPeriodStartDate { get; set; }
        public long? LeadId { get; set; }
        public RepaymentType? RepaymentType { get; set; }
        public string AgentOrganizationUnit { get; set; }
        public bool IsProposal { get; set; }
        public int InstallmentScheduleDayOfMonth { get; set; }
        public override string ToString()
        {
            return "Product Code: " + ProductCode + ", Email Address: " + EmailAddress + ", Amount: " + Amount + Currency + ", Annuity: " + Annuity + ", Term: " + Term;
        }
    }
}
