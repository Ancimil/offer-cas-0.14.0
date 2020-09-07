using MicroserviceCommon.Services;
using Newtonsoft.Json;
using PriceCalculation.Models.Pricing;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PriceCalculatationTest.Services
{
    public class ConfigurationService : IConfigurationService
    {
        public Task<string> ForceEffective(string key, string defaultValue = null)
        {
            throw new NotImplementedException();
        }

        public Task<Dictionary<string, string>> ForceEffectiveUnder(string key, Dictionary<string, string> defaultValue = null)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetEffective(string key, string defaultValue = null)
        {
            string res;
            switch (key)
            {
                case "offer/currency-conversion-method":
                    res = "Buy to middle";
                    break;
                case "offer/price-calculation/default-parameters":
                    var defaultParameters = new VariationDefinitionParams
                    {
                        CollateralModel = "co-debtor",
                        CustomerSegment = "professional"
                    };
                    res = JsonConvert.SerializeObject(defaultParameters);
                    break;
                case "offer/price-calculation/bundle-variation-group":
                    res = "product-bundling-discount";
                    break;
                default:
                    if (defaultValue != null)
                    {
                        res = defaultValue;
                        break;
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
            }
            return Task.FromResult(res);
        }

        public Task<T> GetEffective<T>(string key, string defaultValue = null)
        {
            return Task.FromResult(JsonConvert.DeserializeObject<T>(GetEffective(key, defaultValue).Result));
        }

        public Task<Dictionary<string, string>> GetEffectiveUnder(string key, Dictionary<string, string> defaultValue = null)
        {
            throw new NotImplementedException();
        }

        public Task<Dictionary<string, T>> GetEffectiveUnder<T>(string key, Dictionary<string, T> defaultValue = null)
        {
            throw new NotImplementedException();
        }

        public void RegisterTopic(string topic, IConfigurationEventBusHandler handler)
        {
            throw new NotImplementedException();
        }
    }
}
