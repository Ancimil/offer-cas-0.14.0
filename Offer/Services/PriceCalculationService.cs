using MicroserviceCommon.ApiUtil;
using MicroserviceCommon.Authentication;
using Newtonsoft.Json;
using Offer.Domain.AggregatesModel.ApplicationAggregate;
using PriceCalculation.Models.Pricing;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using MicroserviceCommon.Services;
using Offer.API.Extensions;
using PriceCalculation.Services;
using PriceCalculation.Models.Product;
using PriceCalculation.Calculations;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using DigitalToolset.ApiExtensions;
using System.IO;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Logging;

namespace Offer.API.Services
{
    public class PriceCalculationService : IPriceCalculationService
    {
        private readonly ApiEndPoints _apiEndPoints;
        private readonly TokenAuthentication tokenAuthentication;
        private readonly IConfigurationService configurationService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<PriceCalculationService> _logger;

        public PriceCalculationService(
            ApiEndPoints apiEndPoints,
            TokenAuthentication tokenAuthentication,
            IConfigurationService configurationService,
            IHttpClientFactory httpClientFactory,
            IHttpContextAccessor httpContextAccessor,
            IServiceProvider serviceProvider,
            ILogger<PriceCalculationService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _apiEndPoints = apiEndPoints ?? throw new ArgumentNullException(nameof(apiEndPoints));
            this.tokenAuthentication = tokenAuthentication ?? throw new ArgumentNullException(nameof(tokenAuthentication));
            this.configurationService = configurationService ?? throw new ArgumentNullException(nameof(configurationService));
            this._httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<List<InterestRateVariation>> ResolveInterestRateVariationDmn(string dmnContent, VariationDefinitionParams definitionParams, string variationDefinition = "")
        {
            var responseForDmn = await GetGenericParamsForDmn(definitionParams, variationDefinition.Replace(".dmn", "-configuration"));

            var decisionRequest = new DecisionRequest
            {
                Data = responseForDmn ?? definitionParams,
                RuleDefinition = dmnContent
            };
            var resolvedDmn = await ResolveVariationDmn(decisionRequest);
            var variations = resolvedDmn.ToInterestRateVariations();
            if (variations != null)
            {
                foreach (InterestRateVariation interestRateVariation in variations)
                {
                    interestRateVariation.Origin = PriceVariationOrigins.Product;
                }
            }
            return variations;
        }

        public async Task<dynamic> GetGenericParamsForDmn(VariationDefinitionParams definitionParams, string variationConfigurationUrl = "")
        {
            try
            {
                string checkConfig = await configurationService.GetEffective(variationConfigurationUrl);
                dynamic responseForDmn = null;
                if (!string.IsNullOrEmpty(checkConfig))
                {
                    List<KeyValuePair<string, StringValues>> query = _httpContextAccessor.HttpContext.Request.Query.ToList();
                    var body = _httpContextAccessor.HttpContext.Request.Body;
                    dynamic originalBody;
                    var newBody = _httpContextAccessor.HttpContext.Request.Body;
                    newBody.Seek(0, System.IO.SeekOrigin.Begin);
                    using (var streamReader = new StreamReader(newBody, Encoding.UTF8, true, 1024, true))
                    {
                        string bodyRead = streamReader.ReadToEnd();
                        originalBody = JObject.Parse(bodyRead);
                    }
                    _httpContextAccessor.HttpContext.Request.Body.Position = 0;
                    Dictionary<string, string> dict = query.ToDictionary(x => x.Key.ToString(), x => x.Value.ToString());

                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var _dmnRequestExtender = scope.ServiceProvider.GetRequiredService<DmnRequestExtender>();
                        responseForDmn = await _dmnRequestExtender.DmnRequestExtenderResponse(variationConfigurationUrl, dict, originalBody);
                    }

                    var defParamse = JObject.FromObject(definitionParams);

                    foreach (var rp in defParamse)
                    {
                        responseForDmn.Add(rp.Key.ToString(), rp.Value);
                    }
                }
                return responseForDmn;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, "Get generic params for dmn variations error");
                return null;
            }
        }

        public async Task<List<FeeVariation>> ResolveFeeVariationDmn(string dmnContent, VariationDefinitionParams definitionParams, string variationDefinition = "")
        {
            var responseForDmn = await GetGenericParamsForDmn(definitionParams, variationDefinition.Replace(".dmn", "-configuration"));

            var decisionRequest = new DecisionRequest
            {
                Data = responseForDmn ?? definitionParams,
                RuleDefinition = dmnContent
            };
            var resolvedDmn = await ResolveVariationDmn(decisionRequest);
            var variations = resolvedDmn.ToFeeVariations();
            if (variations != null)
            {
                foreach (FeeVariation feeVariation in variations)
                {
                    feeVariation.Origin = PriceVariationOrigins.Product;
                }
            }
            return variations;
        }

        public async Task<ProductConditions> ReadVariationDefinitions(ProductConditions conditions)
        {
            if (conditions == null)
            {
                return null;
            }
            if (conditions.InterestRates != null)
            {
                foreach (InterestRateCondition rate in conditions.InterestRates)
                {
                    if (rate.VariationsDefinition != null)
                    {
                        rate.VariationsDefinitionDMN = await configurationService.GetEffective(rate.VariationsDefinition);
                    }
                    if (rate.UpperLimitVariationsDefinition != null)
                    {
                        rate.UpperLimitVariationsDefinitionDMN = await configurationService.GetEffective(rate.UpperLimitVariationsDefinition);
                    }
                    if (rate.LowerLimitVariationsDefinition != null)
                    {
                        rate.LowerLimitVariationsDefinitionDMN = await configurationService.GetEffective(rate.LowerLimitVariationsDefinition);
                    }
                }
            }
            if (conditions.Fees != null)
            {
                foreach (FeeCondition fee in conditions.Fees)
                {
                    if (fee.VariationsDefinition != null)
                    {
                        fee.VariationsDefinitionDMN = await configurationService.GetEffective(fee.VariationsDefinition);
                    }
                }
            }

            return conditions.Copy();
        }

        private async Task<DecisionResponse> ResolveVariationDmn(DecisionRequest decisionRequest)
        {
            using (var httpClient = _httpClientFactory.CreateClient())
            {
                httpClient.AddDefaultJsonHeaders();
                var dataAsString = JsonConvert.SerializeObject(decisionRequest);
                var requestContent = new StringContent(dataAsString, Encoding.UTF8, "application/json");
                requestContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                try
                {
                    using (HttpResponseMessage response = await httpClient.PostAsync(_apiEndPoints.GetServiceUrl("decision") + "decisions?x-asee-auth=true", requestContent))
                    {
                        var res = await response.Content.ReadAsStringAsync();
                        if (res.Equals(""))
                        {
                            throw new Exception("Could not resolve DMN variations table");
                        }
                        var decisionResponse = (DecisionResponse)CaseUtil.ConvertFromJsonToObject(res, typeof(DecisionResponse));
                        return decisionResponse;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Decision endpoint: " + _apiEndPoints.GetServiceUrl("decision") + "decisions");
                    Console.WriteLine("Request content: " + dataAsString);
                    throw e;
                }
            }
        }
    }
}
