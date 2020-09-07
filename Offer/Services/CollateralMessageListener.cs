using Asseco.EventBus.Abstractions;
using Asseco.EventBus.Events;
using MediatR;
using MicroserviceCommon.Application.Commands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Offer.API.Application.Commands.ArrangementRequests.Collateral;
using Offer.Domain.AggregatesModel.ApplicationAggregate;
using Offer.Domain.Utils;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Offer.API.Services
{
    public class CollateralMessageListener : IHostedService
    {
        private readonly IEventBus _bus;
        private readonly IMediator _mediator;
        private readonly ILogger<CollateralMessageListener> _logger;
        private readonly ApplicationDocumentsResolver _documentsResolver;
        private readonly IServiceProvider _serviceProvider;

        public CollateralMessageListener(
            IEventBus bus,
            IMediator mediator,
             ILogger<CollateralMessageListener> logger,
             IApplicationRepository appRepository,
             IServiceProvider serviceProvider,
             ApplicationDocumentsResolver documentsResolver
            )
        {
            this._bus = bus;
            this._mediator = mediator;
            this._logger = logger;
            _documentsResolver = documentsResolver;
            _serviceProvider = serviceProvider;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            CreateListener();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        private void CreateListener()
        {
            _bus.Subscribe("collateral", "secure_deal_links", new CollateralMessageEventListener(this._mediator, this._logger, this._documentsResolver, _serviceProvider));
        }
    }

    public class CollateralMessageEventListener : IIntegrationEventHandler<MessageEvent>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<CollateralMessageListener> _logger;
        private readonly ApplicationDocumentsResolver _documentsResolver;
        private readonly IServiceProvider _serviceProvider;

        public CollateralMessageEventListener(
             IMediator mediator,
             ILogger<CollateralMessageListener> logger,
             ApplicationDocumentsResolver documentsResolver,
             IServiceProvider serviceProvider
            )
        {
            this._mediator = mediator;
            this._logger = logger;
            _documentsResolver = documentsResolver;
            _serviceProvider = serviceProvider;
        }

        public string[] ParseDealNumber(string dealNumber)
        {
            if (dealNumber == null)
            {
                _logger.LogError("Could not corellate message. No dealNumber provided");
                return null;
            }
            string[] applicationData = dealNumber.Split("-");
            if (applicationData.Length != 2)
            {
                _logger.LogError("Could not corellate message. Deal number provided does not have required elements");
                return null;
            }
            return applicationData;
        }

        public async Task Handle(MessageEvent messageEvent)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var messageName = messageEvent.getStringProperty("messageName");
                _logger.LogDebug("CollateralMessageEventListener got message {MessageName}", messageName);
                string applicationNumber = null;
                if (messageName.Equals("secured-deal-link-added") || messageName.Equals("secured-deal-link-updated"))
                {
                    _logger.LogDebug("Handling message {MessageName}", messageName);
                    _logger.LogDebug("Handling message content {MessageContent}", messageEvent.getText());
                    dynamic dynamicMessageObj = JsonConvert.DeserializeObject(messageEvent.getText());
                    string[] applicationData = ParseDealNumber(dynamicMessageObj["deal-number"]?.ToString());
                    if (applicationData == null)
                    {
                        return;
                    }
                    applicationNumber = applicationData[0];
                    if (!int.TryParse(applicationData[1], out int arrangementRequestId))
                    {
                        _logger.LogError("Could not corellate message. No arrangementRequestId provided");
                        return;
                    }
                    string stringCollateralRequirementId = null;
                    try
                    {
                        stringCollateralRequirementId = messageEvent.getStringProperty("collateral-requirement-id");
                    }
                    catch
                    {
                        // ignore, string is already null
                    }
                    _logger.LogDebug("stringCollateralRequirementId: \"{stringCollateralRequirementId}\"", stringCollateralRequirementId);

                    if (stringCollateralRequirementId == null || !int.TryParse(stringCollateralRequirementId, out int collateralRequirementId))
                    {
                        _logger.LogError("Could not corellate message. No collateralRequirementId provided");
                        return;
                    }
                    var stringPledgedValueInCollateralCurrency = dynamicMessageObj["pledged-value-on-deal"]?.ToString();
                    decimal pledgedValueInCollateralCurrency = 0;
                    if (string.IsNullOrEmpty(stringPledgedValueInCollateralCurrency) ||
                        !decimal.TryParse(stringPledgedValueInCollateralCurrency, out pledgedValueInCollateralCurrency))
                    {
                        _logger.LogError("Could not corellate message. No plagedValueInCollateralCurrency provided");
                        return;
                    }
                    var arrangementCurrency = dynamicMessageObj["arrangement-currency"]?.ToString();
                    var arrangementNumber = dynamicMessageObj["arrangement-number"]?.ToString();
                    if (arrangementCurrency == null)
                    {
                        _logger.LogError("Could not corellate message. No arrangementCurrency provided");
                        return;
                    }
                    if (arrangementNumber == null)
                    {
                        _logger.LogError("Could not corellate message. No arrangementNumber provided");
                        return;
                    }
                    var addCollateralCommand = new AddSecuredDealLinkCommand
                    {
                        ApplicationNumber = applicationNumber,
                        ArrangementRequestId = arrangementRequestId,
                        ArrangementNumber = arrangementNumber,
                        ArrangementCurrency = arrangementCurrency,
                        CollateralRequirementId = collateralRequirementId,
                        PledgedValueInCollateralCurrency = pledgedValueInCollateralCurrency
                    };
                    var updateCollateralCommand = new IdentifiedCommand<AddSecuredDealLinkCommand, bool>(addCollateralCommand, new Guid());
                    var commandResult = await _mediator.Send(updateCollateralCommand);
                }
                else if (messageName.Equals("secured-deal-link-deleted"))
                {
                    dynamic dynamicMessageObj = JsonConvert.DeserializeObject(messageEvent.getText());
                    string[] applicationData = ParseDealNumber(dynamicMessageObj["deal-number"]?.ToString());
                    if (applicationData == null)
                    {
                        return;
                    }
                    applicationNumber = applicationData[0];
                    if (!int.TryParse(applicationData[1], out int arrangementRequestId))
                    {
                        _logger.LogError("Could not corellate message. No arrangementRequestId provided");
                        return;
                    }
                    string stringCollateralRequirementId = null;
                    try
                    {
                        stringCollateralRequirementId = messageEvent.getStringProperty("collateral-requirement-id");
                    }
                    catch
                    {
                        // ignore, string is already null
                    }
                    _logger.LogDebug("stringCollateralRequirementId: \"{stringCollateralRequirementId}\"", stringCollateralRequirementId);
                    if (stringCollateralRequirementId == null || !int.TryParse(stringCollateralRequirementId, out int collateralRequirementId))
                    {
                        _logger.LogError("Could not corellate message. No collateralRequirementId provided");
                        return;
                    }
                    var arrangementNumber = dynamicMessageObj["arrangement-number"]?.ToString();
                    if (arrangementNumber == null)
                    {
                        _logger.LogError("Could not corellate message. No arrangementNumber provided");
                        return;
                    }
                    var deleteSecureDealLinkCommand = new DeleteSecuredDealLinkCommand
                    {
                        ApplicationNumber = applicationNumber,
                        ArrangementRequestId = arrangementRequestId,
                        ArrangementNumber = arrangementNumber,
                        CollateralRequirementId = collateralRequirementId
                    };
                    var deleteSecuredCommand = new IdentifiedCommand<DeleteSecuredDealLinkCommand, bool>(deleteSecureDealLinkCommand, new Guid());
                    var commandResult = await _mediator.Send(deleteSecuredCommand);
                }

                if (!string.IsNullOrEmpty(applicationNumber))
                {

                    _logger.LogDebug("Start resolving of documents for {ApplicationNumber}", applicationNumber);
                    var appRepository = scope.ServiceProvider.GetRequiredService<IApplicationRepository>();
                    var app = await appRepository.GetAsync(long.Parse(applicationNumber),
                        "arrangement-requests.collateral-requirements,documents,involved-parties");
                    app = await _documentsResolver.ResolveDocuments(app);
                    await appRepository.UnitOfWork.SaveChangesAsync();
                    await _documentsResolver.CreateApplicationDocumentsFolders();
                }
            }
        }
    }
}
