using System.Collections.Generic;
using MediatR;
using Offer.Domain.AggregatesModel.ApplicationAggregate;

namespace Offer.API.Application.Commands
{
    public class ProductKindsCommand : IRequest<bool?>
    {
        //  public OfferApplication Application { get; set; }
        public long ApplicationId { get; set; }
        public string ApplicationNumber
        {
            get
            {
                var result = "0000000000" + ApplicationId;
                return result.Substring(result.Length - 10);
            }
        }
        public int ArrangementRequestId { get; set; }

        public List<ProductKindLimit> ProductKinds { get; set; }

    }
}
