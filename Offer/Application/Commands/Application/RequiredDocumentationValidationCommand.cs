using MediatR;
using Offer.Domain.AggregatesModel.ApplicationAggregate;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Offer.API.Application.Commands.Application
{
    public class RequiredDocumentationValidationCommand: IRequest<DocumentationValidationResponse>
    {
        public long ApplicationNumber { get; set; }
        [Required]
        public List<DocumentationValidationItemCommand> Items { get; set; }
        public string Milestone { get; set; }
    }
}
