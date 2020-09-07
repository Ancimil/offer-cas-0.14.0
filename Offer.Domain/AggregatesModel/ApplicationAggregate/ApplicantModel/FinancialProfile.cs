using MicroserviceCommon.Models;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using Enumeration = MicroserviceCommon.Contracts.Enumeration;

namespace Offer.Domain.AggregatesModel.ApplicationAggregate.ApplicantModel
{
    public class FinancialProfile
    {
        public List<Income> IncomeInfo { get; set; }
        public List<Expense> ExpenseInfo { get; set; }
    }

    [Enumeration("income-source", "Income Source", "Income Source")]
    public enum IncomeSource
    {
        [EnumMember(Value = "salary")]
        [Description("Salary")]
        Salary,
        [EnumMember(Value = "pension")]
        [Description("Pension")]
        Pension,
        [EnumMember(Value = "contract")]
        [Description("Contract")]
        Contract,
        [EnumMember(Value = "rent")]
        [Description("Rent")]
        Rent,
        [EnumMember(Value = "alimony")]
        [Description("Alimony")]
        Alimony,
        [EnumMember(Value = "royalty")]
        [Description("Royalty")]
        Royalty,
        [EnumMember(Value = "board-membership")]
        [Description("Board Membership")]
        BoardMembership,
        [EnumMember(Value = "separation-allowance")]
        [Description("Separation Allowance")]
        SeparationAllowance
    }

    [Enumeration("expense-origin", "Expense Origin", "Expense Origin")]
    public enum ExpenseOrigin
    {
        [EnumMember(Value = "credit-bureau")]
        [Description("CreditBureau")]
        CreditBureau,
        [EnumMember(Value = "other")]
        [Description("Other")]
        Other
    }

    public class BaseAmount
    {
        public Currency Amount { get; set; }
    }

    public class Income: BaseAmount
    {
        public IncomeSource Source { get; set; }
    }

    public class Expense: BaseAmount
    {
        public string Source { get; set; }
        public ExpenseOrigin Origin { get; set; } = ExpenseOrigin.Other;
    }

    public class FinancialData: FinancialProfile
    {
        public Currency TotalIncomes { get; set; }
        public Currency TotalExpenses { get; set; }
    }
}
