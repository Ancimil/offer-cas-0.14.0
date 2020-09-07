using MicroserviceCommon.Models;
using MicroserviceCommon.Services;
using Microsoft.Extensions.Logging;
using Offer.Domain.AggregatesModel.ApplicationAggregate;
using System;
using System.Collections.Generic;
using System.Linq;
using ProductPartyRoleEnum = Offer.Domain.AggregatesModel.ApplicationAggregate.ProductModel.PartyRoleEnum;

namespace Offer.Domain.Utils
{
    public class RequiredDocumentationResolver
    {
        private Application Application { get; set; }
        private List<DocumentationValidationItemCommand> DocumentationRequirements { get; set; } = new List<DocumentationValidationItemCommand>();
        private List<DocumentValidationItemResponse> NewRequirements { get; set; } = new List<DocumentValidationItemResponse>();
        private readonly IConfigurationService _configurationService;
        private readonly ILogger<RequiredDocumentationResolver> _logger;
        private readonly List<int> Deleted = new List<int>();
        private readonly List<long> Existing = new List<long>();

        public RequiredDocumentationResolver(IConfigurationService configurationService, ILogger<RequiredDocumentationResolver> logger)
        {
            _configurationService = configurationService ?? throw new ArgumentNullException(nameof(configurationService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public DocumentationValidationResponse ResolveDocuments(Application application, List<DocumentationValidationItemCommand> documentationRequirements)
        {
            _logger.LogInformation("Resolving documents for application {ApplicationNumber}", application.ApplicationNumber);
            if (application.Documents == null || application.ArrangementRequests == null || application.InvolvedParties == null)
            {
                throw new Exception("Not all application data are passed with application. Some of documents, arrangement requests or involved parties are null.");
            }
            this.Application = application;
            if (documentationRequirements == null || documentationRequirements.Count() == 0)
            {
                throw new Exception("Documentation requirements are not provided or they are empty.");
            }

            int counter = 0;
            while (counter < documentationRequirements.Count)
            {
                var currentItem = documentationRequirements.ElementAt(counter);
                //var sameDocs = from it in documentationRequirements
                //               where it.Equals(currentItem) && documentationRequirements.IndexOf(it) != counter
                //               select it;

                for (int i = 0; i < documentationRequirements.Count; i++)
                {
                    if (counter == i) continue;

                    if (documentationRequirements.ElementAt(i).Equals(documentationRequirements.ElementAt(counter)))
                    {
                        AddProductCodeToCollection(currentItem, documentationRequirements.ElementAt(i));
                        documentationRequirements.RemoveAt(i--);
                    }
                }
                this.DocumentationRequirements.Add(currentItem);
                counter++;
            }


            //this.DocumentationRequirements = documentationRequirements;

            try
            {
                _logger.LogInformation("Resolving documents for application context on application {ApplicationNumber}", application.ApplicationNumber);
                ResolveForApplication();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An error occurred while trying to resolve application documents for application context on application {ApplicationNumber}",
                    application.ApplicationNumber);
            }

            try
            {
                _logger.LogInformation("Resolving documents for party context on application {ApplicationNumber}", application.ApplicationNumber);
                ResolveForParties();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An error occurred while trying to resolve application documents for party context on application {ApplicationNumber}",
                    application.ApplicationNumber);
            }

            try
            {
                _logger.LogInformation("Resolving documents for arrangement-request context on application {ApplicationNumber}", application.ApplicationNumber);
                ResolveForArrangementRequests();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An error occurred while trying to resolve application documents for arrangement request context on application {ApplicationNumber}",
                    application.ApplicationNumber);
            }

            try
            {
                _logger.LogInformation("Resolving documents for collateral context on application {ApplicationNumber}", application.ApplicationNumber);
                ResolveForCollaterals();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An error occurred while trying to resolve application documents for collateral context on application {ApplicationNumber}",
                    application.ApplicationNumber);
            }

            //var res = new ResolvedDocumentationData
            //{
            //    Application = Application,
            //    DeletedCount = Deleted,
            //    ExistingCount = Existing,
            //    DocumentationRequirements = DocumentationRequirements
            //};

            ValidateRequirements(Application);
            var res = new DocumentationValidationResponse
            {
                Items = NewRequirements
            };
           
            return res;
        }

        private void AddProductCodeToCollection(DocumentationValidationItemCommand toAdd, DocumentationValidationItemCommand toDelete)
        {
            bool definitionForSomeProducts = toDelete.ProductCodes != null && toDelete.ProductCodes.Any();
            if (definitionForSomeProducts)
            {
                if (toAdd.ProductCodes != null && !toAdd.ProductCodes.Any())
                {
                    foreach (var item in toDelete.ProductCodes)
                    {
                        toAdd.ProductCodes.Add(item);
                    }
                }
            }
            else
            {
                toAdd.ProductCodes = null;
            }
        }

        private void ResolveForApplication()
        {
            // var documents = Application.Documents;
            // var existingHandledDocs = new List<long>();

            var applicationDocuments = DocumentationRequirements.Where(d =>
                    d.DocumentContextKind.Equals(DocumentContextKind.ApplicationEnum)).ToList();
            foreach (var document in applicationDocuments)
            {
                //var appDocument = ApplicationDocument.FromProductDocument(document);
                var appDocument = Convert(document);
                appDocument.ApplicationId = Application.ApplicationId;
                appDocument.ApplicationNumber = Application.ApplicationNumber;

                //var matchings = from it in DocumentationRequirements
                //                where it.DocumentType == appDocument.DocumentKind &&
                //                      it.DocumentContextKind == appDocument.DocumentContextKind
                //                select it;
                //if (matchings != null && matchings.Any())
                //{
                //    var existingDoc = matchings.FirstOrDefault(d => d.Equals(appDocument));
                //    //existingHandledDocs.Add(existingDoc.DocumentId);
                //}
                NewRequirements.Add(appDocument);
            }

            //var deleted = documents.Where(d => d.DocumentId != 0 && !existingHandledDocs.Contains(d.DocumentId) &&
            //    d.DocumentContextKind.Equals(DocumentContextKind.ApplicationEnum) && d.Origin.Equals(DocumentOrigin.Product))
            //    .Select(d => d.DocumentId).ToList();
            //_logger.LogInformation("There are {DeletedNumber} unused documents being deleted with application document context kind", deleted.Count());
            //Existing.AddRange(existingHandledDocs);
            //Deleted.AddRange(deleted);
        }

        private void ResolveForArrangementRequests()
        {
            // var documents = Application.Documents;
            var existingHandledDocs = new List<long>();
            var arrRequestDocuments = DocumentationRequirements.Where(d =>
                d.DocumentContextKind.Equals(DocumentContextKind.ArrangementRequestEnum)).ToList();
            foreach (var document in arrRequestDocuments)
            {
                var activeArrangements = Application.ArrangementRequests.Where(a => a.Enabled ?? false);
                foreach (var request in activeArrangements)
                {
                    //var appDocument = ApplicationDocument.FromProductDocument(document);
                    var appDocument = Convert(document);
                    appDocument.ApplicationId = Application.ApplicationId;
                    appDocument.ArrangementRequestId = request.ArrangementRequestId;
                    appDocument.ProductCode = request.ProductCode;
                    appDocument.ApplicationNumber = Application.ApplicationNumber;

                    IEnumerable<PartyRole> partyRoles = null;
                    if (document.PartyRole != null)
                    {
                        partyRoles = from party in Application.InvolvedParties
                                     where (party.PartyRole.Equals(PartyRole.Customer) && document.PartyRole.Value.Equals(ProductPartyRoleEnum.CustomerEnum))
                                     || (party.PartyRole.Equals(PartyRole.Customer)
                                        && string.IsNullOrEmpty(party.CustomerNumber) && document.PartyRole.Equals(ProductPartyRoleEnum.NewCustomerEnum))
                                     || (party.PartyRole.Equals(PartyRole.AuthorizedPerson) && document.PartyRole.Equals(ProductPartyRoleEnum.AuthorizedPersonEnum))
                                     || (party.PartyRole.Equals(PartyRole.AuthorizedPerson)
                                        && string.IsNullOrEmpty(party.CustomerNumber) && document.PartyRole.Equals(ProductPartyRoleEnum.NewAuthorizedPersonEnum))
                                     || (party.PartyRole.Equals(PartyRole.CoDebtor) && document.PartyRole.Equals(ProductPartyRoleEnum.CoDebtorEnum))
                                     || (party.PartyRole.Equals(PartyRole.CoDebtor)
                                        && string.IsNullOrEmpty(party.CustomerNumber) && document.PartyRole.Equals(ProductPartyRoleEnum.NewCoDebtorEnum))
                                     || (party.PartyRole.Equals(PartyRole.Guarantor) && document.PartyRole.Equals(ProductPartyRoleEnum.GuarantorEnum))
                                     || (party.PartyRole.Equals(PartyRole.Guarantor)
                                        && string.IsNullOrEmpty(party.CustomerNumber) && document.PartyRole.Equals(ProductPartyRoleEnum.NewGuarantorEnum))
                                     select party.PartyRole;
                    }

                    if (document.ProductCodes == null ||
                        !document.ProductCodes.Any() ||
                        document.ProductCodes.Contains(request.ProductCode)) {
                        if (partyRoles != null && partyRoles.Any())
                        {
                            NewRequirements.Add(appDocument);
                        }
                        
                    }

                    //var matchings = from it in DocumentationRequirements
                    //                where it.DocumentType == appDocument.DocumentKind &&
                    //                      it.DocumentContextKind == appDocument.DocumentContextKind
                    //                select it;
                    //if (matchings != null && matchings.Any())
                    //{
                    //    var existingDoc = matchings.FirstOrDefault(d => d.Equals(appDocument));
                    //    // existingHandledDocs.Add(existingDoc.DocumentId);
                    //}
                    
                }
            }
            
            //var deleted = documents.Where(d => d.DocumentId != 0 && !existingHandledDocs.Contains(d.DocumentId) &&
            //    d.DocumentContextKind.Equals(DocumentContextKind.ArrangementRequestEnum) && d.Origin.Equals(DocumentOrigin.Product))
            //    .Select(d => d.DocumentId).ToList();
            //_logger.LogInformation("There are {DeletedNumber} unused documents being deleted with arrangement request document context kind", deleted.Count());
            //Existing.AddRange(existingHandledDocs);
            //Deleted.AddRange(deleted);
        }

        private void ResolveForParties()
        {
            var existingHandledDocs = new List<long>();
            var partyDocuments = DocumentationRequirements.Where(d =>
                d.DocumentContextKind.Equals(DocumentContextKind.PartyEnum) && Application.InvolvedParties.Count() > 0).ToList();
            foreach (var document in partyDocuments)
            {
                foreach (var party in Application.InvolvedParties)
                {
                    var appDocument = Convert(document);
                    appDocument.ApplicationId = Application.ApplicationId;
                    appDocument.ApplicationNumber = this.Application.ApplicationNumber;
                    appDocument.PartyId = party.PartyId;
                    // Check if party role corresponds to document PartyRole including "new" status for role
                    if (
                            (party.PartyRole.Equals(PartyRole.Customer) && document.PartyRole.Equals(ProductPartyRoleEnum.CustomerEnum))
                            || (party.PartyRole.Equals(PartyRole.Customer)
                                && string.IsNullOrEmpty(party.CustomerNumber) && document.PartyRole.Equals(ProductPartyRoleEnum.NewCustomerEnum))
                            || (party.PartyRole.Equals(PartyRole.AuthorizedPerson) && document.PartyRole.Equals(ProductPartyRoleEnum.AuthorizedPersonEnum))
                            || (party.PartyRole.Equals(PartyRole.AuthorizedPerson)
                                && string.IsNullOrEmpty(party.CustomerNumber) && document.PartyRole.Equals(ProductPartyRoleEnum.NewAuthorizedPersonEnum))
                            || (party.PartyRole.Equals(PartyRole.CoDebtor) && document.PartyRole.Equals(ProductPartyRoleEnum.CoDebtorEnum))
                            || (party.PartyRole.Equals(PartyRole.CoDebtor)
                                && string.IsNullOrEmpty(party.CustomerNumber) && document.PartyRole.Equals(ProductPartyRoleEnum.NewCoDebtorEnum))
                            || (party.PartyRole.Equals(PartyRole.Guarantor) && document.PartyRole.Equals(ProductPartyRoleEnum.GuarantorEnum))
                            || (party.PartyRole.Equals(PartyRole.Guarantor)
                                && string.IsNullOrEmpty(party.CustomerNumber) && document.PartyRole.Equals(ProductPartyRoleEnum.NewGuarantorEnum)))
                    {
                        //var matchings = from it in DocumentationRequirements
                        //                where it.DocumentType == appDocument.DocumentKind &&
                        //                      it.DocumentContextKind == appDocument.DocumentContextKind &&
                        //                select it;
                        //if (matchings != null && matchings.Any())
                        //{
                        //    var existingDoc = matchings.FirstOrDefault();// d => d.Equals(appDocument));
                        //    // existingHandledDocs.Add(existingDoc.DocumentId); // visak
                        //}
                        NewRequirements.Add(appDocument);
                    }
                }
            }
            //var deleted = documents.Where(d => d.DocumentId != 0 && !existingHandledDocs.Contains(d.DocumentId) &&
            //    d.DocumentContextKind.Equals(DocumentContextKind.PartyEnum) && d.Origin.Equals(DocumentOrigin.Product))
            //    .Select(d => d.DocumentId).ToList();
            //_logger.LogInformation("There are {DeletedNumber} unused documents being deleted with party document context kind", deleted.Count());
            //Existing.AddRange(existingHandledDocs);
            //Deleted.AddRange(deleted);
        }

        private void ResolveForCollaterals()
        {
            var documents = Application.Documents;
            var existingHandledDocs = new List<long>();
            var collateralCodes = _configurationService.GetEffective<ClassificationSchema>("collateral/classification-schemes/collateral-code").Result;
            var collateralDocuments = DocumentationRequirements.Where(d =>
                d.DocumentContextKind.Equals(DocumentContextKind.CollateralEnum)).ToList();
            _logger.LogInformation("Found {NumberOfDocuments} documents related to collaterals", collateralDocuments.Count());
            foreach (var document in collateralDocuments)
            {
                var activeArrangements = Application.ArrangementRequests.Where(a => a.Enabled ?? false);
                foreach (var request in activeArrangements)
                {
                    if (request is FinanceServiceArrangementRequest financeRequest && financeRequest.CollateralRequirements != null)
                    {
                        foreach (var collateralRequirement in financeRequest.CollateralRequirements)
                        {
                            var hasCollateralKind = collateralCodes.Values
                                .Exists(v => v.AdditionalFields.GetValueOrDefault("collateral-arrangement-code", "").Equals(collateralRequirement.CollateralArrangementCode) &&
                                            v.AdditionalFields.GetValueOrDefault("collateral-kind", "").Equals(document.CollateralKind));
                            if (hasCollateralKind)
                            {
                                foreach (var deal in collateralRequirement.SecuredDealLinks)
                                {
                                    var appDocument = Convert(document);
                                    appDocument.ApplicationId = Application.ApplicationId;
                                    appDocument.ArrangementRequestId = request.ArrangementRequestId;
                                    appDocument.CollateralId = "" + collateralRequirement.CollateralRequirementId + "-" + deal.ArrangementNumber;

                                    //if (documents.Contains(appDocument))
                                    //{
                                    //    _logger.LogDebug("Found existing application document related to collateral.");
                                    //    var existingDoc = documents.FirstOrDefault(d => d.Equals(appDocument));
                                    //    existingHandledDocs.Add(existingDoc.DocumentId);
                                    //}
                                    //else
                                    //{
                                    //    _logger.LogDebug("Adding new application document related to collateral");
                                    //    documents.Add(appDocument);
                                    //}
                                }
                            }
                            else
                            {
                                _logger.LogInformation("Collateral kind {CollateralKind} not found for collateral arrangement code {CollateralArrangementCode}",
                                    document.CollateralKind, collateralRequirement.CollateralArrangementCode);
                            }
                        }
                    }
                }
            }
            var deleted = documents.Where(d => d.DocumentId != 0 && !existingHandledDocs.Contains(d.DocumentId) &&
                d.DocumentContextKind.Equals(DocumentContextKind.CollateralEnum) && d.Origin.Equals(DocumentOrigin.Product))
                .Select(d => d.DocumentId).ToList();
            _logger.LogInformation("There are {DeletedNumber} unused documents being deleted with collateral document context kind", deleted.Count());
            _logger.LogInformation("There are {ExistingNumber} existing documents with collateral document context kind", existingHandledDocs.Count());
            Existing.AddRange(existingHandledDocs);
            Deleted.AddRange(deleted);
        }

        public void ValidateRequirements(Application application)
        {
            Application = application;
            var documentsToValidate = Application.Documents;

            foreach (var item in NewRequirements)
            {
                //ApplicationDocument currentClone = Convert(item);
                var requirementMatchings = from it in documentsToValidate
                                           where it.ApplicationId == item.ApplicationId &&
                                                 it.DocumentKind == item.DocumentKind &&
                                                 it.DocumentContextKind == item.DocumentContextKind &&
                                                 (it.ArrangementRequestId == item.ArrangementRequestId) &&
                                                 (it.PartyId == item.PartyId)
                                           select it;
                if (requirementMatchings != null && requirementMatchings.Any())
                {

                    var document = requirementMatchings.FirstOrDefault();
                    PopulateValidationData(item, document.Status);
                }
                else
                {
                    item.ValidationStatus = DocumentRequirementValidationStatus.NotFoundEnum;
                }
            }
        }

        private ApplicationDocument Convert(DocumentValidationItemResponse document)
        {
            ApplicationDocument result = new ApplicationDocument();
            // result.ApplicationNumber = document.ApplicationNumber;
            result.DocumentKind = document.DocumentKind;
            result.PartyId = document.PartyId;
            result.DocumentContextKind = document.DocumentContextKind;
            result.ArrangementRequestId = document.ArrangementRequestId;
            result.CollateralId = document.CollateralId;
            result.Status = document.Status;

            return result;
        }

        private DocumentValidationItemResponse Convert(DocumentationValidationItemCommand command)
        {
            DocumentValidationItemResponse result = new DocumentValidationItemResponse();
            result.PartyRole = command.PartyRole;
            result.DocumentKind = command.DocumentType;
            result.DocumentContextKind = command.DocumentContextKind;
            result.CollateralKind = command.CollateralKind;
            result.RequiredStatus = command.RequiredStatus;
            result.DocumentKind = command.DocumentType;
            return result;
        }

        private void PopulateValidationData(DocumentValidationItemResponse requirement, DocumentStatus actualStatus)
        {
            // TODO: add ordering of statuses -> HIERARCHICAL ordering of STATUSES
            // some statuses implicitly are included in some other statuses
            // example: if document has status SIGNED -> that implies that status is allready COMPOSED
            requirement.ValidationStatus = DocumentRequirementValidationStatus.WrongStatusEnum;
            requirement.Status = actualStatus;
            if (requirement.RequiredStatus == actualStatus)
            {
                requirement.ValidationStatus = DocumentRequirementValidationStatus.ValidEnum;
            }
        }
    }

    public class ResolvedDocumentationData
    {
        public List<int> DeletedCount { get; set; }
        public List<long> ExistingCount { get; set; }
        public List<DocumentationValidationItemCommand> DocumentationRequirements { get; set; }
        public List<DocumentValidationItemResponse> NewRequirements = new List<DocumentValidationItemResponse>();
        public Application Application { get; set; }
    }
}
