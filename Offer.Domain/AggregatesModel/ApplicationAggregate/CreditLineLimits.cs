using MediatR;
using System.Collections.Generic;

namespace Offer.Domain.AggregatesModel.ApplicationAggregate
{
    public class CreditLineLimits : IRequest<bool>
    {
        public List<RevolvingLineUser> AllowedRevolvingLineUsers { get; set; }
        public List<ProductCodeLimit> ProductCodes { get; set; }
        public List<ProductKindLimit> ProductKinds { get; set; }
    }

    public class RevolvingLineUser
    {
        public string CustomerNumber { get; set; }
        public string CustomerName { get; set; }
        public decimal Limit { get; set; }
    }

    public class ProductCodeLimit : IRequest<bool>
    {
        public string ProductCode { get; set; }
        public decimal Limit { get; set; }
    }
    public class ProductKindLimit
    {
        public string ProductKind { get; set; }
        public decimal Limit { get; set; }
    }

}
