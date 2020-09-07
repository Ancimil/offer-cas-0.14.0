using MediatR;
using System.Linq;

namespace Offer.API.Application.Commands
{
    public class GetProductDataCommand : IRequest<ProductData>
    {
        public string ProductCode { get; set; }
        public string Include { get; set; }
        public string[] IncludeArray
        {
            get
            {
                return string.IsNullOrEmpty(Include) ? new string[0] : Include.Split(",");
            }
            set
            {
                if (value == null || value.Count() == 0)
                {
                    Include = null;
                }
                else
                {
                    Include = string.Join(',', value);
                }
            }
        }
        public string CustomerId { get; set; }
    }
}
