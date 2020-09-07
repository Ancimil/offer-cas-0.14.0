using System.Collections.Generic;
using MediatR;
using Offer.Domain.AggregatesModel.ApplicationAggregate;

namespace Offer.API.Application.Commands
{
    public class GetAvailableProductsCommand : IRequest<AvailableProductsResponse>
    {
        public long ApplicationId { get; set; }
        public List<ProductSnapshot> Products { get; set; }
        public List<BundleComponentInfo> NotAddedProducts { get; set; }
        public string ChannelCode { get; set; }
        public string CustomerId { get; set; }

    }
}
