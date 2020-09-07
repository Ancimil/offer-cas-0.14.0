using System;
using System.Collections.Generic;
using System.Linq;
using PriceCalculation.Models.Pricing;

namespace Offer.Domain.AggregatesModel.ApplicationAggregate
{
    public class DecisionResponse
    {
        public List<Dictionary<string, object>> Results { get; set; }

        public List<InterestRateVariation> ToInterestRateVariations()
        {
            return Results?.Select(v => new InterestRateVariation
            {
                VariationGroup = v.GetValueOrDefault("variationGroup")?.ToString(),
                VariationDescription = v.GetValueOrDefault("variationDescription")?.ToString(),
                Percentage = Decimal.Parse(v.GetValueOrDefault("percentage", "0")?.ToString() ?? "0")
            }).ToList();
        }

        public List<FeeVariation> ToFeeVariations()
        {
            return Results?.Select(v => new FeeVariation
            {
                VariationGroup = v.GetValueOrDefault("variationGroup")?.ToString(),
                VariationDescription = v.GetValueOrDefault("variationDescription")?.ToString(),
                Percentage = Decimal.Parse(v.GetValueOrDefault("percentage", "0")?.ToString() ?? "0"),
                FixedAmount = Decimal.Parse(v.GetValueOrDefault("fixedAmount", "0")?.ToString() ?? "0"),
                LowerLimit = Decimal.Parse(v.GetValueOrDefault("lowerLimit", "0")?.ToString() ?? "0"),
                UpperLimit = Decimal.Parse(v.GetValueOrDefault("upperLimit", "0")?.ToString() ?? "0")
            }).ToList();
        }
    }
}
