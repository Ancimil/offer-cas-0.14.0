using Asseco.EventBus.Abstractions;
using MediatR;
using MicroserviceCommon.Application.Commands;
using MicroserviceCommon.Extensions.Broker;
using MicroserviceCommon.Infrastructure.Idempotency;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Offer.Domain.AggregatesModel.ApplicationAggregate;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Offer.API.Application.Commands
{
    public class UpdateApplicationDocumentsStatusCommandHandler : IRequestHandler<UpdateApplicationDocumentsStatusCommand, bool?>
    {
        private readonly IApplicationDocumentRepository _applicationDocumentRepository;
        private readonly MessageEventFactory _messageEventFactory;
        private readonly IEventBus _bus;
        private readonly ILogger<UpdateApplicationDocumentsStatusCommandHandler> _logger;

        public UpdateApplicationDocumentsStatusCommandHandler(
            IApplicationDocumentRepository applicationDocumentRepository,
            MessageEventFactory messageEventFactory,
            IEventBus bus, ILogger<UpdateApplicationDocumentsStatusCommandHandler> logger)
        {
            this._applicationDocumentRepository = applicationDocumentRepository;
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _messageEventFactory = messageEventFactory;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<bool?> Handle(UpdateApplicationDocumentsStatusCommand message, CancellationToken cancellationToken)
        {
            var documents = _applicationDocumentRepository.GetApplicationDocumentsForIds(message.DocumentIds);
            if (documents == null)
            {
                return null;
            }
            documents.ForEach(d => d.Status = message.Status);
            _applicationDocumentRepository.UpdateApplicationDocuments(documents);
            try
            {
                foreach(var d in documents)
                {
                    var status = d.Status;
                    var serializedStatus = JsonConvert.SerializeObject(status, new Newtonsoft.Json.Converters.StringEnumConverter());
                    var msgBuilder =
                    _messageEventFactory.CreateBuilder("offer", "document-status-changed")
                          .AddHeaderProperty("application-number", message.ApplicationNumber)
                          .AddHeaderProperty("document-id", d.DocumentId)
                          .AddHeaderProperty("document-status", JsonConvert.DeserializeObject(serializedStatus).ToString())
                          .AddHeaderProperty("username", "ALL");

                    _bus.Publish(msgBuilder.Build());
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpdateApplicationDocumentsStatusCommandHandler error");
            }
           
            var saveResult = await _applicationDocumentRepository.UnitOfWork.SaveEntitiesAsync();
            return saveResult;
        }
    }

    public class UpdateApplicationDocumentsCommandIdentifiedHandler : IdentifiedCommandHandler<UpdateApplicationDocumentsStatusCommand, bool?>
    {
        public UpdateApplicationDocumentsCommandIdentifiedHandler(IMediator mediator, IRequestManager requestManager) : base(mediator, requestManager)
        {
        }

        protected override bool? CreateResultForDuplicateRequest()
        {
            return true;
        }
    }
}