using MediatR;
using MicroserviceCommon.Application.Commands;
using MicroserviceCommon.Infrastructure.Idempotency;
using Offer.Domain.AggregatesModel.ApplicationAggregate;
using Offer.Domain.AggregatesModel.ApplicationAggregate.ProductModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Offer.API.Application.Commands
{
    public class AvailableProductsResponse
    {
        public List<BasicProductInfo> Items { get; set; }
    }

    public class GetAvailableProductsCommandHandler : IRequestHandler<GetAvailableProductsCommand, AvailableProductsResponse>
    {
        private readonly IMediator _mediator;
        private readonly IArrangementRequestRepository _arrangementRequestRepository;
        private readonly IApplicationRepository _applicationRepository;

        public GetAvailableProductsCommandHandler(
            IMediator mediator,
            IArrangementRequestRepository applicationDocumentsRepository,
            IApplicationRepository applicationRepository)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _arrangementRequestRepository = applicationDocumentsRepository ?? throw new ArgumentNullException(nameof(applicationDocumentsRepository));
            _applicationRepository = applicationRepository ?? throw new ArgumentNullException(nameof(applicationRepository));
        }

        public async Task<AvailableProductsResponse> Handle(GetAvailableProductsCommand message, CancellationToken cancellationToken)
        {
            var response = new AvailableProductsResponse
            {
                Items = new List<BasicProductInfo>()
            };

            var bundledComponents = _arrangementRequestRepository.GetBundledComponentsForApplication(message.ApplicationId);
            var validationResults = _arrangementRequestRepository.ValidateArrangementRequests(message.ApplicationId);

            // && (v.IsAbstractOrigin ? v.Count == 0 : true)
            var available = validationResults
                .Where(v => Math.Max(v.Count, v.ParentCount) < v.MaximalNumberOfInstances)
                .SelectMany(v => v.ArrangementRequests)
                .Where(r => r.IsAbstractOrigin ? !(r.Enabled ?? false) : true)
                .Select(a => new BasicProductInfo
                {
                    IsRelated = false,
                    Kind = a.ProductSnapshot.Kind,
                    ProductName = a.ProductName,
                    ArrangementRequestId = a.ArrangementRequestId,
                    BenefitsInfo = a.ProductSnapshot.BenefitsInfo,
                    Description = a.ProductSnapshot.Description,
                    ProductCode = a.ProductSnapshot.ProductCode,
                    ImageUrl = a.ProductSnapshot.ImageUrl
                })
                .ToList();
            response.Items.AddRange(available);

            var app = await _applicationRepository.GetAsync(message.ApplicationId, "arrangement-requests");
            var mainArrangement = app.ArrangementRequests.Where(a => app.ProductCode.Equals(a.ProductCode)).FirstOrDefault();
            var relatedProducts = mainArrangement != null ? mainArrangement.ProductSnapshot?.RelatedProducts : null;
            if (!string.IsNullOrEmpty(app._AvailableProducts))
            {
                foreach (var code in app.AvailableProducts.Where(x => !string.IsNullOrEmpty(x)).ToList())
                {
                    var alreadyAddedRelatedProductCodes = response.Items.Select(a => a.ProductCode).ToList();

                    if (!response.Items.Any(i => i.ProductCode.Equals(code)) && !app.ArrangementRequests.Any(v => v.ProductCode.Equals(code)
                    ))
                    {
                        var getProduct = new IdentifiedCommand<GetProductDataCommand, ProductData>(new GetProductDataCommand
                        {
                            ProductCode = code

                        }, new Guid());
                        ProductData product = await _mediator.Send(getProduct);
                        if (product != null && product.ChannelAvailability.Contains(message?.ChannelCode ?? string.Empty)
                            &&
                            (product.Kind == ProductKinds.AbstractProduct ?
                                (string.IsNullOrEmpty(product.Variants) ? new string[0] : product.Variants.Split(","))
                                    .Intersect(alreadyAddedRelatedProductCodes).Count() == 0 : true)
                                    )
                        {

                            response.Items.Add(new BasicProductInfo
                            {
                                IsRelated = !string.IsNullOrEmpty(relatedProducts) ? relatedProducts.Contains(product.ProductCode) : false,
                                Kind = product.Kind,
                                ProductName = product.Name,
                                ProductCode = product.ProductCode,
                                BenefitsInfo = product.BenefitsInfo,
                                Description = product.Description,
                                ImageUrl = product.ImageUrl
                            });
                        }
                    }
                }
            }

            var getRelatedProducts = new IdentifiedCommand<GetRelatedProductsCommand, ProductList>(new GetRelatedProductsCommand
            {
                CustomerId = message.CustomerId,
                ChannelCode = message.ChannelCode,
                ProductCode = app.ProductCode
            }, new Guid());
            ProductList productList = await _mediator.Send(getRelatedProducts);
            if (productList != null && string.IsNullOrEmpty(app._AvailableProducts))
            {
                var alreadyAddedRelatedProductCodes = response.Items.Select(a => a.ProductCode).ToList();
                response.Items.AddRange(
                    productList.Products
                    .Where(p =>
                        !alreadyAddedRelatedProductCodes.Contains(p.ProductCode) &&
                        (p.Kind == ProductKinds.AbstractProduct ?
                            (string.IsNullOrEmpty(p.Variants) ? new string[0] : p.Variants.Split(","))
                                .Intersect(alreadyAddedRelatedProductCodes).Count() == 0 : true)
                    )
                    .Select(p => new BasicProductInfo
                    {
                        IsRelated = true,
                        Kind = p.Kind,
                        ProductName = p.Name,
                        ProductCode = p.ProductCode,
                        BenefitsInfo = p.BenefitsInfo,
                        Description = p.Description,
                        ImageUrl = p.ImageUrl
                    }));
            }

            return response;
        }

        private async Task<List<BundleComponentInfo>> GetAbstractProductsVariants(List<BundleComponentInfo> bundleComponents)
        {
            var abstractComponentsList = new List<BundleComponentInfo>();
            var abstractBundleComponents = bundleComponents.Where(b => b.Kind == ArrangementKind.Abstract).ToList();
            foreach (var component in abstractBundleComponents)
            {
                var getProductData = new IdentifiedCommand<GetProductDataCommand, ProductData>(
                    new GetProductDataCommand { ProductCode = component.ProductCode }, new Guid());
                var data = await _mediator.Send(getProductData);
                var variants = data.Variants?.Split(",");
                foreach (var variant in variants)
                {
                    if (abstractComponentsList.Any(a => a.ProductCode == variant))
                    {
                        continue;
                    }
                    abstractComponentsList.Add(new BundleComponentInfo
                    {
                        Kind = ArrangementKind.OtherProductArrangement,
                        ProductCode = variant,
                        MaximalNumberOfInstances = component.MaximalNumberOfInstances,
                        MinimalNumberOfInstances = component.MinimalNumberOfInstances,
                        ProductName = component.ProductName
                    });
                }
            }
            return abstractComponentsList;
        }
    }
    public class GetAvailableProductsIdentifiedCommandHandler : IdentifiedCommandHandler<GetAvailableProductsCommand, AvailableProductsResponse>
    {
        public GetAvailableProductsIdentifiedCommandHandler(IMediator mediator, IRequestManager requestManager) : base(mediator, requestManager)
        {

        }

        protected override AvailableProductsResponse CreateResultForDuplicateRequest()
        {
            return null;
        }
    }

}
