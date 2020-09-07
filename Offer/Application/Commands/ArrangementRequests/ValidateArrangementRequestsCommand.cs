using MediatR;
using Offer.Domain.AggregatesModel.ApplicationAggregate.ProductModel;
using System.Collections.Generic;

namespace Offer.API.Application.Commands.ArrangementRequests
{
    public class ValidateArrangementRequestResponse
    {
        public List<ArrangementRequestValidationData> Items { get; set; }
    }

    public class ArrangementRequestValidationData : BasicProductInfo
    {
        public int MinimalNumberOfInstances { get; set; }
        public int MaximalNumberOfInstances { get; set; }
        public int Count { get; set; }
        public int ParentCount { get; set; }
        public bool IsValid { get; set; }
    }
    public class ValidateArrangementRequestsCommand : IRequest<ValidateArrangementRequestResponse>
    {
        public long ApplicationId { get; set; }
    }
}
