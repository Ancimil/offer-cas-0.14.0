using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations.Schema;

namespace Offer.Domain.AggregatesModel.ApplicationAggregate
{
    public partial class CreditLineRequest : FinanceServiceArrangementRequest
    {
        [NotMapped]
        public CreditLineLimits CreditLineLimits { get; set; }

        [Column("CreditLineLimits")]
        [JsonIgnore]
        public string _CreditLineLimits
        {
            get
            {
                return CreditLineLimits == null ? null : JsonConvert.SerializeObject(CreditLineLimits);
            }
            set
            {
                CreditLineLimits = value == null ? null : JsonConvert.DeserializeObject<CreditLineLimits>(value);
            }
        }


    }


}
