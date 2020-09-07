using AssecoCurrencyConvertion;
using AutoMapper;
using MediatR;
using MicroserviceCommon.Application.Commands;
using MicroserviceCommon.Infrastructure.Idempotency;
using MicroserviceCommon.Models;
using MicroserviceCommon.Services;
using Microsoft.Extensions.Logging;
using Offer.Domain.AggregatesModel.ApplicationAggregate;
using OfferApplication = Offer.Domain.AggregatesModel.ApplicationAggregate.Application;
using Offer.Domain.Calculations;
using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AuditClient;
using AuditClient.Model;
using MicroserviceCommon.Extensions.Broker;
using Asseco.EventBus.Abstractions;

namespace Offer.API.Application.Commands
{
    public class UpdateArrangementRequestCommandHandler : IRequestHandler<UpdateArrangementRequestCommand, CommandStatus<bool>>
    {
        private readonly IArrangementRequestRepository _arrangementRequestRepository;
        private readonly IApplicationRepository _applicationRepository;
        private readonly IMediator _mediator;
        private readonly IConfigurationService _configurationService;
        private readonly OfferPriceCalculation priceCalculator;
        private readonly ILogger<UpdateArrangementRequestCommand> _logger;
        private readonly CalculatorProvider _calculatorProvider;
        private readonly IAuditClient _auditClient;
        private readonly MessageEventFactory _messageEventFactory;
        private readonly IEventBus _eventBus;

        public UpdateArrangementRequestCommandHandler(IMediator mediator,
            IArrangementRequestRepository arrangementRequestRepository,
            IConfigurationService configurationService,
            OfferPriceCalculation priceCalculator,
            IApplicationRepository applicationRepository,
        ILogger<UpdateArrangementRequestCommand> logger,
        CalculatorProvider calculatorProvider,
        IAuditClient auditClient,
        MessageEventFactory messageEventFactory,
        IEventBus eventBus)
        {
            this._arrangementRequestRepository = arrangementRequestRepository;
            this._applicationRepository = applicationRepository;
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            this.priceCalculator = priceCalculator;
            this._configurationService = configurationService ?? throw new ArgumentNullException(nameof(configurationService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _calculatorProvider = calculatorProvider ?? throw new ArgumentNullException(nameof(calculatorProvider));
            _auditClient = auditClient ?? throw new ArgumentNullException(nameof(auditClient));
            _messageEventFactory = messageEventFactory ?? throw new ArgumentNullException(nameof(messageEventFactory));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        }

        public async Task<CommandStatus<bool>> Handle(UpdateArrangementRequestCommand message, CancellationToken cancellationToken)
        {
            this._logger.LogInformation("handle u command handleru");
            message.ArrangementRequest.ApplicationId = message.ApplicationNumber;
            message.ArrangementRequest.ArrangementRequestId = message.ArrangementRequestId;
            ArrangementRequest request = _arrangementRequestRepository.GetArrangementRequest(message.ApplicationNumber, message.ArrangementRequestId);
            var conversionMethod = _configurationService.GetEffective("offer/fee-currency-conversion-method", "Buy to middle").Result;
            if (request == null)
            {
                _logger.LogWarning("Tried to update unexisting arrangement request with application number: {0} and arrangement request id: {1}",
                    message.ApplicationNumber, message.ArrangementRequestId);
                var e = new Exception(string.Format("Tried to update unexisting arrangement request with application number: {0} and arrangement request id: {1}",
                    message.ApplicationNumber, message.ArrangementRequestId));
                return new CommandStatus<bool> { Result = false, CommandResult = StandardCommandResult.BAD_REQUEST, Exception = e };
            }
            if (message.ArrangementRequest is TermLoanRequest termRequest &&
                termRequest.DisbursementsInfo != null && termRequest.DisbursementsInfo.Any())
            {
                var amountToBeChecked = termRequest.InvoiceAmount > 0 ? termRequest.InvoiceAmount : termRequest.Amount;
                CurrencyConverter currencyConverter = new CurrencyConverter();
                decimal sumOfDisbursementsInPrimaryCurrency = 0;
                foreach (var item in termRequest.DisbursementsInfo)
                {
                    if (item != null && item.Amount != null) continue;

                    if (item.Amount.Code == termRequest.Currency)
                    {
                        sumOfDisbursementsInPrimaryCurrency += item.Amount.Amount;
                    }
                    else
                    {
                        sumOfDisbursementsInPrimaryCurrency += currencyConverter.CurrencyConvert(item.Amount.Amount,
                            item.Amount.Code,
                            termRequest.Currency,
                            DateTime.Today.ToString("o", CultureInfo.InvariantCulture),
                            conversionMethod);
                    }

                    if (sumOfDisbursementsInPrimaryCurrency > amountToBeChecked)
                    {
                        _logger.LogWarning("Sum of disbursement info entries: {0}{2} is larger than invoice(credit) amount: {1}{2}", sumOfDisbursementsInPrimaryCurrency, amountToBeChecked, termRequest.Currency);
                        var e = new Exception(string.Format("Sum of disbursement info is above limits (invoice amount)"));
                        return new CommandStatus<bool> { Result = false, CommandResult = StandardCommandResult.BAD_REQUEST, Exception = e };
                    }
                }
            }
            message.ArrangementRequest.InstallmentPlan = request.InstallmentPlan;
            if (request.ProductCode == message.ArrangementRequest.ProductCode)
            {
                message.ArrangementRequest.ProductSnapshot = request.ProductSnapshot;
                message.ArrangementRequest.ProductName = request.ProductName;
            }
            else
            {
                return new CommandStatus<bool> { Result = false, CommandResult = StandardCommandResult.BAD_REQUEST, Exception = new Exception("An error occurred while updating arrangment - product code is wrong") };
                //var getProductData = new IdentifiedCommand<GetProductDataCommand, ProductData>(
                //    new GetProductDataCommand { ProductCode = message.ArrangementRequest.ProductCode }, new Guid());
                //ProductData productData = await _mediator.Send(getProductData);
                //ProductSnapshot snapshot = Mapper.Map<ProductData, ProductSnapshot>(productData);
                //message.ArrangementRequest.ProductSnapshot = snapshot;
                //message.ArrangementRequest.ProductName = snapshot.Name;
            }

            OfferApplication application =
                await _applicationRepository.GetAsync(message.ArrangementRequest.ApplicationId);
            application.PreferencialPrice = priceCalculator.HasPreferentialPrice(message.ArrangementRequest.Conditions);

            // Arrangement conditions
            request.Conditions = message.ArrangementRequest.Conditions;

            var priceCalculationParameters = request.GetPriceCalculationParameters(application);
            /*if (additionalProperties != null && additionalProperties.Keys.Count() > 0)
            {
                priceCalculationParameters.AdditionalProperties = priceCalculationParameters.AdditionalProperties ?? new Dictionary<string, JToken>();
                priceCalculationParameters.AdditionalProperties = priceCalculationParameters.AdditionalProperties
                    .Concat(additionalProperties
                            .Where(k => !priceCalculationParameters.AdditionalProperties.ContainsKey(k.Key))
                            .ToDictionary(k => k.Key, v => v.Value))
                    .ToDictionary(k => k.Key, v => v.Value);
            }*/

            message.ArrangementRequest = _calculatorProvider.Calculate(message.ArrangementRequest, application);
            //message.ArrangementRequest.CalculateOffer(priceCalculationParameters, priceCalculator, conversionMethod);

            await _arrangementRequestRepository.UpdateArrangementRequest(message.ArrangementRequest);

            var messageObj = _messageEventFactory.CreateBuilder("offer", "product-selection-changed")
                                .AddHeaderProperty("application-number", application.ApplicationNumber);
            messageObj = messageObj.AddBodyProperty("product-code", application.ProductCode)
                                   .AddBodyProperty("product-name", application.ProductName);
            _eventBus.Publish(messageObj.Build());

            bool result =  await _arrangementRequestRepository.UnitOfWork.SaveEntitiesAsync();
            await _auditClient.WriteLogEntry(AuditLogEntryAction.Update, AuditLogEntryStatus.Success, "arrangement", message.ApplicationNumber.ToString(), "Arrangement has been updated " + message.ArrangementRequestId, new { });
            StandardCommandResult commandResult = result ? StandardCommandResult.OK : StandardCommandResult.BAD_REQUEST;
            return new CommandStatus<bool> { Result = result, CommandResult = commandResult };
        }
    }

    public class UpdateArrangementRequestCommandIdentifiedHandler : IdentifiedCommandHandler<UpdateArrangementRequestCommand, CommandStatus<bool>>
    {
        public UpdateArrangementRequestCommandIdentifiedHandler(IMediator mediator, IRequestManager requestManager) : base(mediator, requestManager)
        {

        }

        protected override CommandStatus<bool> CreateResultForDuplicateRequest()
        {
            return null;
        }
    }
}
