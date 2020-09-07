using MediatR;

namespace Offer.API.Application.Commands
{

    public class DeleteApplicationDocumentCommand : IRequest<bool?>
    {
        public long ApplicationNumber { get; set; }
        public string DocumentId { get; set; }

        public DeleteApplicationDocumentCommand() { }

        public DeleteApplicationDocumentCommand(long applicationNumber, string documentId)
        {
            ApplicationNumber = applicationNumber;
            DocumentId = documentId;
        }
    }
}
