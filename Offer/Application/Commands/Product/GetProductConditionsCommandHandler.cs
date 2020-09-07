using AutoMapper;
using MediatR;
using MicroserviceCommon.ApiUtil;
using MicroserviceCommon.Application.Commands;
using MicroserviceCommon.Infrastructure.Idempotency;
using Microsoft.Extensions.Logging;
using Offer.API.Extensions;
using PriceCalculation.Models.Pricing;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Offer.API.Application.Commands
{

    public class GetProductConditionsCommandHandler : IRequestHandler<GetProductConditionsCommand, bool>
    {
        private readonly IMediator _mediator;
        private readonly ApiEndPoints _apiEndPoints;
        private readonly ILogger<GetProductConditionsCommand> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public GetProductConditionsCommandHandler(
            IMediator mediator,
            ApiEndPoints apiEndPoints,
            ILogger<GetProductConditionsCommand> logger,
            IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _apiEndPoints = apiEndPoints ?? throw new ArgumentNullException(nameof(apiEndPoints));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Conditions>GetProductConditions(string productCode)
        {
            using (var httpClient = _httpClientFactory.CreateClient())
            {
                httpClient.AddDefaultJsonHeaders();

                _logger.LogDebug("Product endpoint is: {productApiUrl}products/{{productCode}}/conditions", _apiEndPoints.GetServiceUrl("product"), productCode);

                try
                {
                    using (HttpResponseMessage response = await httpClient.GetAsync(_apiEndPoints.GetServiceUrl("product") + "products/" + productCode + "/conditions"))
                    {
                        var res = await response.Content.ReadAsStringAsync();
                        if (res.Equals(""))
                        {
                            _logger.LogError("Could not fetch product conditions for product {productCode}", productCode);
                            throw new Exception("Could not fetch Product conditions");
                        }
                        return (Conditions)CaseUtil.ConvertFromJsonToObject(res, typeof(Conditions));
                    }
                }
                catch (HttpRequestException e)
                {
                    _logger.LogError(e, "Could not fetch product conditions for product {productCode}", productCode);
                    throw e;
                }
            }
        }

        public async Task<bool> Handle(GetProductConditionsCommand message, CancellationToken cancellationToken)
        {
            var productCode = message.ArrangementRequest.ProductCode;
            Conditions productConditions = await GetProductConditions(productCode);
            bool commandSuccess = true;
            try
            {
                Conditions snapshot = Mapper.Map<Conditions, Conditions>(productConditions);
                message.ArrangementRequest.Conditions = snapshot;
            }
            catch (Exception exp)
            {
                commandSuccess = false;
                string s = exp.Message;
            }
            return commandSuccess;
        }
    }
    public class GetProductConditionsIdentifiedCommandHandler : IdentifiedCommandHandler<GetProductConditionsCommand, bool>
    {
        public GetProductConditionsIdentifiedCommandHandler(IMediator mediator, IRequestManager requestManager) : base(mediator, requestManager)
        {

        }

        protected override bool CreateResultForDuplicateRequest()
        {
            return true;
        }
    }

}
