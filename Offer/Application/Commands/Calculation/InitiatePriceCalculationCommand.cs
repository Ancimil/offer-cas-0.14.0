using System.Collections.Generic;
using MediatR;
using Offer.Domain.AggregatesModel.ApplicationAggregate;
using PriceCalculation.Models.Pricing;
using PriceCalculation.Models.Product;
using Newtonsoft.Json.Linq;
using PriceCalculation.Models.LeadModel;

namespace Offer.API.Application.Commands
{
    public class InitiatePriceCalculationCommand : IRequest<PriceCalculationResponse>
    {
        public ArrangementKind ArrangementKind { get; set; }
        public string Currency { get; private set; }
        public decimal Amount { get; private set; }
        public string Term { get; private set; }
        public List<FeeCondition> Fees { get; private set; }
        public List<InterestRateCondition> InterestRates { get; private set; }
        public string Channel { get; private set; }
        public decimal? RiskScore { get; private set; }
        public string CustomerSegment { get; private set; }
        public string ProductBundling { get; private set; }
        public string CollateralModel { get; private set; }
        public string CreditRating { get; set; }
        public decimal? CustomerValue { get; set; }
        public decimal? DebtToIncome { get; set; }
        public LeadModel Campaign { get; set; }
        public List<ProductOption> ProductOptions { get; set; }
        public Dictionary<string, JToken> BundledComponents { get; set; }
    }

}
