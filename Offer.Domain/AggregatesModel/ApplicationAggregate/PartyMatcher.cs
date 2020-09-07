namespace Offer.Domain.AggregatesModel.ApplicationAggregate
{
    public class PartyMatcher
    {
        public string Username { get; private set; }
        public string CustomerNumber { get; private set; }
        public string Email { get; private set; }
        public string MobilePhone { get; private set; }

        public PartyMatcher(string username = null, string customerNumber = null, string email = null, string phoneNumber = null)
        {
            Username = username;
            CustomerNumber = customerNumber;
            Email = email;
            MobilePhone = phoneNumber;
        }

        public decimal Matches(Party candidate)
        {
            if (candidate == null)
            {
                return 0;
            }

            if (!string.IsNullOrEmpty(Username) && candidate.Username != null && candidate.Username.Equals(Username))
            {
                return 100;
            }
            else if (!string.IsNullOrEmpty(CustomerNumber) && candidate.CustomerNumber != null && candidate.CustomerNumber.Equals(CustomerNumber))
            {
                return 100;
            }
            else if (!string.IsNullOrEmpty(Email) && !string.IsNullOrEmpty(MobilePhone) &&
                ((IndividualParty)candidate).EmailAddress != null && ((IndividualParty)candidate).EmailAddress.Equals(Email) &&
                ((IndividualParty)candidate).MobilePhone != null && ((IndividualParty)candidate).MobilePhone.Equals(MobilePhone))
            {
                return 100;
            }
            else if (!string.IsNullOrEmpty(Email) && ((IndividualParty)candidate).EmailAddress != null && ((IndividualParty)candidate).EmailAddress.Equals(Email))
            {
                return 80;
            }
            else if (!string.IsNullOrEmpty(MobilePhone) && ((IndividualParty)candidate).MobilePhone != null && ((IndividualParty)candidate).MobilePhone.Equals(MobilePhone))
            {
                return 80;
            }
            else
            {
                return 0;
            }
        }
    }
}
