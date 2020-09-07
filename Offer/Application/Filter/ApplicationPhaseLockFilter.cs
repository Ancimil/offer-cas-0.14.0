using MicroserviceCommon.API.ApiUtils;
using MicroserviceCommon.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.Routing;
using Newtonsoft.Json.Linq;
using Offer.Domain.AggregatesModel.ApplicationAggregate;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Security.Claims;

namespace Offer.API.Application.Filter
{
    public class ApplicationPhaseLockFilterAttribute : ActionFilterAttribute
    {
        private static readonly string ROUTE_PARAM = "application-number";

        public async override Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var httpContext = context.HttpContext;
            var result = await httpContext.AuthenticateAsync("Bearer");
            httpContext.User = result.Principal;
            var username = httpContext.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    ?? httpContext.User?.Claims.Where(x => x.Type == "preferred_username").FirstOrDefault()?.Value;
            var isApi = username == null && httpContext.User != null && httpContext.User.Identity.IsAuthenticated;
            if(!isApi)
            {
                var svc = context.HttpContext.RequestServices;
                IApplicationRepository applicationRepository = svc.GetService(typeof(IApplicationRepository)) as IApplicationRepository;
                IConfigurationService configurationService = svc.GetService(typeof(IConfigurationService)) as IConfigurationService;
                var contextData = httpContext.GetRouteData();
                if (contextData != null
                    && contextData.Values.ContainsKey(ROUTE_PARAM)
                    && (httpContext.Request.Method.Equals("POST")
                    || httpContext.Request.Method.Equals("PATCH")
                    || httpContext.Request.Method.Equals("PUT")
                    || httpContext.Request.Method.Equals("DELETE")))
                {
                    var applicationNumber = contextData.Values[ROUTE_PARAM].ToString();
                    long applicationNumberLong = long.Parse(applicationNumber);
                    var application = await applicationRepository.GetAsync(applicationNumberLong);
                    var phase = application.Phase;
                    var status = EnumUtils.ToEnumString(application.Status);

                    var router = (MvcAttributeRouteHandler)contextData.Routers.FirstOrDefault(r => r is MvcAttributeRouteHandler);
                    if (router != null && router.Actions.Count() > 0)
                    {
                        var endpoint = ReplaceParamTypes(router.Actions[0].AttributeRouteInfo.Template);
                        endpoint = TrimStart(endpoint, "v1/offer/");
                        endpoint = TrimStart(endpoint, "v2/offer/");

                        if (phase != null)
                        {
                            var blockedUrlsData = await configurationService.GetEffective("offer/data-lock-rules/" + phase, "[]");
                            List<RouteFilterConfiguration> blockedUrls = JArray.Parse(blockedUrlsData).ToObject<List<RouteFilterConfiguration>>();
                            if (blockedUrls.Any(x => x.Route.Equals(endpoint) && x.Method.Equals(httpContext.Request.Method)))
                            {
                                context.Result = new StatusCodeResult(403);
                                return;
                            }
                        }
                    }
                }
            }
            await next();
        }
        private static string ReplaceParamTypes(string path)
        {
            return Regex.Replace(path, "(\\:.*?\\})", "}");
        }

        private static string TrimStart(string source, string toTrim)
        {
            string s = source;
            while (s.StartsWith(toTrim))
            {
                s = s.Substring(toTrim.Length);
            }
            return s;
        }
    }
}
