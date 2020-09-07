using System.ComponentModel.DataAnnotations;

namespace Offer.Domain.AggregatesModel.ApplicationAggregate.ApplicantModel
{
    public class Relationship
    {
        [MaxLength(256)]
        public string Kind { get; set; }
        [MaxLength(1024)]
        public string Role { get; set; }
            
        public ToParty ToParty { get; set; }
       
    }

    public class ToParty
    {
        [MaxLength(256)]
        public string Number { get; set; }
        [MaxLength(256)]
        public string Name { get; set; }
        public PartyKind Kind { get; set; }
    }
}
