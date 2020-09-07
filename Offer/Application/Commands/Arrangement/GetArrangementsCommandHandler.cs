using MediatR;
using MicroserviceCommon.Application.Commands;
using MicroserviceCommon.Infrastructure.Idempotency;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using System.Net.Http;
using MicroserviceCommon.Services;
using Newtonsoft.Json.Linq;
using MicroserviceCommon.ApiUtil;
using System.Threading;
using Offer.API.Extensions;

namespace Offer.API.Application.Commands
{
    public class GetArrangementsCommandHandler : IRequestHandler<GetArrangementsCommand, JArray>
    {
        private readonly ILogger<GetArrangementsCommandHandler> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfigurationService _configurationService;
        private readonly ApiEndPoints _apiEndPoints;

        public GetArrangementsCommandHandler(
            ILogger<GetArrangementsCommandHandler> logger,
            IHttpClientFactory httpClientFactory,
            ApiEndPoints apiEndPoints,
            IConfigurationService configurationService
            )
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _apiEndPoints = apiEndPoints ?? throw new ArgumentNullException(nameof(apiEndPoints));
            _configurationService = configurationService ?? throw new ArgumentNullException(nameof(configurationService));
        }

        public async Task<JArray> Handle(GetArrangementsCommand message, CancellationToken cancellationToken)
        {
            using (var httpClient = _httpClientFactory.CreateClient())
            {
                httpClient.AddDefaultJsonHeaders();
                try
                {
                    var activeStatuses = message.ActiveStatuses ?? _configurationService.GetEffective("arrangement/active-statuses", "effective").Result;                    
                    var activeRoles = message.ActiveRoles ?? _configurationService.GetEffective("offer/exposure/active-roles", "customer,guarantor,co-debtor").Result;
                    var arrangementType = message.ArrangementType ?? "term-loan,overdraft-facility,credit-facility,credit-card-facility";
                    JArray list = null;
                    using (HttpResponseMessage response = await httpClient.GetAsync(_apiEndPoints.GetServiceUrl("arrangement") +
                        "arrangements/?customer-id=" + message.CustomerNumber + "&statuses=" + activeStatuses +
                        "&page-size=1000&role-kinds=" + activeRoles + "&kinds=" + arrangementType + "&x-asee-auth=true"))
                    {
                        var res = await response.Content.ReadAsStringAsync();
                        if (!res.Equals(""))
                        {
                            JObject json = JObject.Parse(res);
                            list = (JArray)json["items"];
                        }
                        return list;
                    }
                }
                catch (HttpRequestException e)
                {
                    _logger.LogError(e, "Could not fetch arrangements data for party number: {partyNumber}", message.CustomerNumber);
                    throw e;
                }
            }
        }
    }
    public class GetArrangementsIdentifiedCommandHandler : IdentifiedCommandHandler<GetArrangementsCommand, JArray>
    {
        public GetArrangementsIdentifiedCommandHandler(IMediator mediator, IRequestManager requestManager) : base(mediator, requestManager)
        {
        }

        protected override JArray CreateResultForDuplicateRequest()
        {
            return null;
        }
    }
}
