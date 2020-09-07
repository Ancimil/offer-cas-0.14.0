using PriceCalculation.Models.Pricing;

namespace Offer.Domain.AggregatesModel.ApplicationAggregate
{
    public interface ICalculationHelpers : ICalculation
    {
        PriceCalculationParameters GetPriceCalculationParameters(Application application);
        void MergePriceCalculationResults(PriceCalculationResult result);
        bool IsFinanceService();
    }
}
