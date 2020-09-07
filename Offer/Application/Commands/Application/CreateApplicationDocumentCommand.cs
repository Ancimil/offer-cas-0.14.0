using Offer.Domain.AggregatesModel.ApplicationAggregate;
using MediatR;
using System.ComponentModel.DataAnnotations;
using MicroserviceCommon.Models;

namespace Offer.API.Application.Commands
{
    public class CreateApplicationDocumentCommand : IRequest<CommandStatus<ApplicationDocument>>
    {
        public long ApplicationNumber { get; set; }
        [Required]
        public DocumentContextKind DocumentContextKind { get; set; }
        public string ArrangementRequestId { get; set; }
        public string CollateralId { get; set; }
        public string PartyId { get; set; }
        [Required]
        public string DocumentName { get; set; }
        [Required]
        public string DocumentKind { get; set; }
        public string DocumentReviewPeriod { get; set; }
        public bool IsMandatory { get; set; }
        public bool IsForSigning { get; set; }
        public bool IsForUpload { get; set; }
        public bool IsForPhysicalArchiving { get; set; }
        public bool IsInternal { get; set; }
        public bool IsComposedFromTemplate { get; set; }
        public string TemplateUrl { get; set; }
        public DocumentOrigin Origin { get; private set; }

        public CreateApplicationDocumentCommand(long applicationNumber, DocumentContextKind documentContextKind, string arrangementRequestId, string collateralId, 
            string partyId, string documentName, string documentKind, string documentReviewPeriod, bool isMandatory, bool isForSigning, bool isForUpload, 
            bool isForPhysicalArchiving, bool isInternal, bool isComposedFromTemplate, string templateUrl, DocumentOrigin origin)
        {
            ApplicationNumber = applicationNumber;
            DocumentContextKind = documentContextKind;
            ArrangementRequestId = arrangementRequestId;
            CollateralId = collateralId;
            PartyId = partyId;
            DocumentName = documentName;
            DocumentKind = documentKind;
            DocumentReviewPeriod = documentReviewPeriod;
            IsMandatory = isMandatory;
            IsForSigning = isForSigning;
            IsForUpload = isForUpload;
            IsForPhysicalArchiving = isForPhysicalArchiving;
            IsInternal = isInternal;
            IsComposedFromTemplate = isComposedFromTemplate;
            TemplateUrl = templateUrl;
            Origin = origin;
        }
    }
}
