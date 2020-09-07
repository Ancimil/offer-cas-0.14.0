using PriceCalculatationTest.Initialization;
using PriceCalculation.Calculations;
using Xunit;

namespace PriceCalculatationTest
{
    public class FixedRateTests
    {
        private PriceCalculator PriceCalculator { get; set; }
        public FixedRateTests()
        {
            PriceCalculator = PriceCalculationInitializer.GetPriceCalculator();
        }

        /*[Fact]
        public void NoParamsDefined()
        {
            var calcParams = PriceCalculationInitializer.GetCalculationParameters("no-params-defined");
            var calcRes = PriceCalculator.CalculatePrice(calcParams)?.Result;

            Assert.Null(calcRes);
        }*/

        [Theory]
        [InlineData("no-rate-variations-single-positive-spread")]
        [InlineData("no-rate-variations-single-zero-spread")]
        [InlineData("no-rate-variations-single-negative-spread")]
        public void NoVariationsSingleRate(string paramsType)
        {
            var calcParams = PriceCalculationInitializer.GetCalculationParameters(paramsType);
            var calcRes = PriceCalculator.CalculatePrice(calcParams)?.Result;

            foreach (var rate in calcRes.InterestRates)
            {
                Assert.True(rate.CalculatedRate == rate.Rate.SpreadRateValue);
            }
        }

        [Theory]
        [InlineData("no-variations-exceeds-upper-limit")]
        [InlineData("no-variations-exceeds-lower-limit")]
        public void NoVariationsExeedsLimits(string paramsType)
        {
            var calcParams = PriceCalculationInitializer.GetCalculationParameters(paramsType);
            var calcRes = PriceCalculator.CalculatePrice(calcParams)?.Result;

            foreach (var rate in calcRes.InterestRates)
            {
                Assert.True(rate.CalculatedRate == rate.Rate.SpreadRateValue);
            }
        }
    }
}
