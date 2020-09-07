using JsonSubTypes;
using Newtonsoft.Json;

namespace Offer.Domain.AggregatesModel.ApplicationAggregate.ApplicantModel
{
    [JsonConverter(typeof(JsonSubtypes))]
    [JsonSubtypes.KnownSubTypeWithProperty(typeof(IndividualContactPoints), "mobile-phone")]
    public class ContactPoints
    {
        public PostalAddress LegalAddress { get; set; }
        public PostalAddress ContactAddress { get; set; }
        public string EmailAddress { get; set; }
    }

    public class IndividualContactPoints: ContactPoints
    {
        public string MobilePhone { get; set; }
        public string HomePhoneNumber { get; set; }
    }

}