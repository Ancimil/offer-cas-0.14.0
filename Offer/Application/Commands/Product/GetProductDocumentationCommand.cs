using MediatR;
using System.Collections.Generic;

namespace Offer.API.Application.Commands
{
    public class GetProductDocumentationCommand : IRequest<List<ProductDocumentation>>
    {
        public string ProductCode { get; set; }
        public string CustomerId { get; set; }

    }
}
