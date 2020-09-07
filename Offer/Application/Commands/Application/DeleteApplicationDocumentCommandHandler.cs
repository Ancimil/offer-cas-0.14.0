using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MediatR;
using MicroserviceCommon.Application.Commands;
using MicroserviceCommon.Infrastructure.Idempotency;
using Offer.Domain.AggregatesModel.ApplicationAggregate;
using Microsoft.Extensions.Logging;
using MicroserviceCommon.Services;
using System.Threading;
using AuditClient;
using AuditClient.Model;

namespace Offer.API.Application.Commands
{
    public class DeleteApplicationDocumentCommandHandler : IRequestHandler<DeleteApplicationDocumentCommand, bool?>
    {
        private readonly IApplicationDocumentRepository _applicationDocumentRepository;
        private readonly ILogger<DeleteApplicationDocumentCommand> _logger;
        private readonly IContentService _contentService;
        private readonly IAuditClient _auditClient;

        public DeleteApplicationDocumentCommandHandler(IApplicationDocumentRepository applicationDocumentRepository,
            ILogger<DeleteApplicationDocumentCommand> logger, IContentService contentService, IAuditClient auditClient)
        {
            this._applicationDocumentRepository = applicationDocumentRepository ?? throw new ArgumentNullException(nameof(applicationDocumentRepository));
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this._contentService = contentService ?? throw new ArgumentNullException(nameof(contentService));
            _auditClient = auditClient ?? throw new ArgumentNullException(nameof(auditClient));
        }

        public async Task<bool?> Handle(DeleteApplicationDocumentCommand message, CancellationToken cancellationToken)
        {
            var deletedDocument = _applicationDocumentRepository.DeleteDocument(message.ApplicationNumber, message.DocumentId);
            if (deletedDocument == null)
            {
                return null;
            }
            var folderPath = "/offer/" + message.ApplicationNumber + "/" + message.DocumentId;
            try
            {
                await _contentService.DeleteFolderByPath(folderPath);
                return await _applicationDocumentRepository.UnitOfWork.SaveEntitiesAsync();
            }
            catch (AggregateException e) when (e.InnerException is KeyNotFoundException)
            {
                _logger.LogWarning("Document deleted from Offer DB but not found on Content path {ContentFolderPath}", folderPath);
                await _auditClient.WriteLogEntry(AuditLogEntryAction.Delete, AuditLogEntryStatus.Success, "document", message.ApplicationNumber.ToString(), "Document has been deleted", deletedDocument);
                return await _applicationDocumentRepository.UnitOfWork.SaveEntitiesAsync();
            }
            catch
            {
                return false;
            }
        }
    }

    public class DeleteApplicationDocumentCommandIdentifiedHandler : IdentifiedCommandHandler<DeleteApplicationDocumentCommand, bool?>
    {
        public DeleteApplicationDocumentCommandIdentifiedHandler(IMediator mediator, IRequestManager requestManager) : base(mediator, requestManager)
        {

        }
    }
}
