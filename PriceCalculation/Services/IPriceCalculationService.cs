using PriceCalculation.Models.Pricing;
using PriceCalculation.Models.Product;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PriceCalculation.Services
{
    public interface IPriceCalculationService
    {
        Task<List<InterestRateVariation>> ResolveInterestRateVariationDmn(string dmnContent, VariationDefinitionParams definitionParams, string variationDefinition = "");
        Task<List<FeeVariation>> ResolveFeeVariationDmn(string dmnContent, VariationDefinitionParams definitionParams, string variatonDefinition = "");
        Task<ProductConditions> ReadVariationDefinitions(ProductConditions condition);
    }
}
