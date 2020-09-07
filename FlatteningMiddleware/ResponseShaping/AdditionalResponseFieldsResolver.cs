using MicroserviceCommon.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlatteningMiddleware.ResponseShaping
{
    public class AdditionalResponseFieldsResolver
    {
        private readonly IConfigurationService _configurationService;
        private readonly ILogger<AdditionalResponseFieldsResolver> _logger;

        public AdditionalResponseFieldsResolver(
            IConfigurationService configurationService,
            ILogger<AdditionalResponseFieldsResolver> logger)
        {
            _configurationService = configurationService ?? throw new ArgumentNullException(nameof(configurationService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<dynamic> AppendAdditionalFields(HttpContext httpContext, dynamic responseObject)
        {
            var router = (MvcAttributeRouteHandler)httpContext.GetRouteData().Routers.FirstOrDefault(r => r is MvcAttributeRouteHandler);
            if (router == null || router.Actions.Count() == 0)
            {
                return responseObject;
            }
            var endpoint = RemoveRouteParamsType(router.Actions[0].AttributeRouteInfo.Template).Trim('/');
            var additionalFieldsConfig = await _configurationService.GetEffective<AdditionalResponseFieldsConfiguration>("data-management/additional-fields/" + endpoint);



            return responseObject; // todo
        }

        private string RemoveRouteParamsType(string endpoint)
        {
            var endpointParts = endpoint.Split(":");
            if (endpointParts.Length == 0)
            {
                return endpoint;
            }
            for (int i = 1; i < endpointParts.Length; i++)
            {
                endpointParts[i] = endpointParts[i].Remove(0, endpointParts[i].IndexOf("}"));
            }
            return endpointParts.Join("");
        }
    }
}
