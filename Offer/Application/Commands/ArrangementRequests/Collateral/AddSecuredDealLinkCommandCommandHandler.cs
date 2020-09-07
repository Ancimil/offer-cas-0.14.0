using Asseco.EventBus.Abstractions;
using AssecoCurrencyConvertion;
using MediatR;
using MicroserviceCommon.Application.Commands;
using MicroserviceCommon.Extensions.Broker;
using MicroserviceCommon.Infrastructure.Idempotency;
using MicroserviceCommon.Services;
using Microsoft.Extensions.Logging;
using Offer.Domain.AggregatesModel.ApplicationAggregate;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Offer.API.Application.Commands.ArrangementRequests.Collateral
{
    public class AddSecuredDealLinkCommandCommandHandler : IRequestHandler<AddSecuredDealLinkCommand, bool>
    {
        private readonly IArrangementRequestRepository _arrangementRequestRepository;
        private readonly IEventBus _bus;
        private readonly IMediator _mediator;
        private readonly IConfigurationService _configurationService;
        private readonly ILogger<AddSecuredDealLinkCommand> _logger;
        private readonly MessageEventFactory _messageEventFactory;

        public AddSecuredDealLinkCommandCommandHandler(IMediator mediator,
            IArrangementRequestRepository arrangementRequestRepository,
            IEventBus bus,
            IConfigurationService configurationService,
            MessageEventFactory messageEventFactory,
            ILogger<AddSecuredDealLinkCommand> logger
        )
        {
            this._arrangementRequestRepository = arrangementRequestRepository;
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _configurationService = configurationService ?? throw new ArgumentNullException(nameof(configurationService));
            this._messageEventFactory = messageEventFactory;
        }

        public async Task<bool> Handle(AddSecuredDealLinkCommand message, CancellationToken cancellationToken)
        {
            var longApplicationNumber = long.Parse(message.ApplicationNumber);
            CollateralRequirement requirement = _arrangementRequestRepository.GetCollateralRequirementById(longApplicationNumber, message.ArrangementRequestId, message.CollateralRequirementId);
            List<SecuredDealLink> securedDealLinks = requirement?.SecuredDealLinks ?? new List<SecuredDealLink>();
            ArrangementRequest request = _arrangementRequestRepository.GetArrangementRequest(longApplicationNumber, message.ArrangementRequestId);
            if (request is FinanceServiceArrangementRequest finRequest)
            {
                CurrencyConverter currencyConverter = new CurrencyConverter();
                var currencyConversionMethod = _configurationService.GetEffective("offer/currency-conversion-method").Result;
                var pledgedValueInLoanCurrency = currencyConverter.CurrencyConvert(message.PledgedValueInCollateralCurrency, message.ArrangementCurrency, finRequest.Currency, DateTime.Today.ToString("o", CultureInfo.InvariantCulture), currencyConversionMethod);

                var existingSecuredDealLink = securedDealLinks.Where
                    (x => x.ArrangementRequestId.Equals(message.ArrangementRequestId) &&
                    x.ApplicationNumber.Equals(message.ApplicationNumber) &&
                    x.ArrangementNumber.Equals(message.ArrangementNumber)).FirstOrDefault();

                if (existingSecuredDealLink != null)
                {
                    existingSecuredDealLink.PledgedValueInCollateralCurrency = message.PledgedValueInCollateralCurrency;
                    existingSecuredDealLink.PledgedValueInLoanCurrency = pledgedValueInLoanCurrency;
                    requirement.SecuredDealLinks = securedDealLinks;
                }
                else
                {
                    securedDealLinks.Add(new SecuredDealLink
                    {
                        ApplicationNumber = message.ApplicationNumber,
                        ArrangementNumber = message.ArrangementNumber,
                        ArrangementRequestId = message.ArrangementRequestId,
                        PledgedValueInCollateralCurrency = message.PledgedValueInCollateralCurrency,
                        PledgedValueInLoanCurrency = pledgedValueInLoanCurrency
                    });
                    requirement.SecuredDealLinks = securedDealLinks;
                }
                _arrangementRequestRepository.UpdateCollateralRequirement(requirement);
                var result = await _arrangementRequestRepository.UnitOfWork.SaveEntitiesAsync();

                var msgBuilder = 
                    _messageEventFactory.CreateBuilder("offer", "secured-deal-link-changed")
                          .AddHeaderProperty("application-number", message.ApplicationNumber)
                          .AddHeaderProperty("username", "ALL");

                _bus.Publish(msgBuilder.Build());
                return result;
            }
            else
            {
                return false;
            }
        }
    }

    public class AddSecuredDealLinkCommandIdentifiedHandler : IdentifiedCommandHandler<AddSecuredDealLinkCommand, bool>
    {
        public AddSecuredDealLinkCommandIdentifiedHandler(IMediator mediator, IRequestManager requestManager) : base(mediator, requestManager)
        {

        }

        protected override bool CreateResultForDuplicateRequest()
        {
            return true;
        }
    }
}
