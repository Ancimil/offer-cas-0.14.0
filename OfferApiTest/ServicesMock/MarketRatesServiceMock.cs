using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using PriceCalculation.Models.MarketRates;
using PriceCalculation.Models.Pricing;
using PriceCalculation.Services;

namespace OfferApiTest
{
    class MarketRatesServiceMock : IMarketRatesService
    {
        public Task<RateValue> GetRate(string rateCode, DateTime dateTime)
        {
            RateValue rateValue = new RateValue();
            switch (rateCode) {
                case "EURIBOR-3M":
                     rateValue = new RateValue()
                    {
                        Value = (decimal)-1.5,
                        RateCode = rateCode,
                        EffectiveDate = dateTime
                    };
                    break;
                case "EURIBOR-4M":
                    rateValue = new RateValue()
                    {
                        Value = 2,
                        RateCode = rateCode,
                        EffectiveDate = dateTime
                    };
                    break;
                case "EURIBOR-6M":
                    rateValue = new RateValue()
                    {
                        Value = 0,
                        RateCode = rateCode,
                        EffectiveDate = dateTime
                    };
                    break;
                case "BELIBOR-3M":
                    rateValue = new RateValue()
                    {
                        Value = (decimal)-2.5,
                        RateCode = rateCode,
                        EffectiveDate = dateTime
                    };
                    break;
                
                case "EARLY-EUR":
                    rateValue = new RateValue()
                    {
                        Value = (decimal)1.5,
                        RateCode = rateCode,
                        EffectiveDate = dateTime
                    };
                    break;
            }
            return Task.FromResult(rateValue);
        }

        public Task<RateListModel> GetRateList(string listCode)
        {
            throw new NotImplementedException();
        }
    }
}
