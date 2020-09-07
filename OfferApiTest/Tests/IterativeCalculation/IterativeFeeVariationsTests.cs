using System;
using Xunit;
using Offer.Domain.Calculations;
using Offer.Domain.AggregatesModel.ApplicationAggregate;
using PriceCalculation.Models.Pricing;
using System.Collections.Generic;
using MicroserviceCommon.Models;
using PriceCalculation.Models.Product;

namespace OfferApiTest.Tests
{
    public class IterativeFeeVariationsTests
    {
        [Fact]
        //Percentage and FixedAmount have a variation that sets them to near upper limit but still below
        public void IterativeCalculatedNearUpperLimit()
        {
            ConfigurationServiceMock configurationService = new ConfigurationServiceMock();
            PriceCalculationDMNServiceMock priceCalCService = new PriceCalculationDMNServiceMock(configurationService);
            MarketRatesServiceMock marketRatesService = new MarketRatesServiceMock();
            var priceCalculator = new OfferPriceCalculation(priceCalCService, marketRatesService, configurationService, null);
            FeeCondition regularFee = new FeeCondition
            {
                Percentage = (decimal)0.1,
                FixedAmount = new Currency { Amount = 20, Code = "EUR" },
                LowerLimit = new Currency { Amount = 20, Code = "EUR" },
                UpperLimit = new Currency { Amount = 200, Code = "EUR" },
                VariationsDefinitionDMN = "product/price-variations/iterative-fee-in-range-upper.dmn",
                PercentageLowerLimit = (decimal)0.01,
                PercentageUpperLimit = 1,
                Title = "Origination-fee1",
                EffectiveDate = DateTime.Now.AddDays(-1),
                Currencies = new List<string> { "RSD", "EUR" }
            };
            
            ProductConditions conditions = new ProductConditions
            {
                Fees = new List<FeeCondition> { regularFee },
                InterestRates = new List<InterestRateCondition> { }
            };
            var arrangementRequest = Utility.GetArangementRequest(conditions);

            Application application = new Application
            {
                ArrangementNumber = "1",
                CustomerName = "Tester",
                StatusInformation = new StatusInformation
                {
                    Description = "Works as a tester"
                },
                CustomerSegment = "student",
                CollateralModel = "two-co-debtors",
                
                RiskScore = 55,
                ChannelCode = "web",
                RequestDate = DateTime.Now
            };

            var result = priceCalculator.CalculatePrice(application, arrangementRequest);

            //Basic assertions
            Assert.Equal("RanToCompletion", result.Status.ToString());

            Assert.Single(result.Result.Conditions.Fees);
            var resultFees = result.Result.Conditions.Fees[0];

            Assert.Single(resultFees.Variations);
            var variation = resultFees.Variations[0];

            //Asserting input
            Assert.Equal(20, resultFees.FixedAmount.Amount);
            Assert.Equal((decimal)0.1, resultFees.Percentage);

            //Asserting output
            Assert.Equal("product/price-variations/iterative-fee-in-range-upper.dmn", resultFees.VariationsDefinitionDMN);
            Assert.Equal((decimal)0.98, resultFees.CalculatedPercentage);
            Assert.Equal((decimal)0.88, variation.Percentage);
            Assert.Equal(198, resultFees.CalculatedFixedAmount);
            Assert.Equal(178, variation.FixedAmount);
        }

        [Fact]
        //Percentage and FixedAmount have a variation that sets them to near lower limit but still above
        public void IterativeCalculatedNearLowerLimit()
        {
            ConfigurationServiceMock configurationService = new ConfigurationServiceMock();
            PriceCalculationDMNServiceMock priceCalCService = new PriceCalculationDMNServiceMock(configurationService);
            MarketRatesServiceMock marketRatesService = new MarketRatesServiceMock();
            var priceCalculator = new OfferPriceCalculation(priceCalCService, marketRatesService, configurationService, null);
            FeeCondition regularFee = new FeeCondition
            {
                //Samo % se testira za sada jer trenutno nema za fixedAmount
                //Samo varijacije uticu na PercentageKind = FeeConditionKinds.OriginationFee,
                Percentage = (decimal)0.1,
                FixedAmount = new Currency { Amount = 200, Code = "EUR" },
                LowerLimit = new Currency { Amount = 20, Code = "EUR" },
                UpperLimit = new Currency { Amount = 200, Code = "EUR" },
                VariationsDefinitionDMN = "product/price-variations/iterative-fee-in-range-lower.dmn",
                PercentageLowerLimit = (decimal)0.01,
                PercentageUpperLimit = 1,
                Title = "Origination-fee1",
                EffectiveDate = DateTime.Now.AddDays(-1),
                Currencies = new List<string> { "RSD", "EUR" }
            };

            ProductConditions conditions = new ProductConditions
            {
                Fees = new List<FeeCondition> { regularFee },
                InterestRates = new List<InterestRateCondition> { }
            };
            var arrangementRequest = Utility.GetArangementRequest(conditions);

            Application application = new Application
            {
                ArrangementNumber = "1",
                CustomerName = "Tester",
                StatusInformation = new StatusInformation
                {
                    Description = "Works as a tester"
                },
                CustomerSegment = "student",
                CollateralModel = "two-co-debtors",
                
                RiskScore = 55,
                ChannelCode = "web",
                RequestDate = DateTime.Now
            };

            var result = priceCalculator.CalculatePrice(application, arrangementRequest);

            //Basic assertions
            Assert.Equal("RanToCompletion", result.Status.ToString());

            Assert.Single(result.Result.Conditions.Fees);
            var resultFees = result.Result.Conditions.Fees[0];

            Assert.Single(resultFees.Variations);
            var variation = resultFees.Variations[0];


            //Asserting input
            Assert.Equal(200, resultFees.FixedAmount.Amount);
            Assert.Equal((decimal)0.1, resultFees.Percentage);

            //Asserting output
            Assert.Equal("product/price-variations/iterative-fee-in-range-lower.dmn", resultFees.VariationsDefinitionDMN);
            Assert.Equal((decimal)0.03, resultFees.CalculatedPercentage);
            Assert.Equal((decimal)-0.07, variation.Percentage);
            Assert.Equal(22, resultFees.CalculatedFixedAmount);
            Assert.Equal(-178, variation.FixedAmount);
        }

        [Fact]
        //Percentage and FixedAmount have a variation that sets them under lower limit and calculated should be equal to lower limit
        public void IterativeCalculatedUnderLimit()
        {
            ConfigurationServiceMock configurationService = new ConfigurationServiceMock();
            PriceCalculationDMNServiceMock priceCalCService = new PriceCalculationDMNServiceMock(configurationService);
            MarketRatesServiceMock marketRatesService = new MarketRatesServiceMock();
            var priceCalculator = new OfferPriceCalculation(priceCalCService, marketRatesService, configurationService, null);
            FeeCondition regularFee = new FeeCondition
            {
                Percentage = (decimal)0.1,
                FixedAmount = new Currency { Amount = 200, Code = "EUR" },
                LowerLimit = new Currency { Amount = 20, Code = "EUR" },
                UpperLimit = new Currency { Amount = 300, Code = "EUR" },
                VariationsDefinitionDMN = "product/price-variations/iterative-fee-under-range.dmn",
                PercentageLowerLimit = (decimal)0.05,
                PercentageUpperLimit = 1,
                Title = "Origination-fee1",
                EffectiveDate = DateTime.Now.AddDays(-1),
                Currencies = new List<string> { "RSD", "EUR" }
            };

            ProductConditions conditions = new ProductConditions
            {
                Fees = new List<FeeCondition> { regularFee },
                InterestRates = new List<InterestRateCondition> { }
            };

            var arrangementRequest = Utility.GetArangementRequest(conditions);

            Application application = new Application
            {
                ArrangementNumber = "1",
                CustomerName = "Tester",
                StatusInformation = new StatusInformation
                {
                    Description = "Works as a tester"
                },
                CustomerSegment = "student",
                CollateralModel = "two-co-debtors",
                
                RiskScore = 55,
                ChannelCode = "web",
                RequestDate = DateTime.Now
            };

            var result = priceCalculator.CalculatePrice(application, arrangementRequest);

            //Basic assertions
            Assert.Equal("RanToCompletion", result.Status.ToString());

            Assert.Single(result.Result.Conditions.Fees);
            var resultFees = result.Result.Conditions.Fees[0];

            Assert.Single(resultFees.Variations);
            var variation = resultFees.Variations[0];


            //Asserting input
            Assert.Equal(200, resultFees.FixedAmount.Amount);
            Assert.Equal((decimal)0.1, resultFees.Percentage);

            //Asserting output
            Assert.Equal("product/price-variations/iterative-fee-under-range.dmn", resultFees.VariationsDefinitionDMN);
            Assert.Equal((decimal)0.05, resultFees.CalculatedPercentage);
            Assert.Equal((decimal)-0.09, variation.Percentage);
            Assert.Equal(20, resultFees.CalculatedFixedAmount);
            Assert.Equal(-180, variation.FixedAmount);

        }

        [Fact]
        //Percentage and FixedAmount have a variation that sets them over upper limit and calculated should be equal to upper limit
        public void IterativeCalculatedOverLimit()
         {
            ConfigurationServiceMock configurationService = new ConfigurationServiceMock();
            PriceCalculationDMNServiceMock priceCalCService = new PriceCalculationDMNServiceMock(configurationService);
            MarketRatesServiceMock marketRatesService = new MarketRatesServiceMock();
            var priceCalculator = new OfferPriceCalculation(priceCalCService, marketRatesService, configurationService, null);
            FeeCondition regularFee = new FeeCondition
            {
                Percentage = (decimal)0.1,
                FixedAmount = new Currency { Amount = 20, Code = "EUR" },
                LowerLimit = new Currency { Amount = 20, Code = "EUR" },
                UpperLimit = new Currency { Amount = 200, Code = "EUR" },
                VariationsDefinitionDMN = "product/price-variations/iterative-fee-over-range.dmn",
                PercentageLowerLimit = (decimal)0.01,
                PercentageUpperLimit = 1,
                Title = "Origination-fee1",
                EffectiveDate = DateTime.Now.AddDays(-1),
                Currencies = new List<string> { "RSD", "EUR" }
            };

            ProductConditions conditions = new ProductConditions
            {
                Fees = new List<FeeCondition> { regularFee },
                InterestRates = new List<InterestRateCondition> { }
            };
            var arrangementRequest = Utility.GetArangementRequest(conditions);

            Application application = new Application
            {
                ArrangementNumber = "1",
                CustomerName = "Tester",
                StatusInformation = new StatusInformation
                {
                    Description = "Works as a tester"
                },
                CustomerSegment = "student",
                CollateralModel = "two-co-debtors",
                
                RiskScore = 55,
                ChannelCode = "web",
                RequestDate = DateTime.Now
            };

            var result = priceCalculator.CalculatePrice(application, arrangementRequest);

            //Basic assertions
            Assert.Equal("RanToCompletion", result.Status.ToString());

            Assert.Single(result.Result.Conditions.Fees);
            var resultFees = result.Result.Conditions.Fees[0];

            Assert.Single(resultFees.Variations);
            var variation = resultFees.Variations[0];


            //Asserting input
            Assert.Equal(20, resultFees.FixedAmount.Amount);
            Assert.Equal((decimal)0.1, resultFees.Percentage);

            //Asserting output
            Assert.Equal("product/price-variations/iterative-fee-over-range.dmn", resultFees.VariationsDefinitionDMN);
            Assert.Equal(1, resultFees.CalculatedPercentage);
            Assert.Equal((decimal)1.01, variation.Percentage);
            Assert.Equal(202, resultFees.CalculatedFixedAmount);
            Assert.Equal(182, variation.FixedAmount);

        }

        [Fact]
        //Percentage and FixedAmount have two variations that sets them near upper limit and calculated should be under upper limit
        public void IterativeCalculatedNearUpperLimitMultipleVariations()
         {
            ConfigurationServiceMock configurationService = new ConfigurationServiceMock();
            PriceCalculationDMNServiceMock priceCalCService = new PriceCalculationDMNServiceMock(configurationService);
            MarketRatesServiceMock marketRatesService = new MarketRatesServiceMock();
            var priceCalculator = new OfferPriceCalculation(priceCalCService, marketRatesService, configurationService, null);
            FeeCondition regularFee = new FeeCondition
            {
                Percentage = (decimal)0.1,
                FixedAmount = new Currency { Amount = 20, Code = "EUR" },
                LowerLimit = new Currency { Amount = 20, Code = "EUR" },
                UpperLimit = new Currency { Amount = 200, Code = "EUR" },
                VariationsDefinitionDMN = "product/price-variations/iterative-multiple-fee-var-in-range-upper.dmn",
                PercentageLowerLimit = (decimal)0.01,
                PercentageUpperLimit = 1,
                Title = "Origination-fee1",
                EffectiveDate = DateTime.Now.AddDays(-1),
                Currencies = new List<string> { "RSD", "EUR" }
            };

            ProductConditions conditions = new ProductConditions
                {
                    Fees = new List<FeeCondition> { regularFee },
                InterestRates = new List<InterestRateCondition> { }
            };
            var arrangementRequest = Utility.GetArangementRequest(conditions);

            Application application = new Application
            {
                ArrangementNumber = "1",
                CustomerName = "Tester",
                StatusInformation = new StatusInformation
                {
                    Description = "Works as a tester"
                },
                CustomerSegment = "student",
                CollateralModel = "two-co-debtors",
                
                RiskScore = 55,
                ChannelCode = "web",
                RequestDate = DateTime.Now
            };

            var result = priceCalculator.CalculatePrice(application, arrangementRequest);

            //Basic assertions
            Assert.Equal("RanToCompletion", result.Status.ToString());
            Assert.Single(result.Result.Conditions.Fees);

            var resultFees = result.Result.Conditions.Fees[0];

            Assert.Equal("2", resultFees.Variations.Count.ToString());
            var variationOne = resultFees.Variations[0];
            var variationTwo = resultFees.Variations[1];

            //Asserting input
            Assert.Equal(20, resultFees.FixedAmount.Amount);
            Assert.Equal((decimal)0.1, resultFees.Percentage);

            //Asserting output
            Assert.Equal("product/price-variations/iterative-multiple-fee-var-in-range-upper.dmn", resultFees.VariationsDefinitionDMN);
            Assert.Equal((decimal)0.98, resultFees.CalculatedPercentage);
            Assert.Equal((decimal)0.48, variationOne.Percentage);
            Assert.Equal((decimal)0.40, variationTwo.Percentage);
            Assert.Equal(198, resultFees.CalculatedFixedAmount);
            Assert.Equal(100, variationOne.FixedAmount);
            Assert.Equal(78, variationTwo.FixedAmount);

        }

        [Fact]
        //Percentage and FixedAmount have two variations that sets them near lower limit and calculated should be above lower limit
        public void IterativeCalculatedNearLowerLimitMultipleVariations()
        {
            ConfigurationServiceMock configurationService = new ConfigurationServiceMock();
            PriceCalculationDMNServiceMock priceCalCService = new PriceCalculationDMNServiceMock(configurationService);
            MarketRatesServiceMock marketRatesService = new MarketRatesServiceMock();
            var priceCalculator = new OfferPriceCalculation(priceCalCService, marketRatesService, configurationService, null);
            FeeCondition regularFee = new FeeCondition
            {
                Percentage = (decimal)0.1,
                FixedAmount = new Currency { Amount = 200, Code = "EUR" },
                LowerLimit = new Currency { Amount = 20, Code = "EUR" },
                UpperLimit = new Currency { Amount = 300, Code = "EUR" },
                VariationsDefinitionDMN = "product/price-variations/iterative-multiple-fee-var-in-range-lower.dmn",
                PercentageLowerLimit = (decimal)0.01,
                PercentageUpperLimit = 1,
                Title = "Origination-fee1",
                EffectiveDate = DateTime.Now.AddDays(-1),
                Currencies = new List<string> { "RSD", "EUR" }
            };

            ProductConditions conditions = new ProductConditions
            {
                Fees = new List<FeeCondition> { regularFee },
                InterestRates = new List<InterestRateCondition> { }
            };
            var arrangementRequest = Utility.GetArangementRequest(conditions);

            Application application = new Application
            {
                ArrangementNumber = "1",
                CustomerName = "Tester",
                StatusInformation = new StatusInformation
                {
                    Description = "Works as a tester"
                },
                CustomerSegment = "student",
                CollateralModel = "two-co-debtors",
                
                RiskScore = 55,
                ChannelCode = "web",
                RequestDate = DateTime.Now
            };

            var result = priceCalculator.CalculatePrice(application, arrangementRequest);

            //Basic assertions
            Assert.Equal("RanToCompletion", result.Status.ToString());
            Assert.Single(result.Result.Conditions.Fees);

            var resultFees = result.Result.Conditions.Fees[0];

            Assert.Equal("2", resultFees.Variations.Count.ToString());
            var variationOne = resultFees.Variations[0];
            var variationTwo = resultFees.Variations[1];

            //Asserting input
            Assert.Equal(200, resultFees.FixedAmount.Amount);
            Assert.Equal((decimal)0.1, resultFees.Percentage);

            //Asserting output
            Assert.Equal("product/price-variations/iterative-multiple-fee-var-in-range-lower.dmn", resultFees.VariationsDefinitionDMN);
            Assert.Equal((decimal)0.03, resultFees.CalculatedPercentage);
            Assert.Equal((decimal)-0.03, variationOne.Percentage);
            Assert.Equal((decimal)-0.04, variationTwo.Percentage);
            Assert.Equal(22, resultFees.CalculatedFixedAmount);
            Assert.Equal(-100, variationOne.FixedAmount);
            Assert.Equal(-78, variationTwo.FixedAmount);
        }

        [Fact]
        //Percentage and FixedAmount have two variations that sets them under lower limit and calculated should be equal to lower limit
        public void IterativeCalculatedUnderLowerLimitMultipleVariations()
        {
            ConfigurationServiceMock configurationService = new ConfigurationServiceMock();
            PriceCalculationDMNServiceMock priceCalCService = new PriceCalculationDMNServiceMock(configurationService);
            MarketRatesServiceMock marketRatesService = new MarketRatesServiceMock();
            var priceCalculator = new OfferPriceCalculation(priceCalCService, marketRatesService, configurationService, null);
            FeeCondition regularFee = new FeeCondition
            {
                Percentage = (decimal)0.1,
                FixedAmount = new Currency { Amount = 200, Code = "EUR" },
                LowerLimit = new Currency { Amount = 20, Code = "EUR" },
                UpperLimit = new Currency { Amount = 300, Code = "EUR" },
                VariationsDefinitionDMN = "product/price-variations/iterative-multiple-fee-var-under-range.dmn",
                PercentageLowerLimit = (decimal)0.05,
                PercentageUpperLimit = 1,
                Title = "Origination-fee1",
                EffectiveDate = DateTime.Now.AddDays(-1),
                Currencies = new List<string> { "RSD", "EUR" }
            };

            ProductConditions conditions = new ProductConditions
            {
                Fees = new List<FeeCondition> { regularFee },
                InterestRates = new List<InterestRateCondition> { }
            };
            var arrangementRequest = Utility.GetArangementRequest(conditions);

            Application application = new Application
            {
                ArrangementNumber = "1",
                CustomerName = "Tester",
                StatusInformation = new StatusInformation
                {
                    Description = "Works as a tester"
                },
                CustomerSegment = "student",
                CollateralModel = "two-co-debtors",
                
                RiskScore = 55,
                ChannelCode = "web",
                RequestDate = DateTime.Now
            };

            var result = priceCalculator.CalculatePrice(application, arrangementRequest);

            //Basic assertions
            Assert.Equal("RanToCompletion", result.Status.ToString());
            Assert.Single(result.Result.Conditions.Fees);

            var resultFees = result.Result.Conditions.Fees[0];

            Assert.Equal("2", resultFees.Variations.Count.ToString());
            var variationOne = resultFees.Variations[0];
            var variationTwo = resultFees.Variations[1];

            //Asserting input
            Assert.Equal(200, resultFees.FixedAmount.Amount);
            Assert.Equal((decimal)0.1, resultFees.Percentage);

            //Asserting output           
            Assert.Equal("product/price-variations/iterative-multiple-fee-var-under-range.dmn", resultFees.VariationsDefinitionDMN);
            Assert.Equal((decimal)0.05, resultFees.CalculatedPercentage);
            Assert.Equal((decimal)-0.07, variationOne.Percentage);
            Assert.Equal((decimal)-0.04, variationTwo.Percentage);
            Assert.Equal(18, resultFees.CalculatedFixedAmount);
            Assert.Equal(-101, variationOne.FixedAmount);
            Assert.Equal(-81, variationTwo.FixedAmount);

        }

        [Fact]
        //Percentage and FixedAmount have two variations that sets them over upper limit and calculated should be equal to upper limit
        public void IterativeCalculatedOverUpperLimitMultipleVariations()
        {ConfigurationServiceMock configurationService = new ConfigurationServiceMock();
        PriceCalculationDMNServiceMock priceCalCService = new PriceCalculationDMNServiceMock(configurationService);
        MarketRatesServiceMock marketRatesService = new MarketRatesServiceMock();
            var priceCalculator = new OfferPriceCalculation(priceCalCService, marketRatesService, configurationService, null);
            FeeCondition regularFee = new FeeCondition
            {
                Percentage = (decimal)0.1,
                FixedAmount = new Currency { Amount = 20, Code = "EUR" },
                LowerLimit = new Currency { Amount = 20, Code = "EUR" },
                UpperLimit = new Currency { Amount = 200, Code = "EUR" },
                VariationsDefinitionDMN = "product/price-variations/iterative-multiple-fee-var-over-range.dmn",
                PercentageLowerLimit = (decimal)0.05,
                PercentageUpperLimit = 1,
                Title = "Origination-fee1",
                EffectiveDate = DateTime.Now.AddDays(-1),
                Currencies = new List<string> { "RSD", "EUR" }
            };

            ProductConditions conditions = new ProductConditions
            {
                Fees = new List<FeeCondition> { regularFee },
                InterestRates = new List<InterestRateCondition> { }
            };
            var arrangementRequest = Utility.GetArangementRequest(conditions);

            Application application = new Application
            {
                ArrangementNumber = "1",
                CustomerName = "Tester",
                StatusInformation = new StatusInformation
                {
                    Description = "Works as a tester"
                },
                CustomerSegment = "student",
                CollateralModel = "two-co-debtors",
                
                RiskScore = 55,
                ChannelCode = "web",
                RequestDate = DateTime.Now
            };

            var result = priceCalculator.CalculatePrice(application, arrangementRequest);
            var resultFees = result.Result.Conditions.Fees[0];

            var variation = result.Result.Conditions.Fees[0].Variations;

            Assert.Equal("2", variation.Count.ToString());

            var variationOne = variation[0];
            var variationTwo = variation[1];

            Assert.Equal("RanToCompletion", result.Status.ToString());
            Assert.Equal(1, resultFees.CalculatedPercentage);
            Assert.Equal((decimal)0.5, variationOne.Percentage);
            Assert.Equal((decimal)0.6, variationTwo.Percentage);
            Assert.Equal("product/price-variations/iterative-multiple-fee-var-over-range.dmn", resultFees.VariationsDefinitionDMN);
        }
    }
}
