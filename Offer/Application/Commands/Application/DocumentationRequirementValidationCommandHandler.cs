using MediatR;
using MicroserviceCommon.Application.Commands;
using MicroserviceCommon.Infrastructure.Idempotency;
using Offer.Domain.AggregatesModel.ApplicationAggregate;
using OfferApplication = Offer.Domain.AggregatesModel.ApplicationAggregate.Application;
using Offer.Domain.Utils;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AuditClient;
using AuditClient.Model;

namespace Offer.API.Application.Commands.Application
{
    public class DocumentationRequirementValidationCommandHandler : IRequestHandler<RequiredDocumentationValidationCommand, DocumentationValidationResponse>
    {
        private readonly IApplicationRepository _applicationRepository;
        private readonly RequiredDocumentationResolver _requiredDocumentationResolver;
        private readonly IAuditClient _auditClient;

        public DocumentationRequirementValidationCommandHandler(IApplicationRepository applicationRepository,
                                                                RequiredDocumentationResolver requiredDocumentationResolver, IAuditClient auditClient)
        {
            this._applicationRepository = applicationRepository;
            _requiredDocumentationResolver = requiredDocumentationResolver ?? throw new ArgumentNullException(nameof(requiredDocumentationResolver));
            _auditClient = auditClient ?? throw new ArgumentNullException(nameof(auditClient)); ;
        }

        public async Task<DocumentationValidationResponse> Handle(RequiredDocumentationValidationCommand message, CancellationToken cancellationToken)
        {
            OfferApplication application = await _applicationRepository.GetAsync(message.ApplicationNumber,
                "involved-parties,documents,arrangement-requests");

            if (application == null)
            {
                return null;
            }

            application.Documents = application.Documents ?? new List<ApplicationDocument>();
            var res = _requiredDocumentationResolver.ResolveDocuments(application, message.Items);

            await _auditClient.WriteLogEntry(AuditLogEntryAction.Validate, AuditLogEntryStatus.Success, "documents", application.ApplicationNumber.ToString(), "Resolving documents", new { });

            return res;
        }
    }

    public class DocumentationRequirementValidationCommandHandlerIdentifiedHandler : IdentifiedCommandHandler<RequiredDocumentationValidationCommand, DocumentationValidationResponse>
    {
        public DocumentationRequirementValidationCommandHandlerIdentifiedHandler(IMediator mediator, IRequestManager requestManager) : base(mediator, requestManager)
        {

        }
    }
}
