using System;
using Xunit;
using Offer.Domain.Calculations;
using Offer.Domain.AggregatesModel.ApplicationAggregate;
using PriceCalculation.Models.Pricing;
using System.Collections.Generic;
using PriceCalculation.Models.Product;

namespace OfferApiTest.Tests
{
    public class InterestRateVariationsTests
    {
        [Fact]
        //No variations, calc rates should be as they are set in the request
        //Rate is within limits
        public void VariationOnRateWithinLimitsLower()
        {
            ConfigurationServiceMock configurationService = new ConfigurationServiceMock();
            PriceCalculationDMNServiceMock priceCalCService = new PriceCalculationDMNServiceMock(configurationService);
            MarketRatesServiceMock marketRatesService = new MarketRatesServiceMock();
            var OfferPriceCalculation = new OfferPriceCalculation(priceCalCService, marketRatesService, configurationService, null);
            InterestRateCondition regularInterestEuribor = new InterestRateCondition
            {
                VariationsDefinitionDMN = "product/price-variations/int-rate-remove.dmn",
                Kind = InterestRateKinds.RegularInterest,
                Rate = new InterestRate
                {
                    BaseRateId = "EURIBOR-6M",
                    SpreadRateValue = (decimal)11.1
                },
                UpperLimitVariationsDefinitionDMN = "",
                UpperLimit = new InterestRate
                {
                    BaseRateId = "EURIBOR-6M",
                    SpreadRateValue = 18
                },
                LowerLimitVariationsDefinitionDMN = "",
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
            
            
            var result = OfferPriceCalculation.CalculatePrice(application, arrangementRequest);
            var resultInterest = result.Result.Conditions.InterestRates[0];
            //Basic assertions
            Assert.Equal("RanToCompletion", result.Status.ToString());
            Assert.Equal("1", resultInterest.Variations.Count.ToString());

            var variation = resultInterest.Variations[0];

            //Asserting variation
            Assert.Equal(-1, variation.Percentage);

            //Asserting input parameters
            Assert.Equal("EURIBOR-6M", resultInterest.Rate.BaseRateId);
            Assert.Equal(0, resultInterest.Rate.BaseRateValue);
            Assert.Equal((decimal)11.1, resultInterest.Rate.SpreadRateValue);

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
        public void VariationOnRateWithinLimitsUpper()
        {
            ConfigurationServiceMock configurationService = new ConfigurationServiceMock();
            PriceCalculationDMNServiceMock priceCalCService = new PriceCalculationDMNServiceMock(configurationService);
            MarketRatesServiceMock marketRatesService = new MarketRatesServiceMock();
            var OfferPriceCalculation = new OfferPriceCalculation(priceCalCService, marketRatesService, configurationService, null);
            InterestRateCondition regularInterestEuribor = new InterestRateCondition
            {
                VariationsDefinitionDMN = "product/price-variations/int-rate-add.dmn",
                Kind = InterestRateKinds.RegularInterest,
                Rate = new InterestRate
                {
                    BaseRateId = "EURIBOR-6M",
                    SpreadRateValue = (decimal)16.9
                },
                UpperLimitVariationsDefinitionDMN = "",
                UpperLimit = new InterestRate
                {
                    BaseRateId = "EURIBOR-6M",
                    SpreadRateValue = 18
                },
                LowerLimitVariationsDefinitionDMN = "",
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
            
            
            var result = OfferPriceCalculation.CalculatePrice(application, arrangementRequest);
            var resultInterest = result.Result.Conditions.InterestRates[0];
            //Basic assertions
            Assert.Equal("RanToCompletion", result.Status.ToString());
            Assert.Equal("1", resultInterest.Variations.Count.ToString());

            var variation = resultInterest.Variations[0];

            //Asserting variation
            Assert.Equal(1, variation.Percentage);

            //Asserting input parameters
            Assert.Equal("EURIBOR-6M", resultInterest.Rate.BaseRateId);
            Assert.Equal(0, resultInterest.Rate.BaseRateValue);
            Assert.Equal((decimal)16.9, resultInterest.Rate.SpreadRateValue);

            Assert.Equal("EURIBOR-6M", resultInterest.UpperLimit.BaseRateId);
            Assert.Equal(0, resultInterest.UpperLimit.BaseRateValue);
            Assert.Equal((decimal)18.0, resultInterest.UpperLimit.SpreadRateValue);

            Assert.Equal("EURIBOR-6M", resultInterest.LowerLimit.BaseRateId);
            Assert.Equal(0, resultInterest.LowerLimit.BaseRateValue);
            Assert.Equal((decimal)10.0, resultInterest.LowerLimit.SpreadRateValue);

            //Asserting Napr and calculation results
            Assert.Equal((decimal)17.9, ((TermLoanRequest)result.Result).Napr);
            Assert.Equal((decimal)17.9, resultInterest.CalculatedRate);
            Assert.Equal((decimal)10.0, resultInterest.CalculatedLowerLimit);
            Assert.Equal((decimal)18.0, resultInterest.CalculatedUpperLimit);
        }

        [Fact]
        //No variations, calc rates should be as they are set in the request
        //Rate is within limits
        public void VariationOnRateUnderLowerLimit()
        {
            ConfigurationServiceMock configurationService = new ConfigurationServiceMock();
            PriceCalculationDMNServiceMock priceCalCService = new PriceCalculationDMNServiceMock(configurationService);
            MarketRatesServiceMock marketRatesService = new MarketRatesServiceMock();
            var OfferPriceCalculation = new OfferPriceCalculation(priceCalCService, marketRatesService, configurationService, null);
            InterestRateCondition regularInterestEuribor = new InterestRateCondition
            {
                VariationsDefinitionDMN = "product/price-variations/int-rate-remove.dmn",
                Kind = InterestRateKinds.RegularInterest,
                Rate = new InterestRate
                {
                    BaseRateId = "EURIBOR-6M",
                    SpreadRateValue = (decimal)10.9
                },
                UpperLimitVariationsDefinitionDMN = "",
                UpperLimit = new InterestRate
                {
                    BaseRateId = "EURIBOR-6M",
                    SpreadRateValue = 18
                },
                LowerLimitVariationsDefinitionDMN = "",
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
            
            
            var result = OfferPriceCalculation.CalculatePrice(application, arrangementRequest);
            var resultInterest = result.Result.Conditions.InterestRates[0];
            //Basic assertions
            Assert.Equal("RanToCompletion", result.Status.ToString());
            Assert.Equal("1", resultInterest.Variations.Count.ToString());

            var variation = resultInterest.Variations[0];

            //Asserting variation
            Assert.Equal(-1, variation.Percentage);

            //Asserting input parameters
            Assert.Equal("EURIBOR-6M", resultInterest.Rate.BaseRateId);
            Assert.Equal(0, resultInterest.Rate.BaseRateValue);
            Assert.Equal((decimal)10.9, resultInterest.Rate.SpreadRateValue);

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
        public void VariationOnRateOverUpperLimit()
        {
            ConfigurationServiceMock configurationService = new ConfigurationServiceMock();
            PriceCalculationDMNServiceMock priceCalCService = new PriceCalculationDMNServiceMock(configurationService);
            MarketRatesServiceMock marketRatesService = new MarketRatesServiceMock();
            var OfferPriceCalculation = new OfferPriceCalculation(priceCalCService, marketRatesService, configurationService, null);
            InterestRateCondition regularInterestEuribor = new InterestRateCondition
            {
                VariationsDefinitionDMN = "product/price-variations/int-rate-add.dmn",
                Kind = InterestRateKinds.RegularInterest,
                Rate = new InterestRate
                {
                    BaseRateId = "EURIBOR-6M",
                    SpreadRateValue = (decimal)17.1
                },
                UpperLimitVariationsDefinitionDMN = "",
                UpperLimit = new InterestRate
                {
                    BaseRateId = "EURIBOR-6M",
                    SpreadRateValue = 18
                },
                LowerLimitVariationsDefinitionDMN = "",
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
            
            
            var result = OfferPriceCalculation.CalculatePrice(application, arrangementRequest);
            var resultInterest = result.Result.Conditions.InterestRates[0];
            //Basic assertions
            Assert.Equal("RanToCompletion", result.Status.ToString());
            Assert.Equal("1", resultInterest.Variations.Count.ToString());

            var variation = resultInterest.Variations[0];

            //Asserting variation
            Assert.Equal(1, variation.Percentage);

            //Asserting input parameters
            Assert.Equal("EURIBOR-6M", resultInterest.Rate.BaseRateId);
            Assert.Equal(0, resultInterest.Rate.BaseRateValue);
            Assert.Equal((decimal)17.1, resultInterest.Rate.SpreadRateValue);

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
        public void VariationOnUpperLimitRateInRange()
        {
            ConfigurationServiceMock configurationService = new ConfigurationServiceMock();
            PriceCalculationDMNServiceMock priceCalCService = new PriceCalculationDMNServiceMock(configurationService);
            MarketRatesServiceMock marketRatesService = new MarketRatesServiceMock();
            var OfferPriceCalculation = new OfferPriceCalculation(priceCalCService, marketRatesService, configurationService, null);
            InterestRateCondition regularInterestEuribor = new InterestRateCondition
            {
                VariationsDefinitionDMN = "",
                Kind = InterestRateKinds.RegularInterest,
                Rate = new InterestRate
                {
                    BaseRateId = "EURIBOR-6M",
                    SpreadRateValue = (decimal)16.9
                },
                UpperLimitVariationsDefinitionDMN = "product/price-variations/int-rate-remove.dmn",
                UpperLimit = new InterestRate
                {
                    BaseRateId = "EURIBOR-6M",
                    SpreadRateValue = 18
                },
                LowerLimitVariationsDefinitionDMN = "",
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
            
            
            var result = OfferPriceCalculation.CalculatePrice(application, arrangementRequest);
            var resultInterest = result.Result.Conditions.InterestRates[0];
            //Basic assertions
            Assert.Equal("RanToCompletion", result.Status.ToString());
            Assert.Equal("1", resultInterest.UpperLimitVariations.Count.ToString());

            var upperLimitVariation = resultInterest.UpperLimitVariations[0];

            //Asserting variation
            //Proveri zasto puca
            Assert.Equal(-1, upperLimitVariation.Percentage);

            //Asserting input parameters
            Assert.Equal("EURIBOR-6M", resultInterest.Rate.BaseRateId);
            Assert.Equal(0, resultInterest.Rate.BaseRateValue);
            Assert.Equal((decimal)16.9, resultInterest.Rate.SpreadRateValue);

            Assert.Equal("EURIBOR-6M", resultInterest.UpperLimit.BaseRateId);
            Assert.Equal(0, resultInterest.UpperLimit.BaseRateValue);
            Assert.Equal((decimal)18.0, resultInterest.UpperLimit.SpreadRateValue);

            Assert.Equal("EURIBOR-6M", resultInterest.LowerLimit.BaseRateId);
            Assert.Equal(0, resultInterest.LowerLimit.BaseRateValue);
            Assert.Equal((decimal)10.0, resultInterest.LowerLimit.SpreadRateValue);

            //Asserting Napr and calculation results
            Assert.Equal((decimal)16.9, ((TermLoanRequest)result.Result).Napr);
            Assert.Equal((decimal)16.9, resultInterest.CalculatedRate);
            Assert.Equal((decimal)10.0, resultInterest.CalculatedLowerLimit);
            Assert.Equal((decimal)17.0, resultInterest.CalculatedUpperLimit);
        }

        [Fact]
        public void VariationOnUpperLimitRateAboveLimit()
        {
            ConfigurationServiceMock configurationService = new ConfigurationServiceMock();
            PriceCalculationDMNServiceMock priceCalCService = new PriceCalculationDMNServiceMock(configurationService);
            MarketRatesServiceMock marketRatesService = new MarketRatesServiceMock();
            var OfferPriceCalculation = new OfferPriceCalculation(priceCalCService, marketRatesService, configurationService, null);
            InterestRateCondition regularInterestEuribor = new InterestRateCondition
            {
                VariationsDefinitionDMN = "",
                Kind = InterestRateKinds.RegularInterest,
                Rate = new InterestRate
                {
                    BaseRateId = "EURIBOR-6M",
                    SpreadRateValue = (decimal)17.1
                },
                UpperLimitVariationsDefinitionDMN = "product/price-variations/int-rate-remove.dmn",
                UpperLimit = new InterestRate
                {
                    BaseRateId = "EURIBOR-6M",
                    SpreadRateValue = 18
                },
                LowerLimitVariationsDefinitionDMN = "",
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
            
            
            var result = OfferPriceCalculation.CalculatePrice(application, arrangementRequest);
            var resultInterest = result.Result.Conditions.InterestRates[0];
            //Basic assertions
            Assert.Equal("RanToCompletion", result.Status.ToString());
            Assert.Equal("1", resultInterest.UpperLimitVariations.Count.ToString());

            var upperLimitVariation = resultInterest.UpperLimitVariations[0];

            //Asserting variation
            //Proveri zasto puca
            Assert.Equal(-1, upperLimitVariation.Percentage);

            //Asserting input parameters
            Assert.Equal("EURIBOR-6M", resultInterest.Rate.BaseRateId);
            Assert.Equal(0, resultInterest.Rate.BaseRateValue);
            Assert.Equal((decimal)17.1, resultInterest.Rate.SpreadRateValue);

            Assert.Equal("EURIBOR-6M", resultInterest.UpperLimit.BaseRateId);
            Assert.Equal(0, resultInterest.UpperLimit.BaseRateValue);
            Assert.Equal((decimal)18.0, resultInterest.UpperLimit.SpreadRateValue);

            Assert.Equal("EURIBOR-6M", resultInterest.LowerLimit.BaseRateId);
            Assert.Equal(0, resultInterest.LowerLimit.BaseRateValue);
            Assert.Equal((decimal)10.0, resultInterest.LowerLimit.SpreadRateValue);

            //Asserting Napr and calculation results
            Assert.Equal((decimal)17.0, ((TermLoanRequest)result.Result).Napr);
            Assert.Equal((decimal)17.0, resultInterest.CalculatedRate);
            Assert.Equal((decimal)10.0, resultInterest.CalculatedLowerLimit);
            Assert.Equal((decimal)17.0, resultInterest.CalculatedUpperLimit);
        }

        [Fact]
        public void VariationsOnRateAndUpperLimitRateInLimit()
        {
            ConfigurationServiceMock configurationService = new ConfigurationServiceMock();
            PriceCalculationDMNServiceMock priceCalCService = new PriceCalculationDMNServiceMock(configurationService);
            MarketRatesServiceMock marketRatesService = new MarketRatesServiceMock();
            var OfferPriceCalculation = new OfferPriceCalculation(priceCalCService, marketRatesService, configurationService, null);
            InterestRateCondition regularInterestEuribor = new InterestRateCondition
            {
                VariationsDefinitionDMN = "product/price-variations/int-rate-add.dmn",
                Kind = InterestRateKinds.RegularInterest,
                Rate = new InterestRate
                {
                    BaseRateId = "EURIBOR-6M",
                    SpreadRateValue = (decimal)17.1
                },
                UpperLimitVariationsDefinitionDMN = "product/price-variations/int-rate-add.dmn",
                UpperLimit = new InterestRate
                {
                    BaseRateId = "EURIBOR-6M",
                    SpreadRateValue = 18
                },
                LowerLimitVariationsDefinitionDMN = "",
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
            
            
            var result = OfferPriceCalculation.CalculatePrice(application, arrangementRequest);
            var resultInterest = result.Result.Conditions.InterestRates[0];
            //Basic assertions
            Assert.Equal("RanToCompletion", result.Status.ToString());
            Assert.Equal("1", resultInterest.UpperLimitVariations.Count.ToString());
            Assert.Equal("1", resultInterest.Variations.Count.ToString());

            var upperLimitVariation = resultInterest.UpperLimitVariations[0];
            var variation = resultInterest.UpperLimitVariations[0];


            //Asserting variation
            //Proveri zasto puca
            Assert.Equal(1, upperLimitVariation.Percentage);
            Assert.Equal(1, variation.Percentage);

            //Asserting input parameters
            Assert.Equal("EURIBOR-6M", resultInterest.Rate.BaseRateId);
            Assert.Equal(0, resultInterest.Rate.BaseRateValue);
            Assert.Equal((decimal)17.1, resultInterest.Rate.SpreadRateValue);

            Assert.Equal("EURIBOR-6M", resultInterest.UpperLimit.BaseRateId);
            Assert.Equal(0, resultInterest.UpperLimit.BaseRateValue);
            Assert.Equal((decimal)18.0, resultInterest.UpperLimit.SpreadRateValue);

            Assert.Equal("EURIBOR-6M", resultInterest.LowerLimit.BaseRateId);
            Assert.Equal(0, resultInterest.LowerLimit.BaseRateValue);
            Assert.Equal((decimal)10.0, resultInterest.LowerLimit.SpreadRateValue);

            //Asserting Napr and calculation results
            Assert.Equal((decimal)18.1, ((TermLoanRequest)result.Result).Napr);
            Assert.Equal((decimal)18.1, resultInterest.CalculatedRate);
            Assert.Equal((decimal)10.0, resultInterest.CalculatedLowerLimit);
            Assert.Equal((decimal)19.0, resultInterest.CalculatedUpperLimit);
        }

        [Fact]
        public void VariationsOnRateAndUpperLimitRateOverUpperLimit()
        {
            ConfigurationServiceMock configurationService = new ConfigurationServiceMock();
            PriceCalculationDMNServiceMock priceCalCService = new PriceCalculationDMNServiceMock(configurationService);
            MarketRatesServiceMock marketRatesService = new MarketRatesServiceMock();
            var OfferPriceCalculation = new OfferPriceCalculation(priceCalCService, marketRatesService, configurationService, null);
            InterestRateCondition regularInterestEuribor = new InterestRateCondition
            {
                VariationsDefinitionDMN = "product/price-variations/int-rate-add2.dmn",
                Kind = InterestRateKinds.RegularInterest,
                Rate = new InterestRate
                {
                    BaseRateId = "EURIBOR-6M",
                    SpreadRateValue = (decimal)17.1
                },
                UpperLimitVariationsDefinitionDMN = "product/price-variations/int-rate-add.dmn",
                UpperLimit = new InterestRate
                {
                    BaseRateId = "EURIBOR-6M",
                    SpreadRateValue = 18
                },
                LowerLimitVariationsDefinitionDMN = "",
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
            
            
            var result = OfferPriceCalculation.CalculatePrice(application, arrangementRequest);
            var resultInterest = result.Result.Conditions.InterestRates[0];
            //Basic assertions
            Assert.Equal("RanToCompletion", result.Status.ToString());
            Assert.Equal("1", resultInterest.UpperLimitVariations.Count.ToString());
            Assert.Equal("1", resultInterest.Variations.Count.ToString());

            var upperLimitVariation = resultInterest.UpperLimitVariations[0];
            var variation = resultInterest.Variations[0];


            //Asserting variation
            //Proveri zasto puca
            Assert.Equal(1, upperLimitVariation.Percentage);
            Assert.Equal(2, variation.Percentage);

            //Asserting input parameters
            Assert.Equal("EURIBOR-6M", resultInterest.Rate.BaseRateId);
            Assert.Equal(0, resultInterest.Rate.BaseRateValue);
            Assert.Equal((decimal)17.1, resultInterest.Rate.SpreadRateValue);

            Assert.Equal("EURIBOR-6M", resultInterest.UpperLimit.BaseRateId);
            Assert.Equal(0, resultInterest.UpperLimit.BaseRateValue);
            Assert.Equal((decimal)18.0, resultInterest.UpperLimit.SpreadRateValue);

            Assert.Equal("EURIBOR-6M", resultInterest.LowerLimit.BaseRateId);
            Assert.Equal(0, resultInterest.LowerLimit.BaseRateValue);
            Assert.Equal((decimal)10.0, resultInterest.LowerLimit.SpreadRateValue);

            //Asserting Napr and calculation results
            Assert.Equal((decimal)19.0, ((TermLoanRequest)result.Result).Napr);
            Assert.Equal((decimal)19.0, resultInterest.CalculatedRate);
            Assert.Equal((decimal)10.0, resultInterest.CalculatedLowerLimit);
            Assert.Equal((decimal)19.0, resultInterest.CalculatedUpperLimit);
        }

        [Fact]
        public void MultipleVariationsOnRateAndUpperLimitRateInRange()
        {
            ConfigurationServiceMock configurationService = new ConfigurationServiceMock();
            PriceCalculationDMNServiceMock priceCalCService = new PriceCalculationDMNServiceMock(configurationService);
            MarketRatesServiceMock marketRatesService = new MarketRatesServiceMock();
            var OfferPriceCalculation = new OfferPriceCalculation(priceCalCService, marketRatesService, configurationService, null);
            InterestRateCondition regularInterestEuribor = new InterestRateCondition
            {
                VariationsDefinitionDMN = "product/price-variations/int-rate-add-two-variations.dmn",
                Kind = InterestRateKinds.RegularInterest,
                Rate = new InterestRate
                {
                    BaseRateId = "EURIBOR-6M",
                    SpreadRateValue = (decimal)17.1
                },
                UpperLimitVariationsDefinitionDMN = "product/price-variations/int-rate-add-two-variations.dmn",
                UpperLimit = new InterestRate
                {
                    BaseRateId = "EURIBOR-6M",
                    SpreadRateValue = 18
                },
                LowerLimitVariationsDefinitionDMN = "",
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
            
            
            var result = OfferPriceCalculation.CalculatePrice(application, arrangementRequest);
            var resultInterest = result.Result.Conditions.InterestRates[0];
            //Basic assertions
            Assert.Equal("RanToCompletion", result.Status.ToString());
            Assert.Equal("2", resultInterest.UpperLimitVariations.Count.ToString());
            Assert.Equal("2", resultInterest.Variations.Count.ToString());

            var upperLimitVariation = resultInterest.UpperLimitVariations[0];
            var upperLimitVariationTwo = resultInterest.UpperLimitVariations[1];
            var variation = resultInterest.Variations[0];
            var variationTwo = resultInterest.Variations[1];

            //Asserting variation
            //Proveri zasto puca
            Assert.Equal(1, upperLimitVariation.Percentage);
            Assert.Equal(2, upperLimitVariationTwo.Percentage);
            Assert.Equal(1, variation.Percentage);
            Assert.Equal(2, variationTwo.Percentage);

            //Asserting input parameters
            Assert.Equal("EURIBOR-6M", resultInterest.Rate.BaseRateId);
            Assert.Equal(0, resultInterest.Rate.BaseRateValue);
            Assert.Equal((decimal)17.1, resultInterest.Rate.SpreadRateValue);

            Assert.Equal("EURIBOR-6M", resultInterest.UpperLimit.BaseRateId);
            Assert.Equal(0, resultInterest.UpperLimit.BaseRateValue);
            Assert.Equal((decimal)18.0, resultInterest.UpperLimit.SpreadRateValue);

            Assert.Equal("EURIBOR-6M", resultInterest.LowerLimit.BaseRateId);
            Assert.Equal(0, resultInterest.LowerLimit.BaseRateValue);
            Assert.Equal((decimal)10.0, resultInterest.LowerLimit.SpreadRateValue);

            //Asserting Napr and calculation results
            Assert.Equal((decimal)20.1, ((TermLoanRequest)result.Result).Napr);
            Assert.Equal((decimal)20.1, resultInterest.CalculatedRate);
            Assert.Equal((decimal)10.0, resultInterest.CalculatedLowerLimit);
            Assert.Equal((decimal)21.0, resultInterest.CalculatedUpperLimit);
        }

        //----------------------------------------------------


        [Fact]
        public void VariationOnLowerLimitRateInRange()
        {
            ConfigurationServiceMock configurationService = new ConfigurationServiceMock();
            PriceCalculationDMNServiceMock priceCalCService = new PriceCalculationDMNServiceMock(configurationService);
            MarketRatesServiceMock marketRatesService = new MarketRatesServiceMock();
            var OfferPriceCalculation = new OfferPriceCalculation(priceCalCService, marketRatesService, configurationService, null);
            InterestRateCondition regularInterestEuribor = new InterestRateCondition
            {
                VariationsDefinitionDMN = "",
                Kind = InterestRateKinds.RegularInterest,
                Rate = new InterestRate
                {
                    BaseRateId = "EURIBOR-6M",
                    SpreadRateValue = (decimal)11.1
                },
                UpperLimitVariationsDefinitionDMN = "",
                UpperLimit = new InterestRate
                {
                    BaseRateId = "EURIBOR-6M",
                    SpreadRateValue = 18
                },
                LowerLimitVariationsDefinitionDMN = "product/price-variations/int-rate-add.dmn",
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
            
            
            var result = OfferPriceCalculation.CalculatePrice(application, arrangementRequest);
            var resultInterest = result.Result.Conditions.InterestRates[0];
            //Basic assertions
            Assert.Equal("RanToCompletion", result.Status.ToString());
            Assert.Equal("1", resultInterest.LowerLimitVariations.Count.ToString());

            var lowerLimitVariation = resultInterest.LowerLimitVariations[0];

            //Asserting variation
            Assert.Equal(1, lowerLimitVariation.Percentage);

            //Asserting input parameters
            Assert.Equal("EURIBOR-6M", resultInterest.Rate.BaseRateId);
            Assert.Equal(0, resultInterest.Rate.BaseRateValue);
            Assert.Equal((decimal)11.1, resultInterest.Rate.SpreadRateValue);

            Assert.Equal("EURIBOR-6M", resultInterest.UpperLimit.BaseRateId);
            Assert.Equal(0, resultInterest.UpperLimit.BaseRateValue);
            Assert.Equal((decimal)18.0, resultInterest.UpperLimit.SpreadRateValue);

            Assert.Equal("EURIBOR-6M", resultInterest.LowerLimit.BaseRateId);
            Assert.Equal(0, resultInterest.LowerLimit.BaseRateValue);
            Assert.Equal((decimal)10.0, resultInterest.LowerLimit.SpreadRateValue);

            //Asserting Napr and calculation results
            Assert.Equal((decimal)11.1, ((TermLoanRequest)result.Result).Napr);
            Assert.Equal((decimal)11.1, resultInterest.CalculatedRate);
            Assert.Equal((decimal)11.0, resultInterest.CalculatedLowerLimit);
            Assert.Equal((decimal)18.0, resultInterest.CalculatedUpperLimit);
        }

        [Fact]
        public void VariationOnLowerLimitRateUnderLimit()
        {
            ConfigurationServiceMock configurationService = new ConfigurationServiceMock();
            PriceCalculationDMNServiceMock priceCalCService = new PriceCalculationDMNServiceMock(configurationService);
            MarketRatesServiceMock marketRatesService = new MarketRatesServiceMock();
            var OfferPriceCalculation = new OfferPriceCalculation(priceCalCService, marketRatesService, configurationService, null);
            InterestRateCondition regularInterestEuribor = new InterestRateCondition
            {
                VariationsDefinitionDMN = "",
                Kind = InterestRateKinds.RegularInterest,
                Rate = new InterestRate
                {
                    BaseRateId = "EURIBOR-6M",
                    SpreadRateValue = (decimal)10.9
                },
                UpperLimitVariationsDefinitionDMN = "",
                UpperLimit = new InterestRate
                {
                    BaseRateId = "EURIBOR-6M",
                    SpreadRateValue = 18
                },
                LowerLimitVariationsDefinitionDMN = "product/price-variations/int-rate-remove.dmn",
                LowerLimit = new InterestRate
                {
                    BaseRateId = "EURIBOR-6M",
                    SpreadRateValue = 12
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
            
            
            var result = OfferPriceCalculation.CalculatePrice(application, arrangementRequest);
            var resultInterest = result.Result.Conditions.InterestRates[0];
            //Basic assertions
            Assert.Equal("RanToCompletion", result.Status.ToString());
            Assert.Equal("1", resultInterest.LowerLimitVariations.Count.ToString());

            var lowerLimitVariations = resultInterest.LowerLimitVariations[0];

            //Asserting variation
            //Proveri zasto puca
            Assert.Equal(-1, lowerLimitVariations.Percentage);

            //Asserting input parameters
            Assert.Equal("EURIBOR-6M", resultInterest.Rate.BaseRateId);
            Assert.Equal(0, resultInterest.Rate.BaseRateValue);
            Assert.Equal((decimal)10.9, resultInterest.Rate.SpreadRateValue);

            Assert.Equal("EURIBOR-6M", resultInterest.UpperLimit.BaseRateId);
            Assert.Equal(0, resultInterest.UpperLimit.BaseRateValue);
            Assert.Equal((decimal)18.0, resultInterest.UpperLimit.SpreadRateValue);

            Assert.Equal("EURIBOR-6M", resultInterest.LowerLimit.BaseRateId);
            Assert.Equal(0, resultInterest.LowerLimit.BaseRateValue);
            Assert.Equal((decimal)12.0, resultInterest.LowerLimit.SpreadRateValue);

            //Asserting Napr and calculation results
            Assert.Equal((decimal)11.0, ((TermLoanRequest)result.Result).Napr);
            Assert.Equal((decimal)11.0, resultInterest.CalculatedRate);
            Assert.Equal((decimal)11.0, resultInterest.CalculatedLowerLimit);
            Assert.Equal((decimal)18.0, resultInterest.CalculatedUpperLimit);
        }

        [Fact]
        public void VariationsOnRateAndLowerLimitRateInLimit()
        {
            ConfigurationServiceMock configurationService = new ConfigurationServiceMock();
            PriceCalculationDMNServiceMock priceCalCService = new PriceCalculationDMNServiceMock(configurationService);
            MarketRatesServiceMock marketRatesService = new MarketRatesServiceMock();
            var OfferPriceCalculation = new OfferPriceCalculation(priceCalCService, marketRatesService, configurationService, null);
            InterestRateCondition regularInterestEuribor = new InterestRateCondition
            {
                VariationsDefinitionDMN = "product/price-variations/int-rate-add.dmn",
                Kind = InterestRateKinds.RegularInterest,
                Rate = new InterestRate
                {
                    BaseRateId = "EURIBOR-6M",
                    SpreadRateValue = (decimal)10.1
                },
                UpperLimitVariationsDefinitionDMN = "",
                UpperLimit = new InterestRate
                {
                    BaseRateId = "EURIBOR-6M",
                    SpreadRateValue = 18
                },
                LowerLimitVariationsDefinitionDMN = "product/price-variations/int-rate-add.dmn",
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
            
            
            var result = OfferPriceCalculation.CalculatePrice(application, arrangementRequest);
            var resultInterest = result.Result.Conditions.InterestRates[0];
            //Basic assertions
            Assert.Equal("RanToCompletion", result.Status.ToString());
            Assert.Equal("1", resultInterest.LowerLimitVariations.Count.ToString());
            Assert.Equal("1", resultInterest.Variations.Count.ToString());

            var lowerLimitVariation = resultInterest.LowerLimitVariations[0];
            var variation = resultInterest.Variations[0];


            //Asserting variation
            //Proveri zasto puca
            Assert.Equal(1, lowerLimitVariation.Percentage);
            Assert.Equal(1, variation.Percentage);

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
            Assert.Equal((decimal)11.1, ((TermLoanRequest)result.Result).Napr);
            Assert.Equal((decimal)11.1, resultInterest.CalculatedRate);
            Assert.Equal((decimal)11.0, resultInterest.CalculatedLowerLimit);
            Assert.Equal((decimal)18.0, resultInterest.CalculatedUpperLimit);
        }

        [Fact]
        public void VariationsOnRateAndLowerLimitRateUnderLowerLimit()
        {
            ConfigurationServiceMock configurationService = new ConfigurationServiceMock();
            PriceCalculationDMNServiceMock priceCalCService = new PriceCalculationDMNServiceMock(configurationService);
            MarketRatesServiceMock marketRatesService = new MarketRatesServiceMock();
            var OfferPriceCalculation = new OfferPriceCalculation(priceCalCService, marketRatesService, configurationService, null);
            InterestRateCondition regularInterestEuribor = new InterestRateCondition
            {
                VariationsDefinitionDMN = "product/price-variations/int-rate-add.dmn",
                Kind = InterestRateKinds.RegularInterest,
                Rate = new InterestRate
                {
                    BaseRateId = "EURIBOR-6M",
                    SpreadRateValue = (decimal)10.1
                },
                UpperLimitVariationsDefinitionDMN = "",
                UpperLimit = new InterestRate
                {
                    BaseRateId = "EURIBOR-6M",
                    SpreadRateValue = 18
                },
                LowerLimitVariationsDefinitionDMN = "product/price-variations/int-rate-add2.dmn",
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
            
            
            var result = OfferPriceCalculation.CalculatePrice(application, arrangementRequest);
            var resultInterest = result.Result.Conditions.InterestRates[0];
            //Basic assertions
            Assert.Equal("RanToCompletion", result.Status.ToString());
            Assert.Equal("1", resultInterest.LowerLimitVariations.Count.ToString());
            Assert.Equal("1", resultInterest.Variations.Count.ToString());

            var lowerLimitVariations = resultInterest.LowerLimitVariations[0];
            var variation = resultInterest.Variations[0];


            //Asserting variation
            Assert.Equal(2, lowerLimitVariations.Percentage);
            Assert.Equal(1, variation.Percentage);

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
            Assert.Equal((decimal)12.0, ((TermLoanRequest)result.Result).Napr);
            Assert.Equal((decimal)12.0, resultInterest.CalculatedRate);
            Assert.Equal((decimal)12.0, resultInterest.CalculatedLowerLimit);
            Assert.Equal((decimal)18.0, resultInterest.CalculatedUpperLimit);
        }

        [Fact]
        public void MultipleVariationsOnRateAndLowerLimitRateInRange()
        {
            ConfigurationServiceMock configurationService = new ConfigurationServiceMock();
            PriceCalculationDMNServiceMock priceCalCService = new PriceCalculationDMNServiceMock(configurationService);
            MarketRatesServiceMock marketRatesService = new MarketRatesServiceMock();
            var OfferPriceCalculation = new OfferPriceCalculation(priceCalCService, marketRatesService, configurationService, null);
            InterestRateCondition regularInterestEuribor = new InterestRateCondition
            {
                VariationsDefinitionDMN = "product/price-variations/int-rate-add-two-variations.dmn",
                Kind = InterestRateKinds.RegularInterest,
                Rate = new InterestRate
                {
                    BaseRateId = "EURIBOR-6M",
                    SpreadRateValue = (decimal)10.1
                },
                UpperLimitVariationsDefinitionDMN = "",
                UpperLimit = new InterestRate
                {
                    BaseRateId = "EURIBOR-6M",
                    SpreadRateValue = 18
                },
                LowerLimitVariationsDefinitionDMN = "product/price-variations/int-rate-add-two-variations.dmn",
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
            
            
            var result = OfferPriceCalculation.CalculatePrice(application, arrangementRequest);
            var resultInterest = result.Result.Conditions.InterestRates[0];
            //Basic assertions
            Assert.Equal("RanToCompletion", result.Status.ToString());
            Assert.Equal("2", resultInterest.LowerLimitVariations.Count.ToString());
            Assert.Equal("2", resultInterest.Variations.Count.ToString());

            var lowerLimitVariations = resultInterest.LowerLimitVariations[0];
            var lowerLimitVariationsTwo = resultInterest.LowerLimitVariations[1];
            var variation = resultInterest.Variations[0];
            var variationTwo = resultInterest.Variations[1];

            //Asserting variation
            //Proveri zasto puca
            Assert.Equal(1, lowerLimitVariations.Percentage);
            Assert.Equal(2, lowerLimitVariationsTwo.Percentage);
            Assert.Equal(1, variation.Percentage);
            Assert.Equal(2, variationTwo.Percentage);

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
            Assert.Equal((decimal)13.1, ((TermLoanRequest)result.Result).Napr);
            Assert.Equal((decimal)13.1, resultInterest.CalculatedRate);
            Assert.Equal((decimal)13.0, resultInterest.CalculatedLowerLimit);
            Assert.Equal((decimal)18.0, resultInterest.CalculatedUpperLimit);
        }
    }
}
