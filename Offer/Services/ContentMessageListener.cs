using Asseco.EventBus.Abstractions;
using Asseco.EventBus.Events;
using MediatR;
using MicroserviceCommon.Application.Commands;
using MicroserviceCommon.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Offer.API.Application.Commands;
using Offer.Domain.AggregatesModel.ApplicationAggregate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Offer.API.Services
{
    public class ContentMessageListener : IHostedService
    {
        private readonly IEventBus _bus;
        private readonly ILogger<CollateralMessageListener> _logger;
        private readonly IMediator _mediator;
        private readonly IServiceProvider _serviceProvider;
        private readonly IApplicationDocumentRepository _applicationDocumentRepository;

        public ContentMessageListener(
            IEventBus bus,
            IMediator mediator,
            ILogger<CollateralMessageListener> logger,
            IServiceProvider serviceProvider,
            IApplicationDocumentRepository applicationDocumentRepository
        )
        {
            this._bus = bus;
            this._mediator = mediator;
            this._logger = logger;
            _serviceProvider = serviceProvider;
            _applicationDocumentRepository = applicationDocumentRepository;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            CreateListener();
            _logger.LogDebug("Started listener for Content");
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        private void CreateListener()
        {
            _logger.LogDebug("Starting content listener with bus");
            _bus.Subscribe("content", "application_documents", new ContentMessageEventListener(this._mediator, this._logger, _serviceProvider, _applicationDocumentRepository));
        }
    }

    public class ContentMessageEventListener : IIntegrationEventHandler<MessageEvent>
    {
        private readonly ILogger<CollateralMessageListener> _logger;
        private readonly IMediator _mediator;
        private readonly IServiceProvider _serviceProvider;
        private readonly IApplicationDocumentRepository _applicationDocumentRepository;

        public ContentMessageEventListener(
          IMediator mediator,
          ILogger<CollateralMessageListener> logger,
          IServiceProvider serviceProvider,
          IApplicationDocumentRepository applicationDocumentRepository
        )
        {
            this._mediator = mediator;
            this._logger = logger;
            _serviceProvider = serviceProvider;
            _applicationDocumentRepository = applicationDocumentRepository;
        }

        public static String GetDocumentIdFromPath(String path)
        {
            int indexOfLastSlash = path.LastIndexOf("/") + 1;
            string id = path.Substring(indexOfLastSlash, path.Length - indexOfLastSlash);
            return id;
        }

        public async Task Handle(MessageEvent messageEvent)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var messageName = messageEvent.getStringProperty("messageName");
                // document uploaded event
                if (messageName.Equals("document-uploaded"))
                {
                    ContentDocumentMessage contentDocumentMessage;
                    try
                    {
                        contentDocumentMessage = GetContentDocumentMessage(messageEvent);
                        if (contentDocumentMessage == null)
                        {
                            _logger.LogWarning("Content Document Message is null. Aborting Content Message Handling");
                            return;
                        }
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, "An error occurred while getting content document message");
                        return;
                    }
                    var pathElements = contentDocumentMessage.Path.Trim('/').Split("/");
                    if (pathElements.Count() < 3)
                    {
                        return;
                    }
                    string applicationNumber = pathElements[1].ToString();
                    string documentId = pathElements[2].ToString();
                    if (string.IsNullOrEmpty(documentId))
                    {
                        return;
                    }
                    List<int> documentIds = new List<int>() { Convert.ToInt32(documentId) };

                    UpdateDocumentStatus(applicationNumber, documentIds, contentDocumentMessage);
                }

                // document deleted event
                if (messageName.Equals("document-deleted"))
                {
                    ContentDocumentMessage contentDocumentMessage = GetContentDocumentMessage(messageEvent);

                    var pathElements = contentDocumentMessage.Path.Trim('/').Split("/");
                    if (pathElements.Count() < 3)
                    {
                        return;
                    }
                    string applicationNumber = pathElements[1].ToString();
                    string documentId = pathElements[2].ToString();

                    if (string.IsNullOrEmpty(documentId))
                    {
                        return;
                    }
                    List<int> documentIds = new List<int>() { Convert.ToInt32(documentId) };
                    var document = _applicationDocumentRepository.GetApplicationDocumentsForIds(documentIds)?.FirstOrDefault();
                    if (document != null)
                    {
                        if (document.ApplicationNumber == applicationNumber)
                        {
                            var folderHasItems = await FolderHasItems(contentDocumentMessage.Path);
                            if (!folderHasItems)
                            {
                                contentDocumentMessage.FilingPurpose = DocumentStatus.EmptyEnum;
                                UpdateDocumentStatus(applicationNumber, documentIds, contentDocumentMessage);
                            }
                            else
                            {
                                _logger.LogInformation("Folder with path {Path} has items", contentDocumentMessage.Path);
                                return;
                            }
                        }
                        else
                        {
                            return;
                        }
                    }
                    else
                    {
                        _logger.LogError("An error occurred while trying to get app document for {ids}", documentIds);
                        return;
                    }
                }
            }
        }

        private async Task<bool> FolderHasItems(string folderPath)
        {
            var contentService = _serviceProvider.GetService<IContentService>();
            try
            {
                var folderItems = await contentService.GetFolderItems(folderPath);
                return folderItems.TotalCount > 0;
            }
            catch (KeyNotFoundException)
            {
                return false;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An error occurred while checking if content folder on path {ContentPath} has items", folderPath);
                return false;
            }
        }

        private void UpdateDocumentStatus(string applicationNumber, List<int> documentIds, ContentDocumentMessage contentDocumentMessage)
        {
            var updateApplicationDocumentsStatusCommand = new UpdateApplicationDocumentsStatusCommand
            {
                ApplicationId = long.Parse(applicationNumber),
                DocumentIds = documentIds,
                Status = contentDocumentMessage.FilingPurpose
            };

            var updateDocumentStatusCommand = new IdentifiedCommand<UpdateApplicationDocumentsStatusCommand, bool?>(updateApplicationDocumentsStatusCommand, new Guid());
            _mediator.Send(updateDocumentStatusCommand).Wait();
        }

        private ContentDocumentMessage GetContentDocumentMessage(MessageEvent messageEvent)
        {
            var serSettings = new JsonSerializerSettings();
            serSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
            ContentDocumentMessage contentDocumentMessage;
            try
            {
                contentDocumentMessage = (ContentDocumentMessage)JsonConvert
                    .DeserializeObject(messageEvent.getText(), typeof(ContentDocumentMessage), serSettings);
                if (!contentDocumentMessage.Path.Trim('/').StartsWith("offer/"))
                {
                    return null;
                }
                return contentDocumentMessage;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An error occurred while trying to convert message event text {MessageEventText} to ContentDocumentMessage",
                    messageEvent.getText());
                return null;
                // TODO Check if there is need to handle document update in case of an error or if path is not offer originated
                /*dynamic dynamicMsg = JsonConvert.DeserializeObject(messageEvent.getText());
                string path = dynamicMsg["path"]?.ToString();
                contentDocumentMessage = new ContentDocumentMessage
                {
                    Path = path,
                    FillingPurpose = DocumentStatus.UploadedEnum
                };*/
            }
        }
    }
}