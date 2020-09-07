using Offer.Domain.AggregatesModel.ApplicationAggregate;
using System.Collections.Generic;

namespace Offer.Domain.Utils
{
    public class ArrangementRequestFactoryResponse
    {
        public List<ArrangementRequest> AddedArrangementRequests { get; set; } = new List<ArrangementRequest>();
        public List<ArrangementRequest> NotAddedArrangementRequests { get; set; } = new List<ArrangementRequest>();

        public void AddFactoryResponse(ArrangementRequestFactoryResponse factoryResponse)
        {
            if (factoryResponse != null)
            {
                this.AddedArrangementRequests.AddRange(factoryResponse.AddedArrangementRequests);
                this.NotAddedArrangementRequests.AddRange(factoryResponse.NotAddedArrangementRequests);
            }
        }
    }
}
