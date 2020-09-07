using Newtonsoft.Json;
using Offer.Domain.AggregatesModel.ApplicationAggregate.AlternativeOfferModel;
using PriceCalculation.Models.Pricing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Offer.Domain.AggregatesModel.ApplicationAggregate
{
    public class FinanceServiceArrangementRequest : ArrangementRequest
    {
        // not used by all subtypes of arr req
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public decimal AmountInDomesticCurrency { get; set; }
        public DateTime? MaturityDate { get; set; }
        public decimal Eapr { get; set; }
        public decimal Napr { get; set; }
        public string Term { get; set; }

        public List<CollateralRequirement> CollateralRequirements { get; set; }


        public string CollateralModel { get; set; }


        [JsonIgnore]
        public string _AlternativeOffers { get; set; }

        [NotMapped]
        public List<AlternativeOffer> AlternativeOffers
        {
            get { return _AlternativeOffers == null ? null : JsonConvert.DeserializeObject<List<AlternativeOffer>>(_AlternativeOffers); }
            set { _AlternativeOffers = JsonConvert.SerializeObject(value); }
        }
        public decimal? LoanToValue { get; set; }
        public decimal? MaximalAnnuity { get; set; }
        public decimal? MaximalAmount { get; set; }

        public override bool IsFinanceService()
        {
            return true;
        }
        public override PriceCalculationParameters GetPriceCalculationParameters(Application application)
        {
            var parameters = base.GetPriceCalculationParameters(application);
            parameters.Amount = Amount;
            parameters.Term = Term;
            parameters.Currency = Currency;
            parameters.CollateralModel = CollateralModel;
            return parameters;
        }

    }
}
