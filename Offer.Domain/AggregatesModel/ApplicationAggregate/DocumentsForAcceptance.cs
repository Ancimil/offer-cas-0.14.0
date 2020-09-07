using System.Collections.Generic;

namespace Offer.Domain.AggregatesModel.ApplicationAggregate
{

    public class DocumentsForAcceptance
    {
        public List<ApplicationDocument> Documents;
    }

    public class ApplicationAcceptanceDocument
    {
        public int DocumentId { get; set; }
        public string DocumentName { get; set; }
        public string DocumentType { get; set; }
        public string ContentUrl { get; set; }
        public bool IsMandatory { get; set; }
        public bool IsForUpload { get; set; }
        public DocumentStatus Status { get; set; }
    }
}
