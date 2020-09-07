using PriceCalculatationTest.Services;
using PriceCalculation.Calculations;
using PriceCalculation.Models.Pricing;
using System;

namespace PriceCalculatationTest.Initialization
{
    public partial class PriceCalculationInitializer
    {
        private static PriceCalculationParameters Parameters {
            get
            {
                return new PriceCalculationParameters
                {
                    Currency = "RSD",
                    DebtToIncome = (decimal)0.3,
                    CustomerValue = 100,
                    CreditRating = "A",
                    CustomerSegment = "professional",
                    Channel = "web",
                    RequestDate = DateTime.Now,
                    Term = "P10M",
                    Amount = (decimal)10000.55,
                    RiskScore = 200,
                    CollateralModel = "0021",
                    PartOfBundle = "0001",
                    Fees = null,
                    ScheduledPeriods = null,
                    AdditionalProperties = null,
                    InterestRates = null,
                    OtherConditions = null,
                    Options = null,
                    Campaign = null
                };
            }
        }

        public static PriceCalculator GetPriceCalculator()
        {
            return new PriceCalculator(new PriceCalculationService(), new MarketRates(), new ConfigurationService()); ;
        }

        public static PriceCalculationParameters GetCalculationParameters(string type = null)
        {
            switch (type)
            {
                case "no-rate-variations-single-positive-spread":
                    return NoVariationsSingleRate(5);
                case "no-rate-variations-single-zero-spread":
                    return NoVariationsSingleRate(0);
                case "no-rate-variations-single-negative-spread":
                    return NoVariationsSingleRate(-5);
                case "no-params-defined":
                    return null;
                case "fixed-rate":
                    return Parameters;
                default:
                    break;
            }
            return Parameters;
        }
    }
}
