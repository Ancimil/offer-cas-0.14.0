using MicroserviceCommon.Models.Product;

namespace PriceCalculation.Models.Product
{
    public class ProductOption : Option
    {
        public string GroupCode { get; set; }
        public string GroupDescription { get; set; }
    }
}
