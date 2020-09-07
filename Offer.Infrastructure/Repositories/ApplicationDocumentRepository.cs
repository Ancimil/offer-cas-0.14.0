using Offer.Domain.AggregatesModel.ApplicationAggregate;
using System;
using System.Collections.Generic;
using MicroserviceCommon.Domain.SeedWork;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Offer.Domain.Exceptions;
using Offer.Infrastructure.View;
using Offer.Infrastructure.Mappings;

namespace Offer.Infrastructure.Repositories
{
    public class ApplicationDocumentRepository : IApplicationDocumentRepository
    {
        private readonly OfferDBContext _context;
        private readonly IApplicationRepository _applicationRepository;

        public IUnitOfWork UnitOfWork
        {
            get
            {
                return _context;
            }
        }

        public ApplicationDocumentRepository(
            OfferDBContext context,
            IApplicationRepository applicationRepository
        )
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _applicationRepository = applicationRepository ?? throw new ArgumentNullException(nameof(applicationRepository));
        }

        public DocumentsForAcceptance GetDocumentsForAcceptance(long applicationNumber)
        {
            var application = _applicationRepository.GetAsync(applicationNumber).Result;
            if (application == null)
            {
                return null;
            }
            var documents = _context.ApplicationDocuments
                .Where(d => d.ApplicationId == applicationNumber)
                .Where(d => d.IsForSigning)
                .ToList();

            DocumentsForAcceptance documentsForAcceptance = new DocumentsForAcceptance
            {
                Documents = documents
            };

            return documentsForAcceptance;
        }


        public DocumentsForComposition GetDocumentsForComposition(long applicationNumber)
        {
            var application = _applicationRepository.GetAsync(applicationNumber).Result;
            if (application == null)
            {
                return null;
            }
            var documents = _context.ApplicationDocuments
                .Where(d => d.ApplicationId == applicationNumber)
                .Where(d => d.IsComposedFromTemplate)
                .Where(d => d.Status.Equals(DocumentStatus.EmptyEnum))
                .ToList();

            List<ApplicationCompositionDocument> compositionDocuments = new List<ApplicationCompositionDocument>();
            foreach (ApplicationDocument aDoc in documents)
            {
                var document = new ApplicationCompositionDocument
                {
                    DocumentId = aDoc.DocumentId,
                    DocumentName = aDoc.DocumentName,
                    Status = aDoc.Status,
                    TemplateUrl = aDoc.TemplateUrl
                };
                compositionDocuments.Add(document);
            }

            DocumentsForComposition documentsForComposition = new DocumentsForComposition
            {
                documents = compositionDocuments
            };

            return documentsForComposition;
        }

        public List<ApplicationDocument> GetApplicationDocumentsForIds(List<int> documentIds)
        {
            var documents = _context.ApplicationDocuments.Where(d => documentIds.Contains(d.DocumentId)).ToList();
            return documents;
        }

        public void UpdateApplicationDocuments(List<ApplicationDocument> applicationDocuments)
        {
            _context.UpdateRange(applicationDocuments);
        }

        public void UpdateDocumentsStatus(long applicationNumber, List<long> documents, DocumentStatus status)
        {
            var docs = _context.ApplicationDocuments.Where(d => d.ApplicationId == applicationNumber && documents.Contains(d.DocumentId)).ToList();
            docs.ForEach(d => d.Status = status);
            _context.UpdateRange(docs);
        }

        public ApplicationDocument DeleteDocument(long applicationNumber, string documentId)
        {
            var application = _applicationRepository.GetAsync(applicationNumber).Result;
            if (application == null)
            {
                return null;
            }
            var doc = _context.ApplicationDocuments.Where(d => d.ApplicationId == applicationNumber && d.DocumentId == int.Parse(documentId)).FirstOrDefault();
            if (doc == null)
            {
                throw new ApplicationDocumentNotFoundException("Document with ID = " + documentId + " for application " + applicationNumber + " not found");
            }
            _context.ApplicationDocuments.Remove(doc);
            return doc;
        }

       

        public ApplicationDocumentViewList GetApplicationDocuments(long applicationNumber, List<DocumentStatus> statusList = null,
            bool? isForSigning = null, bool? isForUpload = null, bool? isComposedFromTemplate = null, string documentKinds = null,
            string documentNames = null, List<DocumentContextKind> contextList = null, string collateralId = null)
        {
            var application = _applicationRepository.GetAsync(applicationNumber).Result;
            if (application == null)
            {
                return null;
            }
            var query = _context.ApplicationDocuments.AsQueryable();
            query = query.Include(x => x.ArrangementRequest).Include(x => x.Party);
            query = query.Where(d => d.ApplicationId == applicationNumber)
                .Where(d => d.DocumentContextKind != DocumentContextKind.ArrangementRequestEnum ||
                    (d.ArrangementRequest != null && (d.ArrangementRequest.Enabled == true)));
            if (statusList != null && statusList.Count > 0)
                query = query.Where(d => statusList.Contains(d.Status));
            if (isForSigning.HasValue)
                query = query.Where(d => d.IsForSigning == isForSigning.Value);
            if (isForSigning.HasValue)
                query = query.Where(d => d.IsForSigning == isForSigning.Value);
            if (isForUpload.HasValue)
                query = query.Where(d => d.IsForUpload == isForUpload.Value);
            if (isComposedFromTemplate.HasValue)
                query = query.Where(d => d.IsComposedFromTemplate == isComposedFromTemplate.Value);
            if (collateralId != null)
                query = query.Where(d => d.CollateralId.Equals(collateralId));
            if (!string.IsNullOrEmpty(documentKinds))
            {
                var documentKindList = documentKinds.Split(",");
                query = query.Where(d => documentKindList.Contains(d.DocumentKind));
            }
            if (contextList != null && contextList.Count > 0)
                query = query.Where(d => contextList.Contains(d.DocumentContextKind));
            if (documentNames != null)
            {
                var documentNameList = documentNames.Split(",").ToList();
                query = query.Where(d => documentNameList.Contains(d.DocumentName));
            }

            var newQuery = query.Select(x => x.GetDocumentView());

            var list = new ApplicationDocumentViewList
            {
                Documents = newQuery.ToList()
            };
            return list;
        }
    }
}
