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
    public class FeeVariationsTests
    {
        [Fact]
        //Percentage and FixedAmount have a variation that sets them to near upper limit but still below
        public void CalculatedNearUpperLimit()
        {
            ConfigurationServiceMock configurationService = new ConfigurationServiceMock();
            PriceCalculationDMNServiceMock priceCalCService = new PriceCalculationDMNServiceMock(configurationService);
            MarketRatesServiceMock marketRatesService = new MarketRatesServiceMock();
            var OfferPriceCalculation = new OfferPriceCalculation(priceCalCService, marketRatesService, configurationService, null);
            FeeCondition regularFee = new FeeCondition
            {
                Percentage = (decimal)0.1,
                FixedAmount = new Currency { Amount = 20, Code = "EUR" },
                LowerLimit = new Currency { Amount = 20, Code = "EUR" },
                UpperLimit = new Currency { Amount = 200, Code = "EUR" },
                VariationsDefinitionDMN = "product/price-variations/fee-in-range-upper.dmn",
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

            Application application = Utility.GetApplication(conditions);
            var arrangementRequest = application.ArrangementRequests[0] as TermLoanRequest;
            var priceCalculationConditions = Utility.GetPriceCalculationParameterFromTermLoanRequest(arrangementRequest);

            
            var result = OfferPriceCalculation.CalculatePrice(application, arrangementRequest);

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
            Assert.Equal("product/price-variations/fee-in-range-upper.dmn", resultFees.VariationsDefinitionDMN);
            Assert.Equal((decimal)0.99, resultFees.CalculatedPercentage);
            Assert.Equal((decimal)0.89, variation.Percentage);
            Assert.Equal(199, resultFees.CalculatedFixedAmount);
            Assert.Equal(179, variation.FixedAmount);
        }

        [Fact]
        //Percentage and FixedAmount have a variation that sets them to near lower limit but still above
        public void CalculatedNearLowerLimit()
        {
            ConfigurationServiceMock configurationService = new ConfigurationServiceMock();
            PriceCalculationDMNServiceMock priceCalCService = new PriceCalculationDMNServiceMock(configurationService);
            MarketRatesServiceMock marketRatesService = new MarketRatesServiceMock();
            var OfferPriceCalculation = new OfferPriceCalculation(priceCalCService, marketRatesService, configurationService, null);
            FeeCondition regularFee = new FeeCondition
            {
                //Samo % se testira za sada jer trenutno nema za fixedAmount
                //Samo varijacije uticu na PercentageKind = FeeConditionKinds.OriginationFee,
                Percentage = (decimal)0.1,
                FixedAmount = new Currency { Amount = 200, Code = "EUR" },
                LowerLimit = new Currency { Amount = 20, Code = "EUR" },
                UpperLimit = new Currency { Amount = 200, Code = "EUR" },
                VariationsDefinitionDMN = "product/price-variations/fee-in-range-lower.dmn",
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

            Application application = Utility.GetApplication(conditions);
            var arrangementRequest = application.ArrangementRequests[0] as TermLoanRequest;
            var priceCalculationConditions = Utility.GetPriceCalculationParameterFromTermLoanRequest(arrangementRequest);
          
            
            var result = OfferPriceCalculation.CalculatePrice(application, arrangementRequest);

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
            Assert.Equal("product/price-variations/fee-in-range-lower.dmn", resultFees.VariationsDefinitionDMN);
            Assert.Equal((decimal)0.02, resultFees.CalculatedPercentage);
            Assert.Equal((decimal)-0.08, variation.Percentage);
            Assert.Equal(21, resultFees.CalculatedFixedAmount);
            Assert.Equal(-179, variation.FixedAmount);
        }

        [Fact]
        //Percentage and FixedAmount have a variation that sets them under lower limit and calculated should be equal to lower limit
        public void CalculatedUnderLimit()
        {
            ConfigurationServiceMock configurationService = new ConfigurationServiceMock();
            PriceCalculationDMNServiceMock priceCalCService = new PriceCalculationDMNServiceMock(configurationService);
            MarketRatesServiceMock marketRatesService = new MarketRatesServiceMock();
            var OfferPriceCalculation = new OfferPriceCalculation(priceCalCService, marketRatesService, configurationService, null);
            FeeCondition regularFee = new FeeCondition
            {
                Percentage = (decimal)0.1,
                FixedAmount = new Currency { Amount = 200, Code = "EUR" },
                LowerLimit = new Currency { Amount = 20, Code = "EUR" },
                UpperLimit = new Currency { Amount = 300, Code = "EUR" },
                VariationsDefinitionDMN = "product/price-variations/fee-under-range.dmn",
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
            Application application = Utility.GetApplication(conditions);
            var arrangementRequest = application.ArrangementRequests[0] as TermLoanRequest;
            var priceCalculationConditions = Utility.GetPriceCalculationParameterFromTermLoanRequest(arrangementRequest);
            

            
            var result = OfferPriceCalculation.CalculatePrice(application, arrangementRequest);

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
            Assert.Equal("product/price-variations/fee-under-range.dmn", resultFees.VariationsDefinitionDMN);
            Assert.Equal((decimal)0.05, resultFees.CalculatedPercentage);
            Assert.Equal((decimal)-0.1, variation.Percentage);
            Assert.Equal((decimal)19.0, resultFees.CalculatedFixedAmount);
            Assert.Equal(-181, variation.FixedAmount);

        }

        [Fact]
        //Percentage and FixedAmount have a variation that sets them over upper limit and calculated should be equal to upper limit
        public void CalculatedOverLimit()
         {
            ConfigurationServiceMock configurationService = new ConfigurationServiceMock();
            PriceCalculationDMNServiceMock priceCalCService = new PriceCalculationDMNServiceMock(configurationService);
            MarketRatesServiceMock marketRatesService = new MarketRatesServiceMock();
            var OfferPriceCalculation = new OfferPriceCalculation(priceCalCService, marketRatesService, configurationService, null);
            FeeCondition regularFee = new FeeCondition
            {
                Percentage = (decimal)0.1,
                FixedAmount = new Currency { Amount = 20, Code = "EUR" },
                LowerLimit = new Currency { Amount = 20, Code = "EUR" },
                UpperLimit = new Currency { Amount = 200, Code = "EUR" },
                VariationsDefinitionDMN = "product/price-variations/fee-over-range.dmn",
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
            Application application = Utility.GetApplication(conditions);
            var arrangementRequest = application.ArrangementRequests[0] as TermLoanRequest;
            var priceCalculationConditions = Utility.GetPriceCalculationParameterFromTermLoanRequest(arrangementRequest);

          
            
            var result = OfferPriceCalculation.CalculatePrice(application, arrangementRequest);

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
            Assert.Equal("product/price-variations/fee-over-range.dmn", resultFees.VariationsDefinitionDMN);
            Assert.Equal(1, resultFees.CalculatedPercentage);
            Assert.Equal(1, variation.Percentage);
            Assert.Equal((decimal)201.0, resultFees.CalculatedFixedAmount);
            Assert.Equal(181, variation.FixedAmount);

        }

        [Fact]
        //Percentage and FixedAmount have two variations that sets them near upper limit and calculated should be under upper limit
        public void CalculatedNearUpperLimitMultipleVariations()
        {
            ConfigurationServiceMock configurationService = new ConfigurationServiceMock();
            PriceCalculationDMNServiceMock priceCalCService = new PriceCalculationDMNServiceMock(configurationService);
            MarketRatesServiceMock marketRatesService = new MarketRatesServiceMock();
            var OfferPriceCalculation = new OfferPriceCalculation(priceCalCService, marketRatesService, configurationService, null);
            FeeCondition regularFee = new FeeCondition
            {
                Percentage = (decimal)0.1,
                FixedAmount = new Currency { Amount = 20, Code = "EUR" },
                LowerLimit = new Currency { Amount = 20, Code = "EUR" },
                UpperLimit = new Currency { Amount = 200, Code = "EUR" },
                VariationsDefinitionDMN = "product/price-variations/multiple-fee-var-in-range-upper.dmn",
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
            Application application = Utility.GetApplication(conditions);
            var arrangementRequest = application.ArrangementRequests[0] as TermLoanRequest;
           
            var priceCalculationConditions = Utility.GetPriceCalculationParameterFromTermLoanRequest(arrangementRequest);

            
            var result = OfferPriceCalculation.CalculatePrice(application, arrangementRequest);

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
            Assert.Equal("product/price-variations/multiple-fee-var-in-range-upper.dmn", resultFees.VariationsDefinitionDMN);
            Assert.Equal((decimal)0.99, resultFees.CalculatedPercentage);
            Assert.Equal((decimal)0.49, variationOne.Percentage);
            Assert.Equal((decimal)0.40, variationTwo.Percentage);
            Assert.Equal(199, resultFees.CalculatedFixedAmount);
            Assert.Equal(100, variationOne.FixedAmount);
            Assert.Equal(79, variationTwo.FixedAmount);

        }

        [Fact]
        //Percentage and FixedAmount have two variations that sets them near lower limit and calculated should be above lower limit
        public void CalculatedNearLowerLimitMultipleVariations()
        {
            ConfigurationServiceMock configurationService = new ConfigurationServiceMock();
            PriceCalculationDMNServiceMock priceCalCService = new PriceCalculationDMNServiceMock(configurationService);
            MarketRatesServiceMock marketRatesService = new MarketRatesServiceMock();
            var OfferPriceCalculation = new OfferPriceCalculation(priceCalCService, marketRatesService, configurationService, null);
            FeeCondition regularFee = new FeeCondition
            {
                Percentage = (decimal)0.1,
                FixedAmount = new Currency { Amount = 200, Code = "EUR" },
                LowerLimit = new Currency { Amount = 20, Code = "EUR" },
                UpperLimit = new Currency { Amount = 300, Code = "EUR" },
                VariationsDefinitionDMN = "product/price-variations/multiple-fee-var-in-range-lower.dmn",
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
            Application application = Utility.GetApplication(conditions);
            var arrangementRequest = application.ArrangementRequests[0] as TermLoanRequest;
            var priceCalculationConditions = Utility.GetPriceCalculationParameterFromTermLoanRequest(arrangementRequest);
            

            var result = OfferPriceCalculation.CalculatePrice(application, arrangementRequest);

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
            Assert.Equal("product/price-variations/multiple-fee-var-in-range-lower.dmn", resultFees.VariationsDefinitionDMN);
            Assert.Equal((decimal)0.02, resultFees.CalculatedPercentage);
            Assert.Equal((decimal)-0.03, variationOne.Percentage);
            Assert.Equal((decimal)-0.05, variationTwo.Percentage);
            Assert.Equal(21, resultFees.CalculatedFixedAmount);
            Assert.Equal(-100, variationOne.FixedAmount);
            Assert.Equal(-79, variationTwo.FixedAmount);
        }

        [Fact]
        //Percentage and FixedAmount have two variations that sets them under lower limit and calculated should be equal to lower limit
        public void CalculatedUnderLowerLimitMultipleVariations()
        {
            ConfigurationServiceMock configurationService = new ConfigurationServiceMock();
            PriceCalculationDMNServiceMock priceCalCService = new PriceCalculationDMNServiceMock(configurationService);
            MarketRatesServiceMock marketRatesService = new MarketRatesServiceMock();
            var OfferPriceCalculation = new OfferPriceCalculation(priceCalCService, marketRatesService, configurationService, null);
            FeeCondition regularFee = new FeeCondition
            {
                Percentage = (decimal)0.1,
                FixedAmount = new Currency { Amount = 200, Code = "EUR" },
                LowerLimit = new Currency { Amount = 20, Code = "EUR" },
                UpperLimit = new Currency { Amount = 300, Code = "EUR" },
                VariationsDefinitionDMN = "product/price-variations/multiple-fee-var-under-range.dmn",
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
            Application application = Utility.GetApplication(conditions);
            var arrangementRequest = application.ArrangementRequests[0] as TermLoanRequest;
            var priceCalculationConditions = Utility.GetPriceCalculationParameterFromTermLoanRequest(arrangementRequest);
            
            
            var result = OfferPriceCalculation.CalculatePrice(application, arrangementRequest);

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
            Assert.Equal("product/price-variations/multiple-fee-var-under-range.dmn", resultFees.VariationsDefinitionDMN);
            Assert.Equal((decimal)0.05, resultFees.CalculatedPercentage);
            Assert.Equal((decimal)-0.06, variationOne.Percentage);
            Assert.Equal((decimal)-0.04, variationTwo.Percentage);
            Assert.Equal((decimal)19.0, resultFees.CalculatedFixedAmount);
            Assert.Equal(-100, variationOne.FixedAmount);
            Assert.Equal(-81, variationTwo.FixedAmount);

        }

        [Fact]
        //Percentage and FixedAmount have two variations that sets them over upper limit and calculated should be equal to upper limit
        public void CalculatedOverUpperLimitMultipleVariations()
        {
            ConfigurationServiceMock configurationService = new ConfigurationServiceMock();
            PriceCalculationDMNServiceMock priceCalCService = new PriceCalculationDMNServiceMock(configurationService);
            MarketRatesServiceMock marketRatesService = new MarketRatesServiceMock();
            var OfferPriceCalculation = new OfferPriceCalculation(priceCalCService, marketRatesService, configurationService, null);
            FeeCondition regularFee = new FeeCondition
            {
                Percentage = (decimal)0.1,
                FixedAmount = new Currency { Amount = 20, Code = "EUR" },
                LowerLimit = new Currency { Amount = 20, Code = "EUR" },
                UpperLimit = new Currency { Amount = 200, Code = "EUR" },
                VariationsDefinitionDMN = "product/price-variations/multiple-fee-var-over-range.dmn",
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
            Application application = Utility.GetApplication(conditions);
            var arrangementRequest = application.ArrangementRequests[0] as TermLoanRequest;
            var priceCalculationConditions = Utility.GetPriceCalculationParameterFromTermLoanRequest(arrangementRequest);

            
            var result = OfferPriceCalculation.CalculatePrice(application, arrangementRequest);
            var resultFees = result.Result.Conditions.Fees[0];
            var variation = result.Result.Conditions.Fees[0].Variations;

            Assert.Equal("2", variation.Count.ToString());

            var variationOne = variation[0];
            var variationTwo = variation[1];

            Assert.Equal("RanToCompletion", result.Status.ToString());
            Assert.Equal(1, resultFees.CalculatedPercentage);
            Assert.Equal((decimal)0.4, variationOne.Percentage);
            Assert.Equal((decimal)0.6, variationTwo.Percentage);
            Assert.Equal("product/price-variations/multiple-fee-var-over-range.dmn", resultFees.VariationsDefinitionDMN);
        }
    }
}
