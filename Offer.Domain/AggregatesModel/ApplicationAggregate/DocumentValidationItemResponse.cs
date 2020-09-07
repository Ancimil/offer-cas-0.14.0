using System.Collections.Generic;
using ProductPartyRoleEnum = Offer.Domain.AggregatesModel.ApplicationAggregate.ProductModel.PartyRoleEnum;

namespace Offer.Domain.AggregatesModel.ApplicationAggregate
{
    public class DocumentValidationItemResponse
    {
        public DocumentContextKind DocumentContextKind { get; set; }
        public string ProductCode { get; set; }
        public ProductPartyRoleEnum? PartyRole { get; set; }
        public string CollateralKind { get; set; }
        public string DocumentName { get; set; }
        public long ApplicationId { get; set; }
        public string ApplicationNumber { get; set; }
        public int ArrangementRequestId { get; set; }
        public string CollateralId { get; set; }
        public string DocumentKind { get; set; }
        public long PartyId { get; set; }
        public DocumentStatus Status { get; set; }
        public DocumentStatus RequiredStatus { get; set; }
        public DocumentRequirementValidationStatus ValidationStatus { get; set; }
    }

    public class DocumentationValidationResponse
    {
        public List<DocumentValidationItemResponse> Items { get; set; }
    }
}
