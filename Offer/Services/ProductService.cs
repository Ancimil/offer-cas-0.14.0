using AutoMapper;
using MediatR;
using MicroserviceCommon.Application.Commands;
using Offer.API.Application.Commands;
using Offer.Domain.AggregatesModel.ApplicationAggregate;
using Offer.Domain.Services;
using System;
using System.Threading.Tasks;

namespace Offer.API.Services
{
    public class ProductService : IProductService
    {
        private readonly IMediator _mediator;

        public ProductService(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        public async Task<ProductSnapshot> GetProductData(string productCode, string include = null, string customerId = "")
        {
            var getProductData = new IdentifiedCommand<GetProductDataCommand, ProductData>(
                new GetProductDataCommand
                {
                    ProductCode = productCode,
                    Include = include,
                    CustomerId  = customerId
                }, new Guid());
            var data = await _mediator.Send(getProductData);
            return Mapper.Map<ProductData, ProductSnapshot>(data);
        }
    }
}
