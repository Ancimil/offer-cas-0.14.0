using System;
using System.Collections.Generic;

namespace PriceCalculation.Models.Pricing
{
    public abstract class PricingCondition : ICloneable
    {
        public string Title { get; set; }
        public DateTime EffectiveDate { get; set; }
        public List<string> Currencies { get; set; }
        public string VariationsDefinition { get; set; }
        public string VariationsDefinitionDMN { get; set; }
        public string PricingRuleCurrency { get; set; }
        public string Periods { get; set; }
        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
