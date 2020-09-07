using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Offer.Domain.AggregatesModel.ApplicationAggregate;
using PriceCalculation.Models.Pricing;
using PriceCalculation.Models.Product;
using PriceCalculation.Services;

namespace OfferApiTest
{
    class PriceCalculationDMNServiceMock : IPriceCalculationService
    {
        private readonly ConfigurationServiceMock configurationService;
        private int feeCounter;
        private int intrateCounter;

        public PriceCalculationDMNServiceMock(ConfigurationServiceMock configurationService)
        {
            this.configurationService = configurationService;
        }

        // For every sent variationDefinition sets the variationDefinitionDMN
        // Nema previse smisla trenutno
        public Task<ProductConditions> ReadVariationDefinitions(ProductConditions condition)
        {
            throw new NotImplementedException();
        }

        //Za prosledjen fee, napravi feeVariation koji u odnosuna definition ubaci tu kalkulaciju
        public Task<List<FeeVariation>> ResolveFeeVariationDmn(string dmnContent, VariationDefinitionParams definitionParams)
        {
            var list = new List<FeeVariation>();
            switch (dmnContent)
            {
                case ("product/price-variations/fee-in-range-upper.dmn"):
                    list.Add(CreateFeeVariation((decimal)0.89, 179));
                    break;
                case ("product/price-variations/fee-in-range-lower.dmn"):
                    list.Add(CreateFeeVariation((decimal)-0.08, -179));
                    break;
                case ("product/price-variations/fee-under-range.dmn"):
                    list.Add(CreateFeeVariation((decimal)-0.1, -181));
                    break;
                case ("product/price-variations/fee-over-range.dmn"):
                    list.Add(CreateFeeVariation(1, 181));
                    break;
                case ("product/price-variations/multiple-fee-var-in-range-upper.dmn"):
                    list.Add(CreateFeeVariation((decimal)0.49, 100));
                    list.Add(CreateFeeVariation((decimal)0.40, 79));
                    break;
                case ("product/price-variations/multiple-fee-var-in-range-lower.dmn"):
                    list.Add(CreateFeeVariation((decimal)-0.03, -100));
                    list.Add(CreateFeeVariation((decimal)-0.05, -79));
                    break;
                case ("product/price-variations/multiple-fee-var-under-range.dmn"):
                    list.Add(CreateFeeVariation((decimal)-0.06, -100));
                    list.Add(CreateFeeVariation((decimal)-0.04, -81));
                    break;
                case ("product/price-variations/multiple-fee-var-over-range.dmn"):
                    list.Add(CreateFeeVariation((decimal)0.4, 200));
                    list.Add(CreateFeeVariation((decimal)0.6, 200));
                    break;
                //iterative_cases
                case ("product/price-variations/iterative-fee-in-range-upper.dmn"):
                    if (feeCounter > 0)
                    {
                        list.Add(CreateFeeVariation((decimal)0.88, 178));
                        break;
                    }
                    list.Add(CreateFeeVariation((decimal)0.89, 179));
                    break;
                case ("product/price-variations/iterative-fee-in-range-lower.dmn"):
                    if (feeCounter > 0)
                    {
                        list.Add(CreateFeeVariation((decimal)-0.07, -178));
                        break;
                    }
                    list.Add(CreateFeeVariation((decimal)-0.08, -179));
                    break;
                case ("product/price-variations/iterative-fee-under-range.dmn"):
                    if (feeCounter > 0)
                    {
                        list.Add(CreateFeeVariation((decimal)-0.09, -180));
                        break;
                    }
                    list.Add(CreateFeeVariation((decimal)-0.1, -181));
                    break;
                case ("product/price-variations/iterative-fee-over-range.dmn"):
                    if (feeCounter > 0)
                    {
                        list.Add(CreateFeeVariation((decimal)1.01, 182));
                        break;
                    }
                    list.Add(CreateFeeVariation(1, 181));
                    break;
                case ("product/price-variations/iterative-multiple-fee-var-in-range-upper.dmn"):
                    if (feeCounter > 0)
                    {
                        list.Add(CreateFeeVariation((decimal)0.48, 100));
                        list.Add(CreateFeeVariation((decimal)0.40, 78));
                        break;
                    }
                    list.Add(CreateFeeVariation((decimal)0.49, 100));
                    list.Add(CreateFeeVariation((decimal)0.40, 79));
                    break;
                case ("product/price-variations/iterative-multiple-fee-var-in-range-lower.dmn"):
                    if (feeCounter > 0)
                    {
                        list.Add(CreateFeeVariation((decimal)-0.03, -100));
                        list.Add(CreateFeeVariation((decimal)-0.04, -78));
                        break;
                    }
                    list.Add(CreateFeeVariation((decimal)-0.03, -100));
                    list.Add(CreateFeeVariation((decimal)-0.05, -79));
                    break;
                case ("product/price-variations/iterative-multiple-fee-var-under-range.dmn"):
                    if (feeCounter > 0)
                    {
                        list.Add(CreateFeeVariation((decimal)-0.07, -101));
                        list.Add(CreateFeeVariation((decimal)-0.04, -81));
                        break;
                    }
                    list.Add(CreateFeeVariation((decimal)-0.06, -100));
                    list.Add(CreateFeeVariation((decimal)-0.04, -81));
                    break;
                case ("product/price-variations/iterative-multiple-fee-var-over-range.dmn"):
                    if (feeCounter > 0)
                    {
                        list.Add(CreateFeeVariation((decimal)0.5, 200));
                        list.Add(CreateFeeVariation((decimal)0.6, 201));
                        break;
                    }
                    list.Add(CreateFeeVariation((decimal)0.4, 200));
                    list.Add(CreateFeeVariation((decimal)0.6, 200));
                    break;
            }
            feeCounter = feeCounter + 1;
            return Task.FromResult(list);
        }

        public Task<List<InterestRateVariation>> ResolveInterestRateVariationDmn(string dmnContent, VariationDefinitionParams definitionParams)
        {
            var list = new List<InterestRateVariation>();
            {
                switch (dmnContent)
                {
                    case ("product/price-variations/int-rate-remove.dmn"):
                        list.Add(CreateIntRateVariation(-1));
                        break;
                    case ("product/price-variations/int-rate-remove2.dmn"):
                        list.Add(CreateIntRateVariation(-2));
                        break;
                    case ("product/price-variations/int-rate-add.dmn"):
                        list.Add(CreateIntRateVariation(1));
                        break;
                    case ("product/price-variations/int-rate-add2.dmn"):
                        list.Add(CreateIntRateVariation(2));
                        break;
                    case ("product/price-variations/int-rate-add-two-variations.dmn"):
                        list.Add(CreateIntRateVariation(1));
                        list.Add(CreateIntRateVariation(2));
                        break;
                    //iterative
                    case ("product/price-variations/iterative-int-rate-remove.dmn"):
                        if (intrateCounter > 0)
                        {
                            list.Add(CreateIntRateVariation((decimal)-0.99));
                            break;
                        }
                        list.Add(CreateIntRateVariation(-1));
                        break;
                    case ("product/price-variations/iterative-int-rate-remove2.dmn"):
                        if (intrateCounter > 0)
                        {
                            list.Add(CreateIntRateVariation((decimal)-1.99));
                            break;
                        }
                        list.Add(CreateIntRateVariation(-2));
                        break;
                    case ("product/price-variations/iterative-int-rate-add.dmn"):
                        if (intrateCounter > 0)
                        {
                            list.Add(CreateIntRateVariation((decimal)0.99));
                            break;
                        }
                        list.Add(CreateIntRateVariation(1));
                        break;
                    case ("product/price-variations/iterative-int-rate-add2.dmn"):
                        if (intrateCounter > 0)
                        {
                            list.Add(CreateIntRateVariation(2));
                            break;
                        }
                        list.Add(CreateIntRateVariation(2));
                        break;
                    case ("product/price-variations/iterative-int-rate-add-two-variations.dmn"):
                        if (intrateCounter > 0)
                        {
                            list.Add(CreateIntRateVariation(1));
                            list.Add(CreateIntRateVariation(2));
                            break;
                        }
                        list.Add(CreateIntRateVariation(1));
                        list.Add(CreateIntRateVariation(2));
                        break;
                }
                intrateCounter = intrateCounter + 1;
                return Task.FromResult(list);
            }
        }

        private FeeVariation CreateFeeVariation(decimal percentage, decimal fixedAmount)
        {
            return new FeeVariation
            {
                Percentage = percentage,
                FixedAmount = fixedAmount,
                VariationGroup = "Test group",
                VariationDescription = "Test description",
                Origin = PriceVariationOrigins.Product
            };
        }

        private InterestRateVariation CreateIntRateVariation(decimal percentage)
        {
            return new InterestRateVariation
            {
                Percentage = percentage,
                VariationGroup = "Test group",
                VariationDescription = "Test description",
                Origin = PriceVariationOrigins.Product
            };
        }
    }
}
