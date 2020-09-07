using PriceCalculation.Models.Pricing;

namespace Offer.Domain.AggregatesModel.ApplicationAggregate
{
    public class DepositRequest : ArrangementRequest
    {
        public string Currency { get; set; }
        public decimal Eapr { get; set; }
        public decimal Napr { get; set; }
        public override PriceCalculationParameters GetPriceCalculationParameters(Application application)
        {
            var parameters = base.GetPriceCalculationParameters(application);
            parameters.Currency = Currency;
            return parameters;
        }
    }
}