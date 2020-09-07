using PriceCalculation.Models.Pricing;

namespace Offer.Domain.AggregatesModel.ApplicationAggregate
{
    public class CurrentAccountRequest : DepositRequest
    {
        public override void MergePriceCalculationResults(PriceCalculationResult result)
        {
            base.MergePriceCalculationResults(result);
            Napr = result.Napr ?? 0;
        }
    }
}
