using MediatR;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace Offer.API.Application.Commands
{
    public class UpdateCustomerNumberCommand : IRequest<bool>
    {
        [Required]
        public string ApplicationNumber { get; set; }
        [Required]
        public string CustomerNumber { get; private set; }
        [Required]
        public string Username { get; set; }
        public bool AuditLog { get; set; }

        public UpdateCustomerNumberCommand(string applicationNumber, string customerNumber, string username)
        {
            ApplicationNumber = applicationNumber;
            CustomerNumber = customerNumber;
            Username = username;
        }
    }
}
