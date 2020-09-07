using MediatR;
using MicroserviceCommon.ApiUtil;
using MicroserviceCommon.Application.Commands;
using MicroserviceCommon.Infrastructure.Idempotency;
using Microsoft.Extensions.Logging;
using Offer.API.Extensions;
using Offer.Domain.AggregatesModel.ApplicationAggregate;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Offer.API.Application.Commands
{

    public class GetRelatedProductsCommandHandler : IRequestHandler<GetRelatedProductsCommand, ProductList>
    {
        private readonly IMediator _mediator;
        private readonly ApiEndPoints _apiEndPoints;
        private readonly ILogger<GetAvailableProductsCommand> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IArrangementRequestRepository _arrangementRequestRepository;

        public GetRelatedProductsCommandHandler(
            IMediator mediator,
            ApiEndPoints apiEndPoints,
            ILogger<GetAvailableProductsCommand> logger,
            IHttpClientFactory httpClientFactory,
            IArrangementRequestRepository applicationDocumentsRepository)
        {
            _httpClientFactory = httpClientFactory;
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _apiEndPoints = apiEndPoints ?? throw new ArgumentNullException(nameof(apiEndPoints));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _arrangementRequestRepository = applicationDocumentsRepository ?? throw new ArgumentNullException(nameof(applicationDocumentsRepository));
        }

        public async Task<ProductList> Handle(GetRelatedProductsCommand message, CancellationToken cancellationToken)
        {
            using (var httpClient = _httpClientFactory.CreateClient())
            {
                httpClient.AddDefaultJsonHeaders();
                var query = "?";
                if (!string.IsNullOrEmpty(message.ChannelCode))
                {
                    query += "channel-code=" + message.ChannelCode + "&";
                }
                if (!string.IsNullOrEmpty(message.CustomerId))
                {
                    query += "customer-id=" + message.CustomerId;
                }
                if (!string.IsNullOrEmpty(message.ProductCode))
                {
                    try
                    {
                        using (HttpResponseMessage response = await httpClient.GetAsync(_apiEndPoints.GetServiceUrl("product") + "products/" +
                            message.ProductCode + "/related-products" + query))
                        {
                            var res = await response.Content.ReadAsStringAsync();
                            if (string.IsNullOrEmpty(res))
                            {
                                _logger.LogError("Could not fetch related products for product: {ProductCode}. Response is empty", message.ProductCode);
                                throw new Exception("Could not fetch Related Products");
                            }
                            var productData = (ProductList)CaseUtil.ConvertFromJsonToObject(res, typeof(ProductList));
                            return productData;
                        }
                    }
                    catch (HttpRequestException e)
                    {
                        _logger.LogError(e, "Could not fetch related products for product: {ProductCode}", message.ProductCode);
                        throw e;
                    }
                } else
                {
                    try
                    {
                        using (HttpResponseMessage response = await httpClient.GetAsync(_apiEndPoints.GetServiceUrl("product") + "products" + query))
                        {
                            var res = await response.Content.ReadAsStringAsync();
                            if (string.IsNullOrEmpty(res))
                            {
                                _logger.LogError("Could not fetch related products for product: {ProductCode}. Response is empty", message.ProductCode);
                                throw new Exception("Could not fetch Related Products");
                            }
                            var productData = (ProductList)CaseUtil.ConvertFromJsonToObject(res, typeof(ProductList));
                            return productData;
                        }
                    }
                    catch (HttpRequestException e)
                    {
                        _logger.LogError(e, "Could not fetch related products for product: {ProductCode}", message.ProductCode);
                        throw e;
                    }
                }
            }
        }
    }
    public class GetRelatedProductsIdentifiedCommandHandler : IdentifiedCommandHandler<GetRelatedProductsCommand, ProductList>
    {
        public GetRelatedProductsIdentifiedCommandHandler(IMediator mediator, IRequestManager requestManager) : base(mediator, requestManager)
        {

        }

        protected override ProductList CreateResultForDuplicateRequest()
        {
            return null;
        }
    }

    public class ProductList
    {
        public List<ProductData> Products { get; set; }
    }
}
