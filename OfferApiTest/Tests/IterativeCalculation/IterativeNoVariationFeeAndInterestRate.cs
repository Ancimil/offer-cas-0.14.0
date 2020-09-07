using System;
using Xunit;
using Offer.Domain.Calculations;
using Offer.Domain.AggregatesModel.ApplicationAggregate;
using PriceCalculation.Models.Pricing;
using System.Collections.Generic;
using MicroserviceCommon.Models;
using PriceCalculation.Models.Product;

namespace OfferApiTest
{
    public class IterativeNoVariationFeeAndInterestRate
    {
        static ConfigurationServiceMock configurationService = new ConfigurationServiceMock();
        static PriceCalculationDMNServiceMock priceCalCService = new PriceCalculationDMNServiceMock(configurationService);
        static MarketRatesServiceMock marketRatesService = new MarketRatesServiceMock();
        //Arrangement Request is set to RSD
        //No interest rate is defined
        //There are no defined variations - Application data does not matter
        [Fact]
        public void IterativeNoFeeNoInterestRate()
        {
            var OfferPriceCalculation = new OfferPriceCalculation(priceCalCService, marketRatesService, configurationService, null);
           
            ProductConditions conditions = new ProductConditions
            {
                Fees = new List<FeeCondition> {},
                InterestRates = new List<InterestRateCondition> {}
            };
            var arrangementRequest = Utility.GetArangementRequest(conditions);

            Application application = new Application
            {
                //U DMN-u pogledati podatke kje uticu na skor iz configa
                //Ovo ce promeniti broj Percentage za fee i za interest-rate
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

            var result = OfferPriceCalculation.CalculatePrice(application, arrangementRequest);
            Assert.Equal("RanToCompletion", result.Status.ToString());
            Assert.Empty(result.Result.Conditions.InterestRates);
            Assert.Empty(result.Result.Conditions.Fees);
        }

        //Arrangement Request is set to RSD
        //Belibor and Euribor are both defined - Belibor should be picked
        //There are no defined variations - Application data does not matter
        //Rate is changed due to baseRateId matching a marketRate in the service "EURIBOR-3M"
        [Fact]
        public void IterativeTwoInterestRatesOneUsed()
        {
            var OfferPriceCalculation = new OfferPriceCalculation(priceCalCService, marketRatesService, configurationService, null);
            /*VariationsDefinition (fee.variationsDefinition) utice na fee.percentage (percentage + varijacije)
            fixedAmount moze da seta u okviru definisanih granica (Upper/LowerLimitAmount ili percentage)
            */
            //moze da postoji samo jedan origination fee
            FeeCondition regularFee = new FeeCondition
            {
                Kind = FeeConditionKinds.OriginationFee,
                ServiceCode = "",
                ServiceDescription = "",
                Percentage = (decimal)0.2,
                FixedAmount = new Currency { Amount = 200, Code = "RSD" },
                LowerLimit = new Currency { Amount = 200, Code = "RSD" },
                UpperLimit = new Currency { Amount = 4000, Code = "RSD" },
                PercentageLowerLimit = (decimal)0.01,
                PercentageUpperLimit = 1,
                Title = "Origination-fee1",
                EffectiveDate = DateTime.Now.AddDays(-1),
                Currencies = new List<string> { "RSD", "EUR" }
            };
            //SpreadRateValue + BaseRateValue + SumaVarijacija = interestRate.CalculatedRate 
            //Vazi za svaki interest rate
            //Samo jedan RegularInterest moze biti na proizvodu i napr je vezan samo za njegov CalculatedRate
            InterestRateCondition regularInterestEuribor = new InterestRateCondition
            {
                //market-rates api ima bazne vrednosti za interest rates - odavde uzeti primere
                //Regular interest moze da bude definisan posebno za svaku valutu
                Kind = InterestRateKinds.RegularInterest,
                Rate = new InterestRate
                {
                    BaseRateId = "EURIBOR-3M",
                    SpreadRateValue = 10
                },
                Title = "regular-interest",
                Currencies = new List<string> { "EUR" }
            };

            InterestRateCondition regularInterestBelibor = new InterestRateCondition
            {
                //SpreadRateValue je fiksni deo kamatne stope
                //Base rate id definise prema cemu se gleda promenljivi deo (baseRateBalue) kamatne stope
                //Obuhvati da nema stopu
                //IsFixed
                Kind = InterestRateKinds.RegularInterest,
                Rate = new InterestRate
                {
                    BaseRateId = "BELIBOR-3M",
                    SpreadRateValue = 12
                },
                Title = "regular-interest",
                Currencies = new List<string> { "RSD" }
            };

            ProductConditions conditions = new ProductConditions {
                Fees = new List<FeeCondition> { regularFee },
                InterestRates = new List<InterestRateCondition> { regularInterestEuribor, regularInterestBelibor }
            };
            var arrangementRequest = Utility.GetArangementRequest(conditions);

            Application application = new Application
            {
                //U DMN-u pogledati podatke kje uticu na skor iz configa
                //Ovo ce promeniti broj Percentage za fee i za interest-rate
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

            var result = OfferPriceCalculation.CalculatePrice(application, arrangementRequest);

            var resultFees = result.Result.Conditions.Fees[0];
            var resultInterest = result.Result.Conditions.InterestRates[0];

            Assert.Equal("RanToCompletion", result.Status.ToString());
            Assert.Empty(resultFees.Variations);
            Assert.Equal((decimal)0.2, resultFees.CalculatedPercentage);
            Assert.Equal(200, resultFees.CalculatedFixedAmount);
            Assert.Null(resultFees.VariationsDefinitionDMN);

            Assert.Equal("BELIBOR-3M", resultInterest.Rate.BaseRateId);
            Assert.Equal((decimal)9.5, resultInterest.CalculatedRate);
            Assert.Equal((decimal)-2.5, resultInterest.Rate.BaseRateValue);
            Assert.Equal((decimal)12.0, resultInterest.Rate.SpreadRateValue);
            //C# won't let this be asserted in any other way
            Assert.Equal("0", resultInterest.Variations.Count.ToString());

            Assert.Empty(resultInterest.Variations);
            Assert.Equal(0, resultInterest.CalculatedLowerLimit);
            Assert.Equal(100, resultInterest.CalculatedUpperLimit);
        }

        [Fact]
        //Arrangement Request is set to EUR
        //Belibor and Euribor are both defined - Euribor should be picked
        //There are no defined variations - Application data does not matter
        //There are two fees, both accounted because they're in the same currency
        public void IterativeTwoFeesDiferenteKindBothUsed()
        {
            var OfferPriceCalculation = new OfferPriceCalculation(priceCalCService, marketRatesService, configurationService, null);
            FeeCondition regularFee = new FeeCondition
            {
                Kind = FeeConditionKinds.OriginationFee,
                Percentage = (decimal)0.2,
                FixedAmount = new Currency { Amount = 200, Code = "EUR" },
                LowerLimit = new Currency { Amount = 200, Code = "EUR" },
                UpperLimit = new Currency { Amount = 4000, Code = "EUR" },
                PercentageLowerLimit = (decimal)0.01,
                PercentageUpperLimit = 1,
                Title = "Origination-fee1",
                EffectiveDate = DateTime.Now.AddDays(-1),
                Currencies = new List<string> { "RSD", "EUR" }
            };

            FeeCondition serviceFee = new FeeCondition
            {
                Kind = FeeConditionKinds.ServiceFee,
                Percentage = (decimal)0.2,
                FixedAmount = new Currency { Amount = 400, Code = "RSD" },
                LowerLimit = new Currency { Amount = 200, Code = "RSD" },
                UpperLimit = new Currency { Amount = 10000, Code = "RSD" },
                PercentageLowerLimit = (decimal)0.01,
                PercentageUpperLimit = 1,
                Title = "Service-Fee",
                EffectiveDate = DateTime.Now.AddDays(-1),
                Currencies = new List<string> { "RSD", "EUR" }
            };

            ProductConditions conditions = new ProductConditions
            {
                Fees = new List<FeeCondition> { regularFee, serviceFee },
                InterestRates = new List<InterestRateCondition>()
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

            var result = OfferPriceCalculation.CalculatePrice(application, arrangementRequest);


            Assert.Equal(2, result.Result.Conditions.Fees.Count);
            var resultRegularFee = result.Result.Conditions.Fees[0];
            var resultServiceFee = result.Result.Conditions.Fees[1];
            Assert.Equal("RanToCompletion", result.Status.ToString());
            Assert.Empty(resultRegularFee.Variations);
            Assert.Equal((decimal)0.2, resultRegularFee.CalculatedPercentage);
            Assert.Equal(200, resultRegularFee.CalculatedFixedAmount);
            Assert.Null(resultRegularFee.VariationsDefinitionDMN);

            Assert.Empty(resultServiceFee.Variations);
            Assert.Equal((decimal)0.2, resultServiceFee.CalculatedPercentage);
            Assert.Equal(400, resultServiceFee.CalculatedFixedAmount);
            Assert.Null(resultServiceFee.VariationsDefinitionDMN);

            Assert.Equal("0", result.Result.Conditions.InterestRates.Count.ToString());
        }

        [Fact]
        //Arrangement Request is set to EUR
        //There are no defined variations - Application data does not matter
        //There are two fees, both accounted because they're in the same currency
        public void IterativeMultipleFeesDiferentKindOneNotUsed()
        {
            var OfferPriceCalculation = new OfferPriceCalculation(priceCalCService, marketRatesService, configurationService, null);
            FeeCondition regularFee = new FeeCondition
            {
                Kind = FeeConditionKinds.OriginationFee,
                Percentage = (decimal)0.2,
                FixedAmount = new Currency { Amount = 200, Code = "EUR" },
                LowerLimit = new Currency { Amount = 200, Code = "EUR" },
                UpperLimit = new Currency { Amount = 4000, Code = "EUR" },
                PercentageLowerLimit = (decimal)0.01,
                PercentageUpperLimit = 1,
                Title = "Origination-fee1",
                EffectiveDate = DateTime.Now.AddDays(-1),
                Currencies = new List<string> { "RSD", "EUR" }
            };

            FeeCondition serviceFeeRSD = new FeeCondition
            {
                Kind = FeeConditionKinds.ServiceFee,
                Percentage = (decimal)0.2,
                FixedAmount = new Currency { Amount = 400, Code = "RSD" },
                LowerLimit = new Currency { Amount = 200, Code = "RSD" },
                UpperLimit = new Currency { Amount = 10000, Code = "RSD" },
                PercentageLowerLimit = (decimal)0.01,
                PercentageUpperLimit = 1,
                Title = "Service-Fee",
                EffectiveDate = DateTime.Now.AddDays(-1),
                Currencies = new List<string> { "RSD" }
            };

            ProductConditions conditions = new ProductConditions
            {
                Fees = new List<FeeCondition> { regularFee, serviceFeeRSD },
                InterestRates = new List<InterestRateCondition>()
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

            var result = OfferPriceCalculation.CalculatePrice(application, arrangementRequest);

            Assert.Single(result.Result.Conditions.Fees);

            var resultRegularFee = result.Result.Conditions.Fees[0];
            Assert.Equal("RanToCompletion", result.Status.ToString());
            Assert.Empty(resultRegularFee.Variations);
            Assert.Equal((decimal)0.2, resultRegularFee.CalculatedPercentage);
            Assert.Equal(200, resultRegularFee.CalculatedFixedAmount);
            Assert.Null(resultRegularFee.VariationsDefinitionDMN);
            Assert.Equal("0", result.Result.Conditions.InterestRates.Count.ToString());
        }

        [Fact]
        //Arrangement Request is set to EUR
        //There are no defined variations - Application data does not matter
        //There are two fees, both accounted because they're in the same currency
        public void IterativeTwoInterestRatesDiferentKind()
        {
            var OfferPriceCalculation = new OfferPriceCalculation(priceCalCService, marketRatesService, configurationService, null);
            FeeCondition regularFee = new FeeCondition
            {
                //Samo % se testira za sada jer trenutno nema za fixedAmount
                //Samo varijacije uticu na Percentage
                Kind = FeeConditionKinds.OriginationFee,
                Percentage = (decimal)0.2,
                FixedAmount = new Currency { Amount = 200, Code = "EUR" },
                LowerLimit = new Currency { Amount = 200, Code = "EUR" },
                UpperLimit = new Currency { Amount = 4000, Code = "EUR" },
                PercentageLowerLimit = (decimal)0.01,
                PercentageUpperLimit = 1,
                Title = "Origination-fee1",
                EffectiveDate = DateTime.Now.AddDays(-1),
                Currencies = new List<string> { "RSD", "EUR" }
            };

            InterestRateCondition regularInterestEuribor = new InterestRateCondition
            {
                Kind = InterestRateKinds.RegularInterest,
                Rate = new InterestRate
                {
                    BaseRateId = "EURIBOR-3M",
                    SpreadRateValue = 10
                },
                Title = "regular-interest",
                Currencies = new List<string> { "EUR" }
            };

            InterestRateCondition earlyWithdrawalInterest = new InterestRateCondition
            {
                Kind = InterestRateKinds.EarlyWithdrawalInterest,
                Rate = new InterestRate
                {
                    BaseRateId = "EARLY-EUR",
                    SpreadRateValue = 2
                },
                Title = "early-withdrawal-interest",
                Currencies = new List<string> { "EUR" }
            };

            ProductConditions conditions = new ProductConditions
            {
                Fees = new List<FeeCondition> { regularFee },
                InterestRates = new List<InterestRateCondition> { regularInterestEuribor, earlyWithdrawalInterest }
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

            var result = OfferPriceCalculation.CalculatePrice(application, arrangementRequest);

            Assert.Single(result.Result.Conditions.Fees);
            Assert.Equal("RanToCompletion", result.Status.ToString());

            var resultInterest = result.Result.Conditions.InterestRates[0];
            var resultEarlyWithdrawalInterest = result.Result.Conditions.InterestRates[1];

            Assert.Equal("EURIBOR-3M", resultInterest.Rate.BaseRateId);
            Assert.Equal((decimal)8.5, resultInterest.CalculatedRate);
            Assert.Equal((decimal)-1.5, resultInterest.Rate.BaseRateValue);
            Assert.Equal((decimal)10.0, resultInterest.Rate.SpreadRateValue);
            //C# won't let this be asserted in any other way
            Assert.Equal("0", resultInterest.Variations.Count.ToString());

            Assert.Equal("EARLY-EUR", resultEarlyWithdrawalInterest.Rate.BaseRateId);
            Assert.Equal((decimal)3.5, resultEarlyWithdrawalInterest.CalculatedRate);
            Assert.Equal((decimal)1.5, resultEarlyWithdrawalInterest.Rate.BaseRateValue);
            Assert.Equal((decimal)2.0, resultEarlyWithdrawalInterest.Rate.SpreadRateValue);

            Assert.Equal(0, resultInterest.CalculatedLowerLimit);
            Assert.Equal(100, resultInterest.CalculatedUpperLimit);
        }

        //Both fees should be calculated and accounted for
        [Fact]
        public void IterativeMultipleOriginationFeesBothCalculated()
        {
            var OfferPriceCalculation = new OfferPriceCalculation(priceCalCService, marketRatesService, configurationService, null);
            FeeCondition regularFee = new FeeCondition
            {
                Kind = FeeConditionKinds.OriginationFee,
                ServiceCode = "",
                ServiceDescription = "",
                Percentage = (decimal)0.2,
                FixedAmount = new Currency { Amount = 200, Code = "RSD" },
                LowerLimit = new Currency { Amount = 200, Code = "RSD" },
                UpperLimit = new Currency { Amount = 4000, Code = "RSD" },
                PercentageLowerLimit = (decimal)0.01,
                PercentageUpperLimit = 1,
                Title = "Origination-fee1",
                EffectiveDate = DateTime.Now.AddDays(-1),
                Currencies = new List<string> { "RSD", "EUR" }
            };

            FeeCondition regularFee2 = new FeeCondition
            {
                Kind = FeeConditionKinds.OriginationFee,
                ServiceCode = "",
                ServiceDescription = "",
                Percentage = (decimal)0.3,
                FixedAmount = new Currency { Amount = 300, Code = "RSD" },
                LowerLimit = new Currency { Amount = 200, Code = "RSD" },
                UpperLimit = new Currency { Amount = 4000, Code = "RSD" },
                PercentageLowerLimit = (decimal)0.01,
                PercentageUpperLimit = 1,
                Title = "Origination-fee2",
                EffectiveDate = DateTime.Now.AddDays(-1),
                Currencies = new List<string> { "RSD", "EUR" }
            };

            ProductConditions conditions = new ProductConditions
            {
                Fees = new List<FeeCondition> { regularFee },
                InterestRates = new List<InterestRateCondition> {  }
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

            var result = OfferPriceCalculation.CalculatePrice(application, arrangementRequest);

            Assert.Equal("RanToCompletion", result.Status.ToString());
            var resultFees = result.Result.Conditions.Fees;

            //Assert.Equal("2", resultFees.Count.ToString());
            Assert.Equal("1", resultFees.Count.ToString());
            var resultFeeOne = resultFees[0];
            Assert.Empty(resultFeeOne.Variations);
            Assert.Equal((decimal)0.2, resultFeeOne.CalculatedPercentage);
            Assert.Equal(200, resultFeeOne.CalculatedFixedAmount);
            Assert.Null(resultFeeOne.VariationsDefinitionDMN);

            //var resultFeeTwo = resultFees[1];
            //Assert.Empty(resultFeeTwo.Variations);
            //Assert.Equal((decimal)0.3, resultFeeTwo.CalculatedPercentage);
            //Assert.Equal(300, resultFeeTwo.CalculatedFixedAmount);
            //Assert.Null(resultFeeTwo.VariationsDefinitionDMN);
        }

        [Fact]
        public void IterativeRateInInterestRateNull()
        {
            var OfferPriceCalculation = new OfferPriceCalculation(priceCalCService, marketRatesService, configurationService, null);
            InterestRateCondition intRate = new InterestRateCondition
            {
                Kind = InterestRateKinds.RegularInterest,
                Rate = null,
                Title = "regular-interest",
                Currencies = new List<string> { "RSD" }
            };

            ProductConditions conditions = new ProductConditions
            {
                InterestRates = new List<InterestRateCondition> { intRate }
            };
            var arrangementRequest = Utility.GetArangementRequest(conditions);

            Application application = new Application
            {
                //U DMN-u pogledati podatke kje uticu na skor iz configa
                //Ovo ce promeniti broj Percentage za fee i za interest-rate
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

            var result = OfferPriceCalculation.CalculatePrice(application, arrangementRequest);

            Assert.Equal("RanToCompletion", result.Status.ToString());
            Assert.Single(result.Result.Conditions.InterestRates);
        }

        [Fact]
        public void IterativeBaseRateInInterestRateNull()
        {
            var OfferPriceCalculation = new OfferPriceCalculation(priceCalCService, marketRatesService, configurationService, null);
            InterestRateCondition regularInterestEuribor = new InterestRateCondition
            {
                //market-rates api ima bazne vrednosti za interest rates - odavde uzeti primere
                //Regular interest moze da bude definisan posebno za svaku valutu
                Kind = InterestRateKinds.RegularInterest,
                Rate = new InterestRate
                {
                    BaseRateId = null,
                    SpreadRateValue = 10
                },
                EffectiveDate = DateTime.Now.AddDays(-1),
                Title = "regular-interest",
                Currencies = new List<string> { "RSD" }
            };

            ProductConditions conditions = new ProductConditions
            {
                InterestRates = new List<InterestRateCondition> { regularInterestEuribor }
            };
            var arrangementRequest = Utility.GetArangementRequest(conditions);

            Application application = new Application
            {
                //U DMN-u pogledati podatke kje uticu na skor iz configa
                //Ovo ce promeniti broj Percentage za fee i za interest-rate
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

            var result = OfferPriceCalculation.CalculatePrice(application, arrangementRequest);

            Assert.Equal("RanToCompletion", result.Status.ToString());
            Assert.Single(result.Result.Conditions.InterestRates);
            var interestRate = result.Result.Conditions.InterestRates[0];
            Assert.Null(interestRate.Rate.BaseRateId);
            Assert.Equal(0, interestRate.Rate.BaseRateValue);
            Assert.Equal((decimal)10.0, interestRate.Rate.SpreadRateValue);
        }

        [Fact]
        public void IterativeFeeListNullInConditions()
        {
            var OfferPriceCalculation = new OfferPriceCalculation(priceCalCService, marketRatesService, configurationService, null);

            ProductConditions conditions = new ProductConditions
            {
               Fees = new List<FeeCondition> { null },
               InterestRates = new List<InterestRateCondition> { }
            };
            var arrangementRequest = Utility.GetArangementRequest(conditions);

            Application application = new Application
            {
                //U DMN-u pogledati podatke kje uticu na skor iz configa
                //Ovo ce promeniti broj Percentage za fee i za interest-rate
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

            var result = OfferPriceCalculation.CalculatePrice(application, arrangementRequest);

            Assert.Equal("RanToCompletion", result.Status.ToString());
            Assert.Empty(result.Result.Conditions.Fees);
        }

        [Fact]
        public void IterativeInterestRateListNullInConditions()
        {
            var OfferPriceCalculation = new OfferPriceCalculation(priceCalCService, marketRatesService, configurationService, null);

            ProductConditions conditions = new ProductConditions
            {
                Fees = new List<FeeCondition> { },
                InterestRates = new List<InterestRateCondition> { null }
            };
            var arrangementRequest = Utility.GetArangementRequest(conditions);

            Application application = new Application
            {
                //U DMN-u pogledati podatke kje uticu na skor iz configa
                //Ovo ce promeniti broj Percentage za fee i za interest-rate
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

            var result = OfferPriceCalculation.CalculatePrice(application, arrangementRequest);

            Assert.Equal("RanToCompletion", result.Status.ToString());
            Assert.Empty(result.Result.Conditions.Fees);
        }

    }
}
