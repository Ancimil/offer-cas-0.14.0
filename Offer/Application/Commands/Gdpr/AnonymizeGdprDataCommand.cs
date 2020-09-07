using MediatR;

namespace Offer.API.Application.Commands
{
    public class AnonymizeGdprDataCommand : IRequest<string>
    {
        public string Username { get; set; }
        public string CustomerNumber { get; set; }
        public bool Fake { get; set; }

        public AnonymizeGdprDataCommand(string username, string customerNumber, bool fake)
        {
            Username = username;
            CustomerNumber = customerNumber;
            Fake = fake;
        }
    }
}
