using PriceCalculation.Models.MarketRates;
using System;
using System.Threading.Tasks;

namespace PriceCalculation.Services
{
    public interface IMarketRatesService
    {
        Task<RateValue> GetRate(string rateCode, DateTime dateTime);
        Task<RateListModel> GetRateList(string listCode);
    }
}
