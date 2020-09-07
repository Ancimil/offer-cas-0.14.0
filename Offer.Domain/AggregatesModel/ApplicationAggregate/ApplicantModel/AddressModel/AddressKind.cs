using System.Runtime.Serialization;

namespace Offer.Domain.AggregatesModel.ApplicationAggregate
{
    public enum AddressKind
    {
        [EnumMember(Value = "postal-address")]
        PostalAddress,

        [EnumMember(Value = "phone-number")]
        PhoneNumber,

        [EnumMember(Value = "email-address")]
        EmailAddress,

        [EnumMember(Value = "facebook-account")]
        FacebookAccount,

        [EnumMember(Value = "web-url")]
        WebUrl
     
    }
}
