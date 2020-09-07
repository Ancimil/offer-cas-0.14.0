using System.Collections.Generic;

namespace Offer.Domain.AggregatesModel.ApplicationAggregate
{

    public class DocumentsForComposition
    {
        public List<ApplicationCompositionDocument> documents;
    }

    public class ApplicationCompositionDocument
    {
        public int DocumentId { get; set; }
        public string DocumentName { get; set; }
        public string TemplateUrl { get; set; }
        public DocumentStatus Status { get; set; }
    }
}
