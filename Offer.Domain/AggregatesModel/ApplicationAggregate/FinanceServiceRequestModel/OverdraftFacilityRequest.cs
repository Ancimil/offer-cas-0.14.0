using PriceCalculation.Models.Pricing;

namespace Offer.Domain.AggregatesModel.ApplicationAggregate
{
    public partial class OverdraftFacilityRequest : FinanceServiceArrangementRequest
    {
        public override void MergePriceCalculationResults(PriceCalculationResult result)
        {
            base.MergePriceCalculationResults(result);
            Napr = result.Napr ?? 0;
        }
    }
}
