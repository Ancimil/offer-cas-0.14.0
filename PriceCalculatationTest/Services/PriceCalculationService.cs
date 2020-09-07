using PriceCalculation.Models.Pricing;
using PriceCalculation.Models.Product;
using PriceCalculation.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PriceCalculatationTest.Services
{
    public class PriceCalculationService : IPriceCalculationService
    {

        public Task<List<FeeVariation>> ResolveFeeVariationDmn(string dmnContent, VariationDefinitionParams definitionParams, string variationDefinition = "")
        {
            throw new NotImplementedException();
        }

        public Task<List<InterestRateVariation>> ResolveInterestRateVariationDmn(string dmnContent, VariationDefinitionParams definitionParams, string variationDefinition = "")
        {
            var list = new List<InterestRateVariation>();
            switch (dmnContent)
            {
                default:
                    break;
            }
            return Task.FromResult(list);
        }

        public Task<ProductConditions> ReadVariationDefinitions(ProductConditions condition)
        {
            throw new NotImplementedException();
        }

        private InterestRateVariation CreateIntRateVariation(decimal percentage, string variationGroup = "Test group",
            PriceVariationOrigins origin = PriceVariationOrigins.Product, string description = "Test description")
        {
            return new InterestRateVariation
            {
                Percentage = percentage,
                VariationGroup = variationGroup,
                VariationDescription = description,
                Origin = origin
            };
        }
    }
}
