namespace Offer.Domain.AggregatesModel.ApplicationAggregate.ProductModel
{
    public class BundledProductInfo
    {
        public string ProductCode { get; set; }
        public ProductKinds ProductKind { get; set; }
        public string ProductName { get; set; }
        public ProductStatus Status { get; set; }
        public string ImageUrl { get; set; }
        public string CampaignCode { get; set; }
        public bool IsOptional { get; set; } = false;
        public int MinimalNumberOfInstances { get; set; }
        public int MaximalNumberOfInstances { get; set; }
    }
}
