using Offer.Domain.AggregatesModel.ApplicationAggregate;
using System.Collections.Generic;

namespace Offer.Infrastructure.View
{
    public class ApplicationDocumentView
    {
        public int DocumentId { get; set; }
        public DocumentContextKind DocumentContextKind { get; set; }
        public string DocumentContextInfo { get; set; }
        public int? ArrangementRequestId { get; set; }
        public long ApplicationId { get; set; } 
        public string CollateralId { get; set; }
        public long? PartyId { get; set; }
        public string DocumentName { get; set; }
        public string DocumentKind { get; set; }
        public string Context { get; set; }
        public string DocumentReviewPeriod { get; set; }
        public bool IsMandatory { get; set; }
        public bool IsComposedFromTemplate { get; set; }
        public string TemplateUrl { get; set; }
        public bool IsForSigning { get; set; }
        public bool IsForUpload { get; set; }
        public bool IsForPhysicalArchiving { get; set; }
        public bool IsInternal { get; set; }
        public DocumentOrigin Origin { get; set; }
        public DocumentStatus Status { get; set; }
        public bool IsForProposal { get; set; }
    }

    public class ApplicationDocumentViewList
    {
        public List<ApplicationDocumentView> Documents { get; set; }
    }
}
