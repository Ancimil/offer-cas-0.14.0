using MediatR;
using MicroserviceCommon.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Offer.API.Application.Commands.Product
{
    public class DeleteAvailableProductsCommand : IRequest<CommandStatus>
    {
        public long ApplicationNumber { get; set; }
        [Required]
        public List<string> Products { get; set; }
    }
}
