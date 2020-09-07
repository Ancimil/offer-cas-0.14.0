using System.Runtime.Serialization;

namespace Offer.Domain.AggregatesModel.ApplicationAggregate
{
    public enum RegistrationProfile
    {
        [EnumMember(Value = "contact")]
        Contact,

        [EnumMember(Value = "prospect")]
        Prospect,

        [EnumMember(Value = "customer")]
        Customer
    }
}
