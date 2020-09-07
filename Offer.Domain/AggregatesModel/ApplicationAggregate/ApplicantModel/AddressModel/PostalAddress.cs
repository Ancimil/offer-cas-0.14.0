using System.ComponentModel.DataAnnotations;

namespace Offer.Domain.AggregatesModel.ApplicationAggregate
{
    public class PostalAddress
    {
        public AddressKind? Kind { get; set; }
        [MaxLength(1024)]
        public string Formatted { get; set; }
        [MaxLength(256)]
        public string Street { get; set; }
        [MaxLength(50)]
        public string StreetNumber { get; set; }
        [MaxLength(10)]
        public string PostalCode { get; set; }
        [MaxLength(256)]
        public string Locality { get; set; }
        [MaxLength(256)]
        public string AddressCode { get; set; }
        [MaxLength(256)]
        public string Country { get; set; }
        [MaxLength(256)]
        public string Apartment { get; set; }
        [MaxLength(256)]
        public string Floor { get; set; }

        public Coordinates Coordinates { get; set; }
    }

    public class Coordinates
    {
        public double? Lat { get; set; }
        public double? Long { get; set; }
    }
}