using System;
using Xunit;
using Offer.Domain.Calculations;
using Offer.Domain.AggregatesModel.ApplicationAggregate;
using PriceCalculation.Models.Pricing;
using System.Collections.Generic;
using PriceCalculation.Models.Product;

namespace OfferApiTest.Tests
{
    public class NoVariationInterestRateLimitTests
    {
        [Fact]
        //No variations, calc rates should be as they are set in the request
        //Rate is within limits
        public void NoVariationsRateInLimits()
        {
            ConfigurationServiceMock configurationService = new ConfigurationServiceMock();
            PriceCalculationDMNServiceMock priceCalCService = new PriceCalculationDMNServiceMock(configurationService);
            MarketRatesServiceMock marketRatesService = new MarketRatesServiceMock();
            var OfferPriceCalculation = new OfferPriceCalculation(priceCalCService, marketRatesService, configurationService, null);
            InterestRateCondition regularInterestEuribor = new InterestRateCondition
            {
                Kind = InterestRateKinds.RegularInterest,
                Rate = new InterestRate
                {
                    BaseRateId = "EURIBOR-6M",
                    SpreadRateValue = (decimal)10.1
                },
                LowerLimit = new InterestRate
                {
                    BaseRateId = "EURIBOR-6M",
                    SpreadRateValue = 10
                },
                UpperLimit = new InterestRate
                {
                    BaseRateId = "EURIBOR-6M",
                    SpreadRateValue = 18
                },
                Title = "regular-interest",
                Currencies = new List<string> { "EUR" }
            };

            ProductConditions conditions = new ProductConditions
            {
                Fees = new List<FeeCondition> { },
                InterestRates = new List<InterestRateCondition> { regularInterestEuribor }
            };
            Application application = Utility.GetApplication(conditions);
            var arrangementRequest = application.ArrangementRequests[0] as TermLoanRequest;
            var priceCalculationConditions = Utility.GetPriceCalculationParameterFromTermLoanRequest(arrangementRequest);

            var variationsDefinition = Utility.GetVariationDefinitionParamsFromApplication(application);
            var result = OfferPriceCalculation.CalculatePrice(application, arrangementRequest);
            var resultInterest = result.Result.Conditions.InterestRates[0];

            //Basic assertions
            Assert.Equal("RanToCompletion", result.Status.ToString());
            Assert.Equal("0", resultInterest.Variations.Count.ToString());

            //Asserting input parameters
            Assert.Equal("EURIBOR-6M", resultInterest.Rate.BaseRateId);
            Assert.Equal(0, resultInterest.Rate.BaseRateValue);
            Assert.Equal((decimal)10.1, resultInterest.Rate.SpreadRateValue);

            Assert.Equal("EURIBOR-6M", resultInterest.UpperLimit.BaseRateId);
            Assert.Equal(0, resultInterest.UpperLimit.BaseRateValue);
            Assert.Equal((decimal)18.0, resultInterest.UpperLimit.SpreadRateValue);

            Assert.Equal("EURIBOR-6M", resultInterest.LowerLimit.BaseRateId);
            Assert.Equal(0, resultInterest.LowerLimit.BaseRateValue);
            Assert.Equal((decimal)10.0, resultInterest.LowerLimit.SpreadRateValue);

            //Asserting Napr and calculation results
            Assert.Equal((decimal)10.1, ((TermLoanRequest)result.Result).Napr);
            Assert.Equal((decimal)10.1, resultInterest.CalculatedRate);
            Assert.Equal((decimal)10.0, resultInterest.CalculatedLowerLimit);
            Assert.Equal((decimal)18.0, resultInterest.CalculatedUpperLimit);
        }

        [Fact]
        //No variations, calc rates should be as they are set in the request
        //Rate is within limits
        public void NoVariationsBelowLimitWithoutMarketRate()
        {
            ConfigurationServiceMock configurationService = new ConfigurationServiceMock();
            PriceCalculationDMNServiceMock priceCalCService = new PriceCalculationDMNServiceMock(configurationService);
            MarketRatesServiceMock marketRatesService = new MarketRatesServiceMock();
            var OfferPriceCalculation = new OfferPriceCalculation(priceCalCService, marketRatesService, configurationService, null);
            InterestRateCondition regularInterestEuribor = new InterestRateCondition
            {
                //This base rate id removes -1.5 from spreadRateValue, making rate under limit
                Kind = InterestRateKinds.RegularInterest,
                Rate = new InterestRate
                {
                    BaseRateId = "EURIBOR-6M",
                    SpreadRateValue = (decimal)9.9
                },
                UpperLimit = new InterestRate
                {
                    BaseRateId = "EURIBOR-6M",
                    SpreadRateValue = 18
                },
                LowerLimit = new InterestRate
                {
                    BaseRateId = "EURIBOR-6M",
                    SpreadRateValue = 10
                },
                Title = "regular-interest",
                Currencies = new List<string> { "EUR" }
            };

            ProductConditions conditions = new ProductConditions
            {
                Fees = new List<FeeCondition> { },
                InterestRates = new List<InterestRateCondition> { regularInterestEuribor }
            };
            Application application = Utility.GetApplication(conditions);
            var arrangementRequest = application.ArrangementRequests[0] as TermLoanRequest;
            var priceCalculationConditions = Utility.GetPriceCalculationParameterFromTermLoanRequest(arrangementRequest);

            var variationsDefinition = Utility.GetVariationDefinitionParamsFromApplication(application);
            var result = OfferPriceCalculation.CalculatePrice(application, arrangementRequest);
            var resultInterest = result.Result.Conditions.InterestRates[0];
            //Basic assertions
            Assert.Equal("RanToCompletion", result.Status.ToString());
            Assert.Equal("0", resultInterest.Variations.Count.ToString());

            //Asserting input parameters
            Assert.Equal("EURIBOR-6M", resultInterest.Rate.BaseRateId);
            Assert.Equal(0, resultInterest.Rate.BaseRateValue);
            Assert.Equal((decimal)9.9, resultInterest.Rate.SpreadRateValue);

            Assert.Equal("EURIBOR-6M", resultInterest.UpperLimit.BaseRateId);
            Assert.Equal(0, resultInterest.UpperLimit.BaseRateValue);
            Assert.Equal((decimal)18.0, resultInterest.UpperLimit.SpreadRateValue);

            Assert.Equal("EURIBOR-6M", resultInterest.LowerLimit.BaseRateId);
            Assert.Equal(0, resultInterest.LowerLimit.BaseRateValue);
            Assert.Equal((decimal)10.0, resultInterest.LowerLimit.SpreadRateValue);

            //Asserting Napr and calculation results
            Assert.Equal((decimal)10.0, ((TermLoanRequest)result.Result).Napr);
            Assert.Equal((decimal)10.0, resultInterest.CalculatedRate);
            Assert.Equal((decimal)10.0, resultInterest.CalculatedLowerLimit);
            Assert.Equal((decimal)18.0, resultInterest.CalculatedUpperLimit);
        }

        [Fact]
        //No variations, calc rates should be as they are set in the request
        //Rate is within limits
        public void NoVariationsRateBelowLimitBasedOnMarketRate()
        {
            ConfigurationServiceMock configurationService = new ConfigurationServiceMock();
            PriceCalculationDMNServiceMock priceCalCService = new PriceCalculationDMNServiceMock(configurationService);
            MarketRatesServiceMock marketRatesService = new MarketRatesServiceMock();
            var OfferPriceCalculation = new OfferPriceCalculation(priceCalCService, marketRatesService, configurationService, null);
            InterestRateCondition regularInterestEuribor = new InterestRateCondition
            {
                //This base rate id removes -1.5 from spreadRateValue, making rate under limit
                Kind = InterestRateKinds.RegularInterest,
                Rate = new InterestRate
                {
                    BaseRateId = "EURIBOR-3M",
                    SpreadRateValue = (decimal)10.1
                },
                UpperLimit = new InterestRate
                {
                    BaseRateId = "EURIBOR-6M",
                    SpreadRateValue = 18
                },
                LowerLimit = new InterestRate
                {
                    BaseRateId = "EURIBOR-6M",
                    SpreadRateValue = 10
                },
                Title = "regular-interest",
                Currencies = new List<string> { "EUR", "RSD" }
            };

            ProductConditions conditions = new ProductConditions
            {
                Fees = new List<FeeCondition> { },
                InterestRates = new List<InterestRateCondition> { regularInterestEuribor }
            };
            Application application = Utility.GetApplication(conditions);
            var arrangementRequest = application.ArrangementRequests[0] as TermLoanRequest;
            var priceCalculationConditions = Utility.GetPriceCalculationParameterFromTermLoanRequest(arrangementRequest);

            var variationsDefinition = Utility.GetVariationDefinitionParamsFromApplication(application);
            var result = OfferPriceCalculation.CalculatePrice(application, arrangementRequest);
            var resultInterest = result.Result.Conditions.InterestRates[0];
            //Basic assertions
            Assert.Equal("RanToCompletion", result.Status.ToString());
            Assert.Equal("0", resultInterest.Variations.Count.ToString());

            //Asserting input parameters
            Assert.Equal("EURIBOR-3M", resultInterest.Rate.BaseRateId);
            Assert.Equal((decimal)-1.5, resultInterest.Rate.BaseRateValue);
            Assert.Equal((decimal)10.1, resultInterest.Rate.SpreadRateValue);

            Assert.Equal("EURIBOR-6M", resultInterest.UpperLimit.BaseRateId);
            Assert.Equal(0, resultInterest.UpperLimit.BaseRateValue);
            Assert.Equal((decimal)18.0, resultInterest.UpperLimit.SpreadRateValue);

            Assert.Equal("EURIBOR-6M", resultInterest.LowerLimit.BaseRateId);
            Assert.Equal(0, resultInterest.LowerLimit.BaseRateValue);
            Assert.Equal((decimal)10.0, resultInterest.LowerLimit.SpreadRateValue);

            //Asserting Napr and calculation results
            Assert.Equal((decimal)10.0, ((TermLoanRequest)result.Result).Napr);
            Assert.Equal((decimal)10.0, resultInterest.CalculatedRate);
            Assert.Equal((decimal)10.0, resultInterest.CalculatedLowerLimit);
            Assert.Equal((decimal)18.0, resultInterest.CalculatedUpperLimit);
        }

        [Fact]
        //No variations, calc rates should be as they are set in the request
        //Rate is within limits
        public void NoVariationsRateAboveLimitWithoutMarketRate()
        {
            ConfigurationServiceMock configurationService = new ConfigurationServiceMock();
            PriceCalculationDMNServiceMock priceCalCService = new PriceCalculationDMNServiceMock(configurationService);
            MarketRatesServiceMock marketRatesService = new MarketRatesServiceMock();
            var OfferPriceCalculation = new OfferPriceCalculation(priceCalCService, marketRatesService, configurationService, null);
            InterestRateCondition regularInterestEuribor = new InterestRateCondition
            {
                //This base rate id removes -1.5 from spreadRateValue, making rate under limit
                Kind = InterestRateKinds.RegularInterest,
                Rate = new InterestRate
                {
                    BaseRateId = "EURIBOR-6M",
                    SpreadRateValue = (decimal)18.1
                },
                UpperLimit = new InterestRate
                {
                    BaseRateId = "EURIBOR-6M",
                    SpreadRateValue = 18
                },
                LowerLimit = new InterestRate
                {
                    BaseRateId = "EURIBOR-6M",
                    SpreadRateValue = 10
                },
                Title = "regular-interest",
                Currencies = new List<string> { "EUR" }
            };

            ProductConditions conditions = new ProductConditions
            {
                Fees = new List<FeeCondition> { },
                InterestRates = new List<InterestRateCondition> { regularInterestEuribor }
            };
            Application application = Utility.GetApplication(conditions);
            var arrangementRequest = application.ArrangementRequests[0] as TermLoanRequest;
            var priceCalculationConditions = Utility.GetPriceCalculationParameterFromTermLoanRequest(arrangementRequest);

            var variationsDefinition = Utility.GetVariationDefinitionParamsFromApplication(application);
            var result = OfferPriceCalculation.CalculatePrice(application, arrangementRequest);
            var resultInterest = result.Result.Conditions.InterestRates[0];
            //Basic assertions
            Assert.Equal("RanToCompletion", result.Status.ToString());
            Assert.Equal("0", resultInterest.Variations.Count.ToString());

            //Asserting input parameters
            Assert.Equal("EURIBOR-6M", resultInterest.Rate.BaseRateId);
            Assert.Equal(0, resultInterest.Rate.BaseRateValue);
            Assert.Equal((decimal)18.1, resultInterest.Rate.SpreadRateValue);

            Assert.Equal("EURIBOR-6M", resultInterest.UpperLimit.BaseRateId);
            Assert.Equal(0, resultInterest.UpperLimit.BaseRateValue);
            Assert.Equal((decimal)18.0, resultInterest.UpperLimit.SpreadRateValue);

            Assert.Equal("EURIBOR-6M", resultInterest.LowerLimit.BaseRateId);
            Assert.Equal(0, resultInterest.LowerLimit.BaseRateValue);
            Assert.Equal((decimal)10.0, resultInterest.LowerLimit.SpreadRateValue);

            //Asserting Napr and calculation results
            Assert.Equal((decimal)18.0, ((TermLoanRequest)result.Result).Napr);
            Assert.Equal((decimal)18.0, resultInterest.CalculatedRate);
            Assert.Equal((decimal)10.0, resultInterest.CalculatedLowerLimit);
            Assert.Equal((decimal)18.0, resultInterest.CalculatedUpperLimit);
        }

        [Fact]
        //No variations, calc rates should be as they are set in the request
        //Rate is within limits
        public void NoVariationsRateAboveLimitBasedOnMarketRate()
        {
            ConfigurationServiceMock configurationService = new ConfigurationServiceMock();
            PriceCalculationDMNServiceMock priceCalCService = new PriceCalculationDMNServiceMock(configurationService);
            MarketRatesServiceMock marketRatesService = new MarketRatesServiceMock();
            var OfferPriceCalculation = new OfferPriceCalculation(priceCalCService, marketRatesService, configurationService, null);
            InterestRateCondition regularInterestEuribor = new InterestRateCondition
            {
                //This base rate id removes -1.5 from spreadRateValue, making rate under limit
                Kind = InterestRateKinds.RegularInterest,
                Rate = new InterestRate
                {
                    BaseRateId = "EURIBOR-4M",
                    SpreadRateValue = 17
                },
                UpperLimit = new InterestRate
                {
                    BaseRateId = "EURIBOR-6M",
                    SpreadRateValue = 18
                },
                LowerLimit = new InterestRate
                {
                    BaseRateId = "EURIBOR-6M",
                    SpreadRateValue = 10
                },
                Title = "regular-interest",
                Currencies = new List<string> { "EUR" }
            };

            ProductConditions conditions = new ProductConditions
            {
                Fees = new List<FeeCondition> { },
                InterestRates = new List<InterestRateCondition> { regularInterestEuribor }
            };
            Application application = Utility.GetApplication(conditions);
            var arrangementRequest = application.ArrangementRequests[0] as TermLoanRequest;
            var priceCalculationConditions = Utility.GetPriceCalculationParameterFromTermLoanRequest(arrangementRequest);

            var variationsDefinition = Utility.GetVariationDefinitionParamsFromApplication(application);
            var result = OfferPriceCalculation.CalculatePrice(application, arrangementRequest);
            var resultInterest = result.Result.Conditions.InterestRates[0];
            //Basic assertions
            Assert.Equal("RanToCompletion", result.Status.ToString());
            Assert.Equal("0", resultInterest.Variations.Count.ToString());

            //Asserting input parameters
            Assert.Equal("EURIBOR-4M", resultInterest.Rate.BaseRateId);
            Assert.Equal(2, resultInterest.Rate.BaseRateValue);
            Assert.Equal(17, resultInterest.Rate.SpreadRateValue);

            Assert.Equal("EURIBOR-6M", resultInterest.UpperLimit.BaseRateId);
            Assert.Equal(0, resultInterest.UpperLimit.BaseRateValue);
            Assert.Equal((decimal)18.0, resultInterest.UpperLimit.SpreadRateValue);

            Assert.Equal("EURIBOR-6M", resultInterest.LowerLimit.BaseRateId);
            Assert.Equal(0, resultInterest.LowerLimit.BaseRateValue);
            Assert.Equal((decimal)10.0, resultInterest.LowerLimit.SpreadRateValue);

            //Asserting Napr and calculation results
            Assert.Equal((decimal)18.0, ((TermLoanRequest)result.Result).Napr);
            Assert.Equal((decimal)18.0, resultInterest.CalculatedRate);
            Assert.Equal((decimal)10.0, resultInterest.CalculatedLowerLimit);
            Assert.Equal((decimal)18.0, resultInterest.CalculatedUpperLimit);
        }
    }
}
