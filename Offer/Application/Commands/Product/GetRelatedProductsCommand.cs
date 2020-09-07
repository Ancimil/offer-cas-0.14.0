using MediatR;

namespace Offer.API.Application.Commands
{
    public class GetRelatedProductsCommand : IRequest<ProductList>
    {
        public string ProductCode { get; set; }
        public string ChannelCode { get; set; }
        public string CustomerId { get; set; }

    }
}
