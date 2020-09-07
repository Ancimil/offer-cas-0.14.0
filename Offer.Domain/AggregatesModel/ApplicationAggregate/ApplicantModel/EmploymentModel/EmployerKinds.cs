using System.Runtime.Serialization;
using System.ComponentModel;
using MicroserviceCommon.Contracts;

namespace Offer.Domain.AggregatesModel.ApplicationAggregate
{
    [Enumeration("employer-kinds", "Employer Kind", "Employer Kind")]
    public enum EmployerKinds
    {
        [EnumMember(Value = "budgetary-and-public")]
        [Description("Budgetary And Public")]
        BudgetaryAndPublic,

        [EnumMember(Value = "enterpreneur-employer-kind")]
        [Description("Enterpreneur Employer Kind")]
        EnterpreneurEmployerKind,

        [EnumMember(Value = "join-stock-company")]
        [Description("Join Stock Company")]
        JoinStockCompany,

        [EnumMember(Value = "limeted-liability-company")]
        [Description("Limeted Liability Company")]
        LimetedLiabilityCompany,

        [EnumMember(Value = "other-employer-kind")]
        [Description("Other Employer Kind")]
        OtherEmployerKind
    }
}
