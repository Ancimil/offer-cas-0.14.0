using Asseco.EventBus.Abstractions;
using MicroserviceCommon.Extensions.Broker;
using MicroserviceCommon.Models;
using MicroserviceCommon.Services;
using Microsoft.Extensions.Logging;
using Offer.Domain.AggregatesModel.ApplicationAggregate;
using Offer.Domain.AggregatesModel.ApplicationAggregate.ProductModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProductPartyRoleEnum = Offer.Domain.AggregatesModel.ApplicationAggregate.ProductModel.PartyRoleEnum;

namespace Offer.Domain.Utils
{
    public class ApplicationDocumentsResolver
    {
        private Application Application { get; set; }
        private List<ProductDocumentation> DocumentationRequirements { get; set; }
        private readonly IConfigurationService _configurationService;
        private readonly MicroserviceCommon.Services.IContentService _contentService;
        private readonly ILogger<ApplicationDocumentsResolver> _logger;
        private readonly IEventBus _bus;
        private readonly MessageEventFactory _messageEventFactory;
        private readonly List<int> Deleted = new List<int>();
        private readonly List<long> Existing = new List<long>();

        public ApplicationDocumentsResolver(IConfigurationService configurationService, MessageEventFactory messageEventFactory,
            ILogger<ApplicationDocumentsResolver> logger, MicroserviceCommon.Services.IContentService contentService, IEventBus bus)
        {
            _configurationService = configurationService ?? throw new ArgumentNullException(nameof(configurationService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _contentService = contentService ?? throw new ArgumentNullException(nameof(contentService));
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _messageEventFactory = messageEventFactory ?? throw new ArgumentNullException(nameof(messageEventFactory));
        }

        public List<ApplicationDocument> GetApplicationDocumentsForRequirements(Application application, List<ProductDocumentation> documentationRequirements = null)
        {
            if (application.Documents == null || application.ArrangementRequests == null || application.InvolvedParties == null)
            {
                throw new Exception("Not all application data are passed with application. Some of documents, arrangement requests or involved parties are null.");
            }
            this.Application = application;
            DocumentationRequirements = documentationRequirements;

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

            return Application.Documents;
        }

        public async Task<Application> ResolveDocuments(Application application)
        {
            _logger.LogInformation("Resolving documents for application {ApplicationNumber}", application.ApplicationNumber);
            if (application.Documents == null || application.ArrangementRequests == null || application.InvolvedParties == null)
            {
                throw new Exception("Not all application data are passed with application. Some of documents, arrangement requests or involved parties are null.");
            }
            this.Application = application;

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

            var offerDocumentPath = "/offer/" + application.ApplicationNumber + "/";
            var taskList = new List<Task>();
            _logger.LogDebug("Removing {DocumentCount} documents from content service for application {ApplicationNumber}", Deleted.Count(), application.ApplicationNumber);
            try
            {
                foreach (var docId in Deleted)
                {
                    _logger.LogDebug("Removing document {DocumentId} for application {ApplicationNumber} on path \"{ContentPath}\"",
                        docId, application.ApplicationNumber, (offerDocumentPath + docId));
                    taskList.Add(_contentService.DeleteFolderByPath(offerDocumentPath + docId));
                }
                await Task.WhenAll(taskList.ToArray());
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "An error occurred while trying to delete unused folders for application {ApplicationNumber} " +
                    "in process of resolving application documents", application.ApplicationNumber);
            }
            return Application;
        }

        public async Task<bool> CreateApplicationDocumentsFolders()
        {
            var taskList = new List<Task>();
            try
            {
                var newDocuments = Application.Documents.Where(d => !Existing.Contains(d.DocumentId)).ToList();
                _logger.LogInformation("Found {NewDocuments} new documents.", newDocuments.Count());
                if (newDocuments.Count() > 0)
                {
                    foreach (var document in newDocuments)
                    {
                        _logger.LogInformation("Creating document {DocumentId} for application {ApplicationNumber} on path \"{ContentPath}\"",
                            document.DocumentId, document.ApplicationNumber, ("/offer/" + document.ApplicationNumber));
                        taskList.Add(_contentService.CreateFolder("" + document.DocumentId, "/offer/" + document.ApplicationNumber, "folder", "generic-folder"));
                    }
                    await Task.WhenAll(taskList.ToArray());
                    /* Temporarily removed
                    var messageObj = _messageEventFactory.CreateBuilder("offer", documentComposistionEventName);
                    messageObj = messageObj.AddHeaderProperty("application-number", Application.ApplicationNumber)
                                .AddBodyProperty("refresh-documents", false)
                                .AddBodyProperty("application-number", Application.ApplicationNumber);
                    _logger.LogInformation("Sending \"{documentComposistionEventName}\" event to broker on topic \"{BrokerTopicName}\" for application number {ApplicationNumber}",
                        documentComposistionEventName, "offer", Application.ApplicationNumber);
                    _bus.Publish(messageObj.Build());*/
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An error occurred while creating Content folders for application documents");
            }
            return true;
        }

        private void ResolveForApplication()
        {
            var documents = Application.Documents;
            var existingHandledDocs = new List<long>();
            foreach (var request in Application.ArrangementRequests)
            {
                if (request.ProductSnapshot?.RequiredDocumentation == null)
                {
                    continue;
                }
                var applicationDocuments = request.ProductSnapshot.RequiredDocumentation.Where(d =>
                    d.DocumentContextKind.Equals(DocumentContextKind.ApplicationEnum)).ToList();
                foreach (var document in applicationDocuments)
                {
                    var appDocument = ApplicationDocument.FromProductDocument(document);
                    appDocument.ApplicationId = Application.ApplicationId;
                    appDocument.Context = "offer/" + Application.ApplicationNumber;

                    if (documents.Contains(appDocument))
                    {
                        var existingDoc = documents.FirstOrDefault(d => d.Equals(appDocument));
                        existingHandledDocs.Add(existingDoc.DocumentId);
                    }
                    else
                    {
                        documents.Add(appDocument);
                    }
                }
            }
            var deleted = documents.Where(d => d.DocumentId != 0 && !existingHandledDocs.Contains(d.DocumentId) &&
                d.DocumentContextKind.Equals(DocumentContextKind.ApplicationEnum) && d.Origin.Equals(DocumentOrigin.Product))
                .Select(d => d.DocumentId).ToList();
            _logger.LogInformation("There are {DeletedNumber} unused documents being deleted with application document context kind", deleted.Count());
            documents.RemoveAll(d => deleted.Contains(d.DocumentId));
            Existing.AddRange(existingHandledDocs);
            Deleted.AddRange(deleted);

        }

        private void ResolveForArrangementRequests()
        {
            var documents = Application.Documents;
            var existingHandledDocs = new List<long>();
            foreach (var request in Application.ArrangementRequests)
            {
                if (request.ProductSnapshot?.RequiredDocumentation == null)
                {
                    continue;
                }
                var arrRequestDocuments = request.ProductSnapshot.RequiredDocumentation.Where(d =>
                    d.DocumentContextKind.Equals(DocumentContextKind.ArrangementRequestEnum)).ToList();
                foreach (var document in arrRequestDocuments)
                {
                    if (!document.PartyRole.HasValue)
                    {
                        var appDocument = ApplicationDocument.FromProductDocument(document);
                        appDocument.ApplicationId = Application.ApplicationId;
                        appDocument.ArrangementRequestId = request.ArrangementRequestId;
                        appDocument.Context = "offer/" + Application.ApplicationNumber + "/arrangement-requests/" + request.ArrangementRequestId;

                        if (documents.Contains(appDocument))
                        {
                            var existingDoc = documents.FirstOrDefault(d => d.Equals(appDocument));
                            existingHandledDocs.Add(existingDoc.DocumentId);
                        }
                        else
                        {
                            documents.Add(appDocument);
                        }
                    }
                    else
                    {
                        foreach (var party in Application.InvolvedParties)
                        {
                            if (PartyCorespondsToDocumentPartyRole(party, document))
                            {
                                var appDocument = ApplicationDocument.FromProductDocument(document);
                                appDocument.ApplicationId = Application.ApplicationId;
                                appDocument.ArrangementRequestId = request.ArrangementRequestId;
                                appDocument.PartyId = party.PartyId;
                                if (documents.Contains(appDocument))
                                {
                                    var existingDoc = documents.FirstOrDefault(d => d.Equals(appDocument));
                                    existingHandledDocs.Add(existingDoc.DocumentId);
                                }
                                else
                                {
                                    documents.Add(appDocument);
                                }
                            }

                        }
                    }
                }
            }
            var deleted = documents.Where(d => d.DocumentId != 0 && !existingHandledDocs.Contains(d.DocumentId) &&
                d.DocumentContextKind.Equals(DocumentContextKind.ArrangementRequestEnum) && d.Origin.Equals(DocumentOrigin.Product))
                .Select(d => d.DocumentId).ToList();
            _logger.LogInformation("There are {DeletedNumber} unused documents being deleted with arrangement request document context kind", deleted.Count());
            documents.RemoveAll(d => deleted.Contains(d.DocumentId));
            Existing.AddRange(existingHandledDocs);
            Deleted.AddRange(deleted);
        }

        private void ResolveForParties()
        {
            var documents = Application.Documents;
            var existingHandledDocs = new List<long>();
            foreach (var request in Application.ArrangementRequests)
            {
                if (request.ProductSnapshot?.RequiredDocumentation == null)
                {
                    continue;
                }
                var partyDocuments = request.ProductSnapshot.RequiredDocumentation.Where(d =>
                    d.DocumentContextKind.Equals(DocumentContextKind.PartyEnum) && Application.InvolvedParties.Count() > 0).ToList();
                foreach (var document in partyDocuments)
                {
                    foreach (var party in Application.InvolvedParties)
                    {
                        var appDocument = ApplicationDocument.FromProductDocument(document);
                        appDocument.ApplicationId = Application.ApplicationId;
                        appDocument.PartyId = party.PartyId;
                        appDocument.Context = "offer/" + Application.ApplicationNumber + "/involved-parties/" + party.PartyId;

                        if (PartyCorespondsToDocumentPartyRole(party, document))
                        {
                            if (documents.Contains(appDocument))
                            {
                                var existingDoc = documents.FirstOrDefault(d => d.Equals(appDocument));
                                existingHandledDocs.Add(existingDoc.DocumentId);
                            }
                            else
                            {
                                documents.Add(appDocument);
                            }
                        }
                    }
                }
            }
            var deleted = documents.Where(d => d.DocumentId != 0 && !existingHandledDocs.Contains(d.DocumentId) &&
                d.DocumentContextKind.Equals(DocumentContextKind.PartyEnum) && d.Origin.Equals(DocumentOrigin.Product))
                .Select(d => d.DocumentId).ToList();
            _logger.LogInformation("There are {DeletedNumber} unused documents being deleted with party document context kind", deleted.Count());
            documents.RemoveAll(d => deleted.Contains(d.DocumentId));
            Existing.AddRange(existingHandledDocs);
            Deleted.AddRange(deleted);
        }

        // Check if party role corresponds to document PartyRole including "new" status for role
        private bool PartyCorespondsToDocumentPartyRole(Party party, ProductDocumentation document)
        {
            return ((party.PartyRole.Equals(PartyRole.Customer) && document.PartyRole.Equals(ProductPartyRoleEnum.CustomerEnum))
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
                                    && string.IsNullOrEmpty(party.CustomerNumber) && document.PartyRole.Equals(ProductPartyRoleEnum.NewGuarantorEnum)));
        }

        private void ResolveForCollaterals()
        {
            var documents = Application.Documents;
            var existingHandledDocs = new List<long>();
            var collateralCodes = _configurationService.GetEffective<ClassificationSchema>("collateral/classification-schemes/collateral-code").Result;
            foreach (var request in Application.ArrangementRequests)
            {
                if (request is FinanceServiceArrangementRequest financeRequest)
                {
                    if (request.ProductSnapshot?.RequiredDocumentation == null)
                    {
                        _logger.LogInformation("Required documentation on product snapshot is null. Passing to the next request...");
                        continue;
                    }
                    var collateralDocuments = request.ProductSnapshot.RequiredDocumentation.Where(d =>
                        d.DocumentContextKind.Equals(DocumentContextKind.CollateralEnum) && financeRequest.CollateralRequirements != null).ToList();
                    _logger.LogInformation("Found {NumberOfDocuments} documents related to collaterals", collateralDocuments.Count());
                    foreach (var document in collateralDocuments)
                    {
                        foreach (var collateralRequirement in financeRequest.CollateralRequirements)
                        {
                            var hasCollateralKind = collateralCodes.Values
                                .Exists(v => v.AdditionalFields.GetValueOrDefault("collateral-arrangement-code", "").Equals(collateralRequirement.CollateralArrangementCode) &&
                                            v.AdditionalFields.GetValueOrDefault("collateral-kind", "").Equals(document.CollateralKind));
                            if (hasCollateralKind)
                            {
                                collateralRequirement.SecuredDealLinks = collateralRequirement.SecuredDealLinks ?? new List<SecuredDealLink>();
                                foreach (var deal in collateralRequirement.SecuredDealLinks)
                                {
                                    var appDocument = ApplicationDocument.FromProductDocument(document);
                                    appDocument.ApplicationId = Application.ApplicationId;
                                    appDocument.ArrangementRequestId = request.ArrangementRequestId;
                                    appDocument.CollateralId = "" + collateralRequirement.CollateralRequirementId + "-" + deal.ArrangementNumber;
                                    appDocument.Context = "offer/" + Application.ApplicationNumber + "/arrangement-requests/" +
                                        request.ArrangementRequestId + "/collateral-arrangements/" + deal.ArrangementNumber;

                                    if (documents.Contains(appDocument))
                                    {
                                        _logger.LogDebug("Found existing application document related to collateral.");
                                        var existingDoc = documents.FirstOrDefault(d => d.Equals(appDocument));
                                        existingHandledDocs.Add(existingDoc.DocumentId);
                                    }
                                    else
                                    {
                                        _logger.LogDebug("Adding new application document related to collateral");
                                        documents.Add(appDocument);
                                    }
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
            documents.RemoveAll(d => deleted.Contains(d.DocumentId));
            Existing.AddRange(existingHandledDocs);
            Deleted.AddRange(deleted);
        }

    }
}
