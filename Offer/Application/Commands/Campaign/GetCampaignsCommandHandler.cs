using MediatR;
using MicroserviceCommon.ApiUtil;
using MicroserviceCommon.Application.Commands;
using MicroserviceCommon.Infrastructure.Idempotency;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Threading;
using Offer.API.Extensions;
using PriceCalculation.Models.LeadModel;

namespace Offer.API.Application.Commands
{

    public class GetCampaignsCommandHandler : IRequestHandler<GetCampaignsCommand, LeadList>
    {
        private readonly IMediator _mediator;
        private readonly ApiEndPoints _apiEndPoints;
        private readonly ILogger<GetCampaignsCommand> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public GetCampaignsCommandHandler(
            IMediator mediator,
            ApiEndPoints apiEndPoints,
            ILogger<GetCampaignsCommand> logger,
            IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _apiEndPoints = apiEndPoints ?? throw new ArgumentNullException(nameof(apiEndPoints));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<LeadList> Handle(GetCampaignsCommand message, CancellationToken cancellationToken)
        {
            var customerNumber = message.CustomerNumber;
            using (var httpClient = _httpClientFactory.CreateClient())
            {
                httpClient.AddDefaultJsonHeaders();
                   var builder = new UriBuilder(_apiEndPoints.GetServiceUrl("campaign") + "campaigns/leads?x-asee-auth=true");
                var query = HttpUtility.ParseQueryString(string.Empty);
                query["party-id"] = customerNumber;
                builder.Query = query.ToString();
                _logger.LogDebug("Campaign endpoint is: {campaignApiUrl}campaigns/{{leads}}", _apiEndPoints.GetServiceUrl("campaign"), customerNumber);

                try
                {
                    using (HttpResponseMessage response = await httpClient.GetAsync(builder.ToString()))
                    {
                        var res = await response.Content.ReadAsStringAsync();
                        if (res.Equals("") || ((int)response.StatusCode) >= 400)
                        {
                            _logger.LogError("Could not fetch campaigns for customer number {customerNumber}. Response is empty.", customerNumber);
                            // throw new Exception("Could not fetch Campaign data for party");
                            return null;
                        }
                        return (LeadList)CaseUtil.ConvertFromJsonToObject(res, typeof(LeadList));
                    }
                }
                catch (HttpRequestException e)
                {
                    _logger.LogError(e, "Could not fetch campaigns for customer number {customerNumber}", customerNumber);
                    return null;
                }
            }
        }
    }
    public class GetCampaignsCommandIdentifiedCommandHandler : IdentifiedCommandHandler<GetCampaignsCommand, LeadList>
    {
        public GetCampaignsCommandIdentifiedCommandHandler(IMediator mediator, IRequestManager requestManager) : base(mediator, requestManager)
        {

        }

        protected override LeadList CreateResultForDuplicateRequest()
        {
            return null;
        }
    }

}
