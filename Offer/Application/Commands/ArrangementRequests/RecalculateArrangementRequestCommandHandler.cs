using MediatR;
using MicroserviceCommon.Application.Commands;
using MicroserviceCommon.Infrastructure.Idempotency;
using Offer.Domain.AggregatesModel.ApplicationAggregate;
using OfferApplication = Offer.Domain.AggregatesModel.ApplicationAggregate.Application;
using Offer.Domain.Calculations;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Offer.API.Application.Commands.ArrangementRequests
{
    public class RecalculateArrangementRequestCommandHandler : IRequestHandler<RecalculateArrangementRequestCommand, bool?>
    {
        private readonly IApplicationRepository _applicationRepository;
        private readonly IArrangementRequestRepository _arrangementRequestRepository;
        private readonly CalculatorProvider _calculatorProvider;
        public RecalculateArrangementRequestCommandHandler(
            IApplicationRepository applicationRepository,
            IArrangementRequestRepository arrangementRequestRepository,
            CalculatorProvider calculatorProvider
            )
        {

            this._applicationRepository = applicationRepository;
            this._arrangementRequestRepository = arrangementRequestRepository;
            _calculatorProvider = calculatorProvider ?? throw new ArgumentNullException(nameof(calculatorProvider));
        }
        public async Task<bool?> Handle(RecalculateArrangementRequestCommand message, CancellationToken cancellationToken)
        {
            //var conversionMethod = _configurationService.GetEffective("offer/fee-currency-conversion-method", "Buy to middle").Result;
            var arrangementRequest = _arrangementRequestRepository.GetArrangementRequest(message.ApplicationNumber, message.ArrangementRequestId);
            OfferApplication application = await _applicationRepository.GetAsync(message.ApplicationNumber);
            _calculatorProvider.Calculate(arrangementRequest, application);
            //arrangementRequest.CalculateOffer(application, priceCalculator, conversionMethod);
            await _arrangementRequestRepository.UpdateArrangementRequest(arrangementRequest);
            return await _arrangementRequestRepository.UnitOfWork.SaveEntitiesAsync();
        }
    }

    public class RecalculateArrangementRequestIdentifiedCommandHandler : IdentifiedCommandHandler<RecalculateArrangementRequestCommand, bool?>
    {
        public RecalculateArrangementRequestIdentifiedCommandHandler(IMediator mediator, IRequestManager requestManager) : base(mediator, requestManager)
        {

        }
    }
}
