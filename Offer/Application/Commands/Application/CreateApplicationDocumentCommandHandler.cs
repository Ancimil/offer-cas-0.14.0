using System;
using System.Threading.Tasks;
using MediatR;
using MicroserviceCommon.Application.Commands;
using MicroserviceCommon.Infrastructure.Idempotency;
using Offer.Domain.AggregatesModel.ApplicationAggregate;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Asseco.EventBus.Abstractions;
using Offer.Domain.Exceptions;
using MicroserviceCommon.Models;
using MicroserviceCommon.Services;
using System.Threading;
using AuditClient;
using AuditClient.Model;

namespace Offer.API.Application.Commands
{
    public class CreateApplicationDocumentCommandHandler : IRequestHandler<CreateApplicationDocumentCommand, CommandStatus<ApplicationDocument>>
    {
        private readonly IApplicationRepository _applicationRepository;
        private readonly IEventBus _eventBus;
        private readonly ILogger<CreateApplicationDocumentCommand> _logger;
        private readonly IContentService _contentService;
        private readonly IAuditClient _auditClient;

        public CreateApplicationDocumentCommandHandler(IEventBus eventBus, IApplicationRepository applicationRepository,
            ILogger<CreateApplicationDocumentCommand> logger, IContentService contentService, IAuditClient auditClient)
        {
            this._applicationRepository = applicationRepository ?? throw new ArgumentNullException(nameof(applicationRepository));
            this._eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this._contentService = contentService ?? throw new ArgumentNullException(nameof(contentService));
            _auditClient = auditClient ?? throw new ArgumentNullException(nameof(auditClient));
        }

        public async Task<CommandStatus<ApplicationDocument>> Handle(CreateApplicationDocumentCommand message, CancellationToken cancellationToken)
        {
            if (message.Origin.Equals(DocumentOrigin.Product))
            {
                _logger.LogWarning("Tried to add application document \"{DocumentName}\" with Product origin outside of product definition, for application: {applicationNumber}",
                    message.DocumentName, message.ApplicationNumber);
                var e = new Exception("Tried to add application document \"" + message.DocumentName +
                    "\" with Product origin outside of product definition, for application: " + message.ApplicationNumber);
                return new CommandStatus<ApplicationDocument> { CommandResult = StandardCommandResult.BAD_REQUEST, Exception = e };
            }
            _logger.LogInformation("Creating application document for application: {applicationNumber}", message.ApplicationNumber);
            var document = Mapper.Map<CreateApplicationDocumentCommand, ApplicationDocument>(message);
            ApplicationDocument newDocument = null;
            try
            {
                newDocument = _applicationRepository.CreateApplicationDocument(message.ApplicationNumber, document);

            }
            catch (ApplicationNotFoundException ex)
            {
                return new CommandStatus<ApplicationDocument> { CommandResult = StandardCommandResult.NOT_FOUND, Exception = ex };
            }
            var finished = await _applicationRepository.UnitOfWork.SaveEntitiesAsync();
            await _auditClient.WriteLogEntry(AuditLogEntryAction.Create, AuditLogEntryStatus.Success, "document", message.ApplicationNumber.ToString(), "New document created", newDocument);
            if (finished)
            {
                try
                {
                    await _contentService.CreateFolder("" + newDocument.DocumentId, "/offer/" + newDocument.ApplicationNumber, "folder", "generic-folder");
                    return new CommandStatus<ApplicationDocument> { Result = newDocument, CommandResult = StandardCommandResult.OK };
                }
                catch (DuplicateObjectException)
                {
                    _logger.LogWarning("Got duplicate object exception while adding document for application {ApplicationNumber}", newDocument.ApplicationNumber);
                    return new CommandStatus<ApplicationDocument> { Result = newDocument, CommandResult = StandardCommandResult.OK };
                }
                catch (Exception e)
                {
                    return new CommandStatus<ApplicationDocument> { CommandResult = StandardCommandResult.INTERNAL_ERROR, Exception = e };
                }
            }
            else
            {
                _logger.LogError("An error occurred while creating application document named '{documentName}' for application {applicationNumber}",
                    message.DocumentName, message.ApplicationNumber);
                return new CommandStatus<ApplicationDocument> { CommandResult = StandardCommandResult.INTERNAL_ERROR };

            }
        }
    }

    public class CreateApplicationDocumentCommandIdentifiedHandler : IdentifiedCommandHandler<CreateApplicationDocumentCommand, CommandStatus<ApplicationDocument>>
    {
        public CreateApplicationDocumentCommandIdentifiedHandler(IMediator mediator, IRequestManager requestManager) : base(mediator, requestManager)
        {

        }

        protected override CommandStatus<ApplicationDocument> CreateResultForDuplicateRequest()
        {
            return null;
        }
    }
}
