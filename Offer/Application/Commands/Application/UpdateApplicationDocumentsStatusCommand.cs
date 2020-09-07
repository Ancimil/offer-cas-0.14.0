using MediatR;
using Offer.Domain.AggregatesModel.ApplicationAggregate;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Offer.API.Application.Commands
{
    public class UpdateApplicationDocumentsStatusCommand : IRequest<bool?>
    {
        public long ApplicationId { get; set; }

        public string ApplicationNumber
        {
            get
            {
                var result = "0000000000" + ApplicationId;
                return result.Substring(result.Length - 10);
            }
        }

        [Required]
        public List<int> DocumentIds { get; set; }

        [Required]
        public DocumentStatus Status { get; set; }

        public UpdateApplicationDocumentsStatusCommand()
        {
        }

        public UpdateApplicationDocumentsStatusCommand(List<int> documentIds, DocumentStatus status)
        {
            DocumentIds = documentIds;
            Status = status;
        }
    }

    public class ContentDocumentMessage
    {
        public string Path { get; set; }
        public DocumentStatus FilingPurpose { get; set; } = DocumentStatus.UploadedEnum;
    }
}