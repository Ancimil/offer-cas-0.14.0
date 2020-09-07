using Offer.Domain.Calculations;
using PriceCalculation.Models.Pricing;

namespace Offer.Domain.AggregatesModel.ApplicationAggregate
{
    public interface ICalculation
    {
        void CalculateOffer(Application application, OfferPriceCalculation priceCalculator, string conversionMethod);
        void CalculateOffer(PriceCalculationParameters calculationParameters, OfferPriceCalculation priceCalculator, string conversionMethod);
    }
}
