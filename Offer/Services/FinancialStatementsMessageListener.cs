using Asseco.EventBus.Abstractions;
using Asseco.EventBus.Events;
using MediatR;
using MicroserviceCommon.Application.Commands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Offer.API.Application.Commands;
using Offer.Domain.AggregatesModel.ApplicationAggregate;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Offer.API.Services
{
    public class FinancialStatementsMessageListener : IHostedService
    {
        private readonly IEventBus _bus;
        private readonly ILogger<FinancialStatementsMessageListener> _logger;
        private readonly IServiceProvider _serviceProvider;

        public FinancialStatementsMessageListener(
            IEventBus bus,
            ILogger<FinancialStatementsMessageListener> logger,
            IServiceProvider serviceProvider
            )
        {
            this._bus = bus;
            this._logger = logger;
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
            _bus.Subscribe("financial_statements", "financial_statements_reports", new FinancialStatementsMessageEventListener(this._logger, _serviceProvider));
        }
    }

    public class FinancialStatementsMessageEventListener : IIntegrationEventHandler<MessageEvent>
    {
        private readonly ILogger<FinancialStatementsMessageListener> _logger;
        private readonly IServiceProvider _serviceProvider;
        private IMediator _mediator { get; set; }

        public FinancialStatementsMessageEventListener(
             ILogger<FinancialStatementsMessageListener> logger,
             IServiceProvider serviceProvider
            )
        {
            this._logger = logger;
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
                _mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                var messageName = messageEvent.getStringProperty("messageName");
                _logger.LogDebug("FinancialStatementsMessageEventListener got message {MessageName}", messageName);
                if (messageName.Equals("suppliers-buyers-report-added"))
                {
                    await HandleSuppliersBuyersReportAdded("suppliers-buyers-report-added", messageEvent);
                }
                else if (messageName.Equals("suppliers-buyers-report-removed"))
                {
                    await HandleSuppliersBuyersReportRemoved("suppliers-buyers-report-removed", messageEvent);
                }
                else if (messageName.Equals("financial-statements-report-added"))
                {
                    await HandleFinancialStatementsReportAdded("financial-statements-report-added", messageEvent);
                }
            }
        }

        private async Task<bool?> HandleSuppliersBuyersReportAdded(string messageName, MessageEvent messageEvent)
        {
            _logger.LogDebug("Handling message {MessageName}", messageName);
            _logger.LogDebug("Handling message content {MessageContent}", messageEvent.getText());
            string applicationNumber;
            try
            {
                applicationNumber = messageEvent.getStringProperty("XAseeApplicationNumber");
            }
            catch (KeyNotFoundException e)
            {
                _logger.LogError(e, "An error occurred while handling set of suppliers-buyers report");
                return false;
            }
            dynamic reportData = JsonConvert.DeserializeObject(messageEvent.getText());
            if (reportData == null ||
                string.IsNullOrEmpty(reportData["suppliers-buyers-report-id"]?.ToString()) ||
                string.IsNullOrEmpty(reportData["party-number"]?.ToString()))
            { 
                return false;
            }

            var customerNumber = reportData["party-number"].ToString();
            var reportId = long.Parse(reportData["suppliers-buyers-report-id"].ToString());

            var updateDocumentStatusCommand = new IdentifiedCommand<SetSuppliersBuyersReportCommand, bool?>
                (new SetSuppliersBuyersReportCommand
                (
                    long.Parse(applicationNumber), customerNumber, reportId
                ), new Guid());
            return await _mediator.Send(updateDocumentStatusCommand);
        }

        private async Task<bool?> HandleSuppliersBuyersReportRemoved(string messageName, MessageEvent messageEvent)
        {
            _logger.LogDebug("Handling message {MessageName}", messageName);
            _logger.LogDebug("Handling message content {MessageContent}", messageEvent.getText());
            string applicationNumber;
            try
            {
                applicationNumber = messageEvent.getStringProperty("XAseeApplicationNumber");
            }
            catch (KeyNotFoundException e)
            {
                _logger.LogError(e, "An error occurred while handling removal of suppliers-buyers report");
                return false;
            }
            dynamic reportData = JsonConvert.DeserializeObject(messageEvent.getText());
            if (reportData == null || string.IsNullOrEmpty(reportData["party-number"]?.ToString()))
            {
                return false;
            }

            var customerNumber = reportData["party-number"].ToString();

            var updateDocumentStatusCommand = new IdentifiedCommand<SetSuppliersBuyersReportCommand, bool?>
                (new SetSuppliersBuyersReportCommand
                (
                    long.Parse(applicationNumber), customerNumber, null
                ), new Guid());
            return await _mediator.Send(updateDocumentStatusCommand);
        }
        
        private async Task<bool> HandleFinancialStatementsReportAdded(string messageName, MessageEvent messageEvent)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                _logger.LogDebug("Handling message {MessageName}", messageName);
                _logger.LogDebug("Handling message content {MessageContent}", messageEvent.getText());
                _logger.LogDebug("Handling message content {MessageEvent}", messageEvent);
                string applicationNumber;                
                try
                {
                    applicationNumber = messageEvent.getStringProperty("XAseeApplicationNumber");
                }
                catch (KeyNotFoundException e)
                {
                    _logger.LogError(e, "An error occurred while handling addition of financial statements report");
                    return false;
                }
                dynamic reportData = JsonConvert.DeserializeObject(messageEvent.getText());
                if (reportData == null || string.IsNullOrEmpty("party-number"))
                {
                    return false;
                }
              
                var customerNumber = reportData["party-number"].ToString();
                var appRepository = scope.ServiceProvider.GetRequiredService<IApplicationRepository>();
                var app = await appRepository.GetAsync(long.Parse(applicationNumber),"involved-parties");


                var updateFinancialStatementsCommand = new IdentifiedCommand<UpdateFinancialStatementsCommand, bool>
                    (new UpdateFinancialStatementsCommand
                    (
                    app
                    ), new Guid());

                await _mediator.Send(updateFinancialStatementsCommand);
                await appRepository.UnitOfWork.SaveChangesAsync();
                return true;
            }
        }
    }
}
