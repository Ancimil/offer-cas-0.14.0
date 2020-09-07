using MicroserviceCommon.Contracts;
using System.Runtime.Serialization;

namespace Offer.Domain.AggregatesModel.ApplicationAggregate
{
    [Enumeration("arrangement-kind", "Arrangement Kind")]
    public enum ArrangementKind
    {
        // DEPOSIT REQUESTS
        [EnumMember(Value = "current-account")]
        [System.ComponentModel.Description("current-account")]
        CurrentAccount,

        [EnumMember(Value = "demand-deposit")]
        [System.ComponentModel.Description("demand-deposit")]
        DemandDeposit,

        [EnumMember(Value = "term-deposit")]
        [System.ComponentModel.Description("term-deposit")]
        TermDeposit,

        // OTHER ARRANGEMENT REQUESTS
        [EnumMember(Value = "electronic-access-arrangement")]
        [System.ComponentModel.Description("electronic-access-arrangement")]
        ElectronicAccessArrangement,

        [EnumMember(Value = "card-access-arrangement")]
        [System.ComponentModel.Description("card-access-arrangement")]
        CardAccessArrangement,
        
        [EnumMember(Value = "securities-arrangement")]
        [System.ComponentModel.Description("securities-arrangement")]
        SecuritiesArrangement,

        [EnumMember(Value = "other-product-arrangement")]
        [System.ComponentModel.Description("other-product-arrangement")]
        OtherProductArrangement,

        //FINANCE SERVICE REQUESTS
        [EnumMember(Value = "term-loan")]
        [System.ComponentModel.Description("term-loan")]
        TermLoan,

        [EnumMember(Value = "overdraft-facility")]
        [System.ComponentModel.Description("overdraft-facility")]
        OverdraftFacility,

        [EnumMember(Value = "credit-facility")]
        [System.ComponentModel.Description("credit-facility")]
        CreditFacility,

        [EnumMember(Value = "credit-card-facility")]
        [System.ComponentModel.Description("credit-card-facility")]
        CreditCardFacility,

        [EnumMember(Value = "credit-line")]
        [System.ComponentModel.Description("credit-line")]
        CreditLine,

        //ABSTRACT
        [EnumMember(Value = "abstract")]
        [System.ComponentModel.Description("abstract")]
        Abstract
    }
}