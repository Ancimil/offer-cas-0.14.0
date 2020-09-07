using System;
using System.Collections.Generic;
using MediatR;
using Newtonsoft.Json.Linq;
using Offer.Domain.AggregatesModel.ApplicationAggregate;
using PriceCalculation.Models.LeadModel;
using PriceCalculation.Models.Pricing;
using PriceCalculation.Models.Product;
using static Offer.Domain.Calculations.InstallmentPlanCalculation;

namespace Offer.API.Application.Commands
{

    public class InitiateCalculateOfferCommand : IRequest<ArrangementRequest>
    {
        public ArrangementKind ArrangementKind { get; set; }
        public string Currency { get; private set; }
        public decimal Amount { get; private set; }
        public decimal Annuity { get; private set; }
        public string Term { get; private set; }
        public decimal InterestRate { get; private set; }
        public decimal DownpaymentAmount { get; private set; }
        public decimal InvoiceAmount { get; private set; }
        public decimal MinimalDownpaymentPercentage { get; private set; }
        public string Channel { get; private set; }
        public decimal? RiskScore { get; private set; }
        public string CreditRating { get; set; }
        public decimal? CustomerValue { get; set; }
        public decimal? DebtToIncome { get; set; }
        public string CustomerSegment { get; private set; }
        public string PartOfBundle { get; private set; }
        public string CollateralModel { get; private set; }
        public ProductConditions ProductConditions { get; set; }
        public Conditions Conditions { get; set; }
        public LeadModel Campaign { get; set; }
        public List<ProductOption> ProductOptions { get; set; }
        public Dictionary<string, JToken> BundledComponents { get; set; }
        public decimal DownpaymentPercentage { get; set; }
        public List<ScheduledPeriod> ScheduledPeriods { get; set; }

        // DATES
        public DateTime? CalculationDate { get; set; } = DateTime.Now;
        public DateTime? RequestDate { get; set; } = DateTime.Now;
        public DateTime? ApprovalDate { get; set; }
        public DateTime? SigningDate { get; set; }
        public DateTime? DisbursmentDate { get; set; }
        public DateTime? FirstInstallmentDate { get; set; }
        public DateTime? MaturityDate { get; set; }
        public string GracePeriod { get; set; }
        public DateTime? GracePeriodStartDate { get; set; }
        public string DrawdownPeriod { get; set; }
        public DateTime? DrawdownPeriodStartDate { get; set; }

        // simple calc.
        public RepaymentType? RepaymentType { get; set; }
        public int NumberOfInstallments { get; set; }
        public int MinimumDaysForFirstInstallment { get; set; } // sa proiz.
        public bool AdjustFirstInstallment { get; set; } // sa conf.
        public bool PayFeeFromDisbursement { get; set; } // ne treba trenutno
        public IntercalarInterestRepaymentType IntercalarInterestRepaymentType { get; set; }
        public SimpleSchedule InstallmentSchedule { get; set; }



        public InitiateCalculateOfferCommand(ArrangementKind arrangementKind, string currency, decimal amount, decimal annuity,
            string term, decimal interestRate, decimal downpaymentAmount, decimal invoiceAmount, decimal minimalDownpaymentPercentage,
            string channel, decimal? riskScore, string creditRating, decimal? customerValue, decimal? debtToIncome, string customerSegment,
            string partOfBundle, string collateralModel, ProductConditions productConditions, Conditions conditions, LeadModel campaign,
            List<ProductOption> productOptions, Dictionary<string, JToken> bundledComponents, decimal downpaymentPercentage,
            List<ScheduledPeriod> scheduledPeriods, DateTime? calculationDate, DateTime? requestDate, DateTime? approvalDate,
            DateTime? signingDate, DateTime? disbursmentDate, DateTime? firstInstallmentDate, DateTime? maturityDate,
            string gracePeriod, DateTime? gracePeriodStartDate, string drawdownPeriod, DateTime? drawdownPeriodStartDate,
            RepaymentType? repaymentType, int numberOfInstallments, int minimumDaysForFirstInstallment, bool adjustFirstInstallment, bool payFeeFromDisbursement,
            SimpleSchedule installmentSchedule, IntercalarInterestRepaymentType intercalarInterestRepaymentType)
        {
            ArrangementKind = arrangementKind;
            Currency = currency;
            Amount = amount;
            Annuity = annuity;
            Term = term;
            InterestRate = interestRate;
            DownpaymentAmount = downpaymentAmount;
            InvoiceAmount = invoiceAmount;
            MinimalDownpaymentPercentage = minimalDownpaymentPercentage;
            Channel = channel;
            RiskScore = riskScore;
            CreditRating = creditRating;
            CustomerValue = customerValue;
            DebtToIncome = debtToIncome;
            CustomerSegment = customerSegment;
            PartOfBundle = partOfBundle;
            CollateralModel = collateralModel;
            ProductConditions = productConditions;
            Conditions = conditions;
            Campaign = campaign;
            ProductOptions = productOptions;
            BundledComponents = bundledComponents;
            DownpaymentPercentage = downpaymentPercentage;
            ScheduledPeriods = scheduledPeriods;
            CalculationDate = calculationDate ?? DateTime.Now;
            RequestDate = requestDate ?? DateTime.Now;
            ApprovalDate = approvalDate;
            SigningDate = signingDate;
            DisbursmentDate = disbursmentDate;
            FirstInstallmentDate = firstInstallmentDate;
            MaturityDate = maturityDate;
            GracePeriod = gracePeriod;
            GracePeriodStartDate = gracePeriodStartDate;
            DrawdownPeriod = drawdownPeriod;
            DrawdownPeriodStartDate = drawdownPeriodStartDate;
            RepaymentType = repaymentType ?? Domain.Calculations.InstallmentPlanCalculation.RepaymentType.FixedAnnuity;
            NumberOfInstallments = numberOfInstallments;
            MinimumDaysForFirstInstallment = minimumDaysForFirstInstallment;
            AdjustFirstInstallment = adjustFirstInstallment;
            PayFeeFromDisbursement = payFeeFromDisbursement;
            InstallmentSchedule = installmentSchedule;
            IntercalarInterestRepaymentType = intercalarInterestRepaymentType;
        }

        public override string ToString()
        {
            return "Amount: " + Amount + Currency + ", Annuity: " + Annuity + ", Term: " + Term;
        }
    }

}