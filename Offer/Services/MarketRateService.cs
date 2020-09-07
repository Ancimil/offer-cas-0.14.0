using MicroserviceCommon.ApiUtil;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Extensions.Caching.Memory;
using Offer.API.Extensions;
using PriceCalculation.Services;
using PriceCalculation.Models.MarketRates;

namespace Offer.API.Services
{
    public class MarketRateService : IMarketRatesService
    {
        private readonly ApiEndPoints _apiEndPoints;
        private readonly IMemoryCache _cache;
        private readonly IHttpClientFactory _httpClientFactory;

        public MarketRateService(
            ApiEndPoints apiEndPoints,
            IMemoryCache memoryCache,
            IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
            _apiEndPoints = apiEndPoints ?? throw new ArgumentNullException(nameof(apiEndPoints));
            _cache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
        }

        public async Task<RateValue> GetRate(string rateCode, DateTime dateTime)
        {
            if (!_cache.TryGetValue(rateCode, out RateValue rateValue))
            {
                using (var httpClient = _httpClientFactory.CreateClient())
                {
                    httpClient.AddDefaultJsonHeaders();
                    var queryString = "";
                    if (dateTime != null)
                    {
                        var query = HttpUtility.ParseQueryString(string.Empty);
                        //query["date"] = dateTime.ToString("yyyy-MM-ddTHH:mm:ssZ");
                        queryString = query.ToString();
                    }
                    try
                    {
                        var builder = new UriBuilder(_apiEndPoints.GetServiceUrl("market-rates") + "interest-rate-values/" + rateCode + "?x-asee-auth=true");
                        if (!queryString.Equals(""))
                        {
                            builder.Query = queryString;
                        }
                        using (HttpResponseMessage response = await httpClient.GetAsync(builder.ToString()))
                        {
                            var res = await response.Content.ReadAsStringAsync();
                            if (res.Equals(""))
                            {
                                throw new Exception("Could not fetch Product conditions");
                            }
                            rateValue = (RateValue)CaseUtil.ConvertFromJsonToObject(res, typeof(RateValue));

                            // Set cache options.
                            var cacheEntryOptions = new MemoryCacheEntryOptions()
                                // Keep in cache for this time, reset time if accessed.
                                .SetSlidingExpiration(GetExpirationTimeSpanForKey(rateCode));

                            // Save data in cache.
                            _cache.Set(rateCode, rateValue, cacheEntryOptions);
                        }
                    }
                    catch (HttpRequestException e)
                    {
                        throw e;
                    }
                }
            }
            return rateValue;
        }

        public async Task<RateListModel> GetRateList(string listCode)
        {
            using (var httpClient = _httpClientFactory.CreateClient())
            {
                httpClient.AddDefaultJsonHeaders();
                var builder = new UriBuilder(_apiEndPoints.GetServiceUrl("market-rates") + "interest-rates?x-asee-auth=true")
                {
                    Port = -1
                };
                var query = HttpUtility.ParseQueryString(string.Empty);
                query["list-code"] = listCode;
                builder.Query = query.ToString();
                try
                {
                    using (HttpResponseMessage response = await httpClient.GetAsync(builder.ToString()))
                    {
                        var res = await response.Content.ReadAsStringAsync();
                        if (res.Equals(""))
                        {
                            throw new Exception("Could not fetch Product conditions");
                        }
                        var list = (RateListModel)CaseUtil.ConvertFromJsonToObject(res, typeof(RateListModel)); ;
                        return list;
                    }
                }
                catch (HttpRequestException e)
                {
                    throw e;
                }
            }
        }

        private TimeSpan GetExpirationTimeSpanForKey(string key)
        {
            return DateTime.Today.AddDays(1).Subtract(DateTime.Now);
        }
    }
}
