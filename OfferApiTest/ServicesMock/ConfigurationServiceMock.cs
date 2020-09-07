using System.Collections.Generic;
using System.Threading.Tasks;
using MicroserviceCommon.Services;

namespace OfferApiTest
{
    internal class ConfigurationServiceMock : IConfigurationService
    {
        public Task<string> ForceEffective(string key, string defaultValue = null)
        {
            throw new System.NotImplementedException();
        }

        public Task<Dictionary<string, string>> ForceEffectiveUnder(string key, Dictionary<string, string> defaultValue = null)
        {
            throw new System.NotImplementedException();
        }

        public Task<string> GetEffective(string key, string defaultValue = null)
        {
            var res = "";
            switch (key)
            {
                case "offer/currency-conversion-method":
                    res = "Buy to middle";
                    break;
                case "offer/price-calculation/default-parameters":
                    res = "{\r\n    \"collateral-model\": \"co-debtor\",\r\n    \"product-bundling\": \"0001\",\r\n    \"risk-score\": 1,\r\n    \"customer-segment\": \"professional\"\r\n}";
                    break;
            }
            return Task.FromResult(res);
        }

        public Task<T> GetEffective<T>(string key, string defaultValue = null)
        {
            throw new System.NotImplementedException();
        }

        public Task<string> GetEffectiveUnder(string key, string defaultValue = null)
        {
            throw new System.NotImplementedException();
        }

        public Task<Dictionary<string, string>> GetEffectiveUnder(string key, Dictionary<string, string> defaultValue = null)
        {
            throw new System.NotImplementedException();
        }

        public Task<Dictionary<string, T>> GetEffectiveUnder<T>(string key, Dictionary<string, T> defaultValue = null)
        {
            throw new System.NotImplementedException();
        }

        public void RegisterTopic(string topic, IConfigurationEventBusHandler handler)
        {
            throw new System.NotImplementedException();
        }
    }
}