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

    public partial class ProductDocumentation
    {
        public DocumentContextKind DocumentContextKind { get; set; }
        public string ProductCode { get; set; }
        public Domain.AggregatesModel.ApplicationAggregate.ProductModel.PartyRoleEnum PartyRole { get; set; }
        public string CollateralKind { get; set; }
        public string DocumentName { get; set; }
        public string DocumentType { get; set; }
        public string WorkItemKind { get; set; }
        public string DocumentReviewPeriod { get; set; }
        public bool IsMandatory { get; set; }
        public string WorkItemPhase { get; set; }
        public bool IsComposedFromTemplate { get; set; }
        public string TemplateUrl { get; set; }
        public bool IsForSigning { get; set; }
        public bool IsForUpload { get; set; }
        public bool IsForPhysicalArchiving { get; set; }
        public bool IsInternal { get; set; }
        public bool SupportsMultipleFiles { get; set; } = false;
        public DocumentOrigin Origin = DocumentOrigin.Product;
        public bool IsForProposal { get; set; }
    }

    public partial class ProductDocumentationItems
    {
        public List<ProductDocumentation> Documentation;
    }

    public class GetProductDocumentationCommandHandler : IRequestHandler<GetProductDocumentationCommand, List<ProductDocumentation>>
    {
        private readonly IMediator _mediator;
        private readonly ApiEndPoints _apiEndPoints;
        private readonly ILogger<GetProductDocumentationCommand> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public GetProductDocumentationCommandHandler(IMediator mediator, ApiEndPoints apiEndPoints,
            ILogger<GetProductDocumentationCommand> logger,
            IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _apiEndPoints = apiEndPoints ?? throw new ArgumentNullException(nameof(apiEndPoints));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<List<ProductDocumentation>> GetProductDocumentation(string productCode, string customerId ="")
        {
            using (var httpClient = _httpClientFactory.CreateClient())
            {
                httpClient.AddDefaultJsonHeaders();
                try
                {
                    string checkCustomerId = string.IsNullOrEmpty(customerId) ? "" : "?customer-id=" + customerId;
                    _logger.LogInformation("Fetching documentation for product: {productCode}", productCode);
                    using (HttpResponseMessage response = await httpClient.GetAsync(_apiEndPoints.GetServiceUrl("product") + "products/" + productCode + "/documentation" + checkCustomerId))
                    {
                        var res = await response.Content.ReadAsStringAsync();
                        if (res.Equals(""))
                        {
                            _logger.LogError("Could not fetch documentation list for product code {productCode}", productCode);
                            throw new Exception("Could not fetch Documentation list");
                        }
                        var items = (ProductDocumentationItems)CaseUtil.ConvertFromJsonToObject(res, typeof(ProductDocumentationItems));

                        return items.Documentation;
                    }
                }
                catch (HttpRequestException e)
                {
                    _logger.LogError(e, "Could not fetch documentation list for product code {productCode}", productCode);
                    throw e;
                }
            }
        }

        public async Task<List<ProductDocumentation>> Handle(GetProductDocumentationCommand message, CancellationToken cancellationToken)
        {
            List<ProductDocumentation> productDocs = await GetProductDocumentation(message.ProductCode, message.CustomerId);
            return productDocs;
        }
    }

    public class GetProductDocumentationIdentifiedCommandHandler : IdentifiedCommandHandler<GetProductDocumentationCommand, List<ProductDocumentation>>
    {
        public GetProductDocumentationIdentifiedCommandHandler(IMediator mediator, IRequestManager requestManager) : base(mediator, requestManager)
        {

        }

        protected override List<ProductDocumentation> CreateResultForDuplicateRequest()
        {
            return null;
        }
    }
}
