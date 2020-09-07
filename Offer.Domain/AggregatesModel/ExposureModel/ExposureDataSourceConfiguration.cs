using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace Offer.Domain.AggregatesModel.ExposureModel
{
    public class ExposureDataSourceConfiguration : List<ExposureDataSource>
    {
        
    }
    public class ExposureDataSource
    {
        public string UrlRoot { get; set; }
        public ExposureDataSourceType SourceType { get; set; }
    }

    public enum ExposureDataSourceType
    {
        [EnumMember(Value = "offer-v1")]
        [Description("Offer v1")]
        OfferV1,
        [EnumMember(Value = "offer-v2")]
        [Description("Offer v2")]
        OfferV2
    }
}
