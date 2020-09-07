using Microsoft.EntityFrameworkCore;
using PriceCalculation.Calculations;
using PriceCalculation.Models.Pricing;

namespace Offer.Domain.AggregatesModel.ApplicationAggregate
{
    public class DemandDepositRequest : DepositRequest
    {
        public SavingsPlan SavingsPlan { get; set; }

        public override PriceCalculationParameters GetPriceCalculationParameters(Application application)
        {
            return new PriceCalculationParameters
            {
                RequestDate = application.RequestDate,
                InterestRates = Utility.MergeRates(ProductSnapshot?.Conditions?.InterestRates, Conditions?.InterestRates),
                Fees = Utility.MergeFees(ProductSnapshot?.Conditions?.Fees, Conditions?.Fees),
                OtherConditions = ProductSnapshot?.Conditions?.Other,
                Currency = Currency,
                Channel = application.ChannelCode,
                RiskScore = application.RiskScore,
                CustomerSegment = application.CustomerSegment,
                PartOfBundle = ParentProductCode,
                Campaign = Campaign,
                Options = Options
            };
        }

        public override void MergePriceCalculationResults(PriceCalculationResult result)
        {
            base.MergePriceCalculationResults(result);
            Napr = result.Napr ?? 0;
        }
    }

    [Owned]
    public class SavingsPlan
    {
        public string Iteration { get; set; }
        public decimal Amount { get; set; }
    }
}
