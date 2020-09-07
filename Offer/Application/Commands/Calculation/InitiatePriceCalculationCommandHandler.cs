using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MediatR;
using MicroserviceCommon.Application.Commands;
using MicroserviceCommon.Infrastructure.Idempotency;
using System.Threading;
using PriceCalculation.Models.Pricing;
using PriceCalculation.Services;
using PriceCalculation.Models.Product;
using Offer.Domain.Calculations;

namespace Offer.API.Application.Commands
{
    public class InitiatePriceCalculationCommandHandler : IRequestHandler<InitiatePriceCalculationCommand, PriceCalculationResponse>
    {
        private readonly OfferPriceCalculation _priceCalculator;
        private readonly IPriceCalculationService _priceCalculation;

        public InitiatePriceCalculationCommandHandler(
            OfferPriceCalculation priceCalculator, IPriceCalculationService priceCalculation)
        {
            this._priceCalculator = priceCalculator ?? throw new ArgumentNullException(nameof(priceCalculator));
            this._priceCalculation = priceCalculation ?? throw new ArgumentNullException(nameof(priceCalculation));
        }

        public async Task<PriceCalculationResponse> Handle(InitiatePriceCalculationCommand message, CancellationToken cancellationToken)
        {
            ProductConditions conditions = await _priceCalculation.ReadVariationDefinitions(new ProductConditions
            {
                Fees = message.Fees == null ? null : new List<FeeCondition>(message.Fees),
                InterestRates = message.InterestRates == null ? null : new List<InterestRateCondition>(message.InterestRates)
            });

            var calcParams = new PriceCalculationParameters
            {
                InterestRates = conditions.InterestRates,
                Fees = conditions.Fees,
                Amount = message.Amount,
                Term = message.Term,
                Currency = message.Currency,
                Channel = message.Channel,
                RiskScore = message.RiskScore,
                CustomerSegment = message.CustomerSegment,
                CollateralModel = message.CollateralModel,
                PartOfBundle = message.ProductBundling,
                Campaign = message.Campaign,
                Options = message.ProductOptions,
                CreditRating = message.CreditRating,
                CustomerValue = message.CustomerValue,
                DebtToIncome = message.DebtToIncome,
                AdditionalProperties = message.BundledComponents
            };

            var calcResult = await _priceCalculator.CalculatePrice(calcParams);
            var result = new PriceCalculationResponse
            {
                Fees = calcResult.Fees,
                InterestRates = calcResult.InterestRates,
                Napr = calcResult.Napr
            };
            return result;
        }
    }

    public class InitiatePriceCalculationIdentifiedCommandHandler : IdentifiedCommandHandler<InitiatePriceCalculationCommand, PriceCalculationResponse>
    {
        public InitiatePriceCalculationIdentifiedCommandHandler(IMediator mediator, IRequestManager requestManager) : base(mediator, requestManager)
        {

        }

        protected override PriceCalculationResponse CreateResultForDuplicateRequest()
        {
            return null;
        }
    }
}
