using MediatR;
using Offer.Domain.AggregatesModel.ApplicationAggregate;

namespace Offer.API.Application.Commands
{
    public class InitiateGdprProcessCommand : IRequest<string>
    {
        public GdprProcessEnum ProcessKey { get; set; }
        public string Username { get; set; }
        public string CustomerNumber { get; set; }
        public string Email { get; set; }
        public string UserType { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string PhoneNumber { get; set; }
        public string Initiator { get; set; }
        public string UserMessage { get; set; }

        public InitiateGdprProcessCommand(GdprProcessEnum processKey, string username, string customerNumber, string email,
            string userType, string lastName, string firstName, string phoneNumber, string initiator, string userMessage)
        {
            ProcessKey = processKey;
            Username = username;
            CustomerNumber = customerNumber;
            Email = email;
            UserType = userType;
            LastName = lastName;
            FirstName = firstName;
            PhoneNumber = phoneNumber;
            Initiator = initiator;
            UserMessage = userMessage;
        }
    }
}
