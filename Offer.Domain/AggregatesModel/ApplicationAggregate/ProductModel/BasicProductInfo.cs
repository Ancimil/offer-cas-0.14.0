namespace Offer.Domain.AggregatesModel.ApplicationAggregate.ProductModel
{
    public class BasicProductInfo
    {
        public string ProductName { get; set; }
        public string ProductCode { get; set; }
        public string ImageUrl { get; set; }
        public string BenefitsInfo { get; set; }
        public string Description { get; set; }
        public ProductKinds Kind { get; set; }
        public bool IsRelated { get; set; }
        public int? ArrangementRequestId { get; set; }
    }
}
