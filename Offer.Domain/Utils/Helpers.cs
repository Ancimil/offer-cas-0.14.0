using Offer.Domain.AggregatesModel.ApplicationAggregate;

namespace Offer.Domain.Utils
{
    public class Helpers
    {
        public static ArrangementKind? GetArrangmentKindByProductKind(ProductKinds kind)
        {
            switch (kind)
            {
                case ProductKinds.CardAccessProduct:
                    return ArrangementKind.CardAccessArrangement;
                case ProductKinds.CreditFacilityProduct:
                    return ArrangementKind.CreditFacility;
                case ProductKinds.CurrentAccountProduct:
                    return ArrangementKind.CurrentAccount;
                case ProductKinds.DemandDepositProduct:
                    return ArrangementKind.DemandDeposit;
                case ProductKinds.ElectronicAccessProduct:
                    return ArrangementKind.ElectronicAccessArrangement;
                case ProductKinds.OverdraftFacilityProduct:
                    return ArrangementKind.OverdraftFacility;
                case ProductKinds.TermDepositProduct:
                    return ArrangementKind.TermDeposit;
                case ProductKinds.TermLoanProduct:
                    return ArrangementKind.TermLoan;
                case ProductKinds.CreditCardFacilityProduct:
                    return ArrangementKind.CreditCardFacility;
                case ProductKinds.AbstractProduct:
                    return ArrangementKind.Abstract;
                default:
                    return null;
            }
        }
    }
}
