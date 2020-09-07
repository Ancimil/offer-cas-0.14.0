using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations.Schema;

namespace PriceCalculation.Models.LeadModel
{
    public class BenefitModel
    {
        public int BenefitId { get; set; }
        public long LeadId { get; set; }
        [JsonIgnore]
        [NotMapped]
        public LeadModel Lead { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public decimal Value { get; set; }
        public string Kind { get; set; }
    }
}
