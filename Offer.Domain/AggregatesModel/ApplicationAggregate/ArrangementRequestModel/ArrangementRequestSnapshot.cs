using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Offer.Domain.AggregatesModel.ApplicationAggregate.ArrangementRequestModel
{
    public class ArrangementRequestSnapshotOld
    {
        [JsonIgnore]
        public long ArrangementRequestSnapshotId { get; set; }
        [JsonIgnore]
        public Application Application { get; set; }

        [JsonIgnore]
        [Required]
        public long ApplicationId { get; set; }
        public string ProductCode { get; set; }
        [JsonIgnore]
        public string _ArrangementRequest { get; set; }
        [NotMapped]
        public ArrangementRequest ArrangementRequest
        {
            get
            {
                JsonSerializerSettings settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };
                return string.IsNullOrEmpty(_ArrangementRequest) ? null :
                    JsonConvert.DeserializeObject<ArrangementRequest>(_ArrangementRequest, settings);
            }
            set
            {
                JsonSerializerSettings settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };
                _ArrangementRequest = JsonConvert.SerializeObject(value, settings);
            }
        }
    }
}
