using MicroserviceCommon.Domain.SeedWork;
using Offer.Infrastructure.View;
using System.Collections.Generic;

namespace Offer.Domain.AggregatesModel.ApplicationAggregate
{
    public interface IApplicationDocumentRepository : IRepository<Application>
    {
        DocumentsForAcceptance GetDocumentsForAcceptance(long applicationNumber);
        DocumentsForComposition GetDocumentsForComposition(long applicationNumber);
        List<ApplicationDocument> GetApplicationDocumentsForIds(List<int> documentIds);
        ApplicationDocumentViewList GetApplicationDocuments(long applicationNumber, List<DocumentStatus> statusList = null,
            bool? isForSigning = null, bool? isForUpload = null, bool? isComposedFromTemplate = null, string documentKinds = null,
            string documentNames = null, List<DocumentContextKind> contextList = null, string collateralId = null);

        void UpdateApplicationDocuments(List<ApplicationDocument> applicationDocuments);
        void UpdateDocumentsStatus(long applicationNumber, List<long> documents, DocumentStatus status);
        ApplicationDocument DeleteDocument(long applicationNumber, string documentId);
    }
}
