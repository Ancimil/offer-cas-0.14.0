using PriceCalculation.Models.MarketRates;
using PriceCalculation.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PriceCalculatationTest.Services
{
    public class MarketRates : IMarketRatesService
    {
        private RateList EuriborList { get; set; }
        private RateList LiborList { get; set; }
        private RateList BeliborList { get; set; }

        private void Initialize()
        {
            if (EuriborList != null)
            {
                return;
            }
            EuriborList.Rates = new List<Rate>
            {
                new Rate { Code = "EURIBOR-1M", Frequency = "P1M", Title = "EURIBOR-1M" },
                new Rate { Code = "EURIBOR-3M", Frequency = "P3M", Title = "EURIBOR-3M" },
                new Rate { Code = "EURIBOR-6M", Frequency = "P6M", Title = "EURIBOR-6M" },
                new Rate { Code = "EURIBOR-12M", Frequency = "P12M", Title = "EURIBOR-12M" }
            };
            LiborList.Rates = new List<Rate>
            {
                new Rate { Code = "LIBOR-USD-OVERNIGHT", Frequency = "P1D", Title = "LIBOR-USD-OVERNIGHT" },
                new Rate { Code = "LIBOR-USD-WEEK", Frequency = "P7D", Title = "LIBOR-USD-WEEK" },
                new Rate { Code = "LIBOR-USD-1M", Frequency = "P1M", Title = "LIBOR-USD-1M" },
                new Rate { Code = "LIBOR-USD-3M", Frequency = "P3M", Title = "LIBOR-USD-3M" },
                new Rate { Code = "LIBOR-USD-6M", Frequency = "P6M", Title = "LIBOR-USD-6M" },
                new Rate { Code = "LIBOR-USD-12M", Frequency = "P12M", Title = "LIBOR-USD-12M" },
                new Rate { Code = "LIBOR-CHF-OVERNIGHT", Frequency = "P1D", Title = "LIBOR-CHF-OVERNIGHT" },
                new Rate { Code = "LIBOR-CHF-WEEK", Frequency = "P7D", Title = "LIBOR-CHF-WEEK" },
                new Rate { Code = "LIBOR-CHF-1M", Frequency = "P1M", Title = "LIBOR-CHF-1M" },
                new Rate { Code = "LIBOR-CHF-3M", Frequency = "P3M", Title = "LIBOR-CHF-3M" },
                new Rate { Code = "LIBOR-CHF-6M", Frequency = "P6M", Title = "LIBOR-CHF-6M" },
                new Rate { Code = "LIBOR-CHF-12M", Frequency = "P12M", Title = "LIBOR-CHF-12M" }
            };
            BeliborList.Rates = new List<Rate>
            {
                new Rate { Code = "BELIBOR-1M", Frequency = "P1M", Title = "BELIBOR-1M" },
                new Rate { Code = "BELIBOR-3M", Frequency = "P3M", Title = "BELIBOR-3M" },
                new Rate { Code = "BELIBOR-6M", Frequency = "P6M", Title = "BELIBOR-6M" }
            };
        }

        public async Task<RateValue> GetRate(string rateCode, DateTime dateTime)
        {
            switch (rateCode.ToUpper())
            {
                case "EURIBOR-1M":
                    return new RateValue { RateCode = "EURIBOR-1M", EffectiveDate = DateTime.Now, Value = Convert.ToDecimal(-0.370) };
                case "EURIBOR-3M": return new RateValue { RateCode = "EURIBOR-3M", EffectiveDate = DateTime.Now, Value = Convert.ToDecimal(-0.321) };
                case "EURIBOR-6M": return new RateValue { RateCode = "EURIBOR-6M", EffectiveDate = DateTime.Now, Value = Convert.ToDecimal(-0.268) };
                case "EURIBOR-12M": return new RateValue { RateCode = "EURIBOR-12M", EffectiveDate = DateTime.Now, Value = Convert.ToDecimal(-0.183) };
                case "LIBOR-USD-OVERNIGHT": return new RateValue { RateCode = "LIBOR-USD-OVERNIGHT", EffectiveDate = DateTime.Now, Value = Convert.ToDecimal(1.92550) };
                case "LIBOR-USD-WEEK": return new RateValue { RateCode = "LIBOR-CHF-WEEK", EffectiveDate = DateTime.Now, Value = Convert.ToDecimal(-0.321) };
                case "LIBOR-USD-1M": return new RateValue { RateCode = "LIBOR-USD-1M", EffectiveDate = DateTime.Now, Value = Convert.ToDecimal(2.08375) };
                case "LIBOR-USD-3M": return new RateValue { RateCode = "LIBOR-USD-3M", EffectiveDate = DateTime.Now, Value = Convert.ToDecimal(2.32469) };
                case "LIBOR-USD-6M": return new RateValue { RateCode = "LIBOR-USD-6M", EffectiveDate = DateTime.Now, Value = Convert.ToDecimal(2.49688) };
                case "LIBOR-USD-12M": return new RateValue { RateCode = "LIBOR-USD-12M", EffectiveDate = DateTime.Now, Value = Convert.ToDecimal(2.77188) };
                case "LIBOR-CHF-OVERNIGHT": return new RateValue { RateCode = "LIBOR-CHF-OVERNIGHT", EffectiveDate = DateTime.Now, Value = Convert.ToDecimal(-0.370) };
                case "LIBOR-CHF-WEEK": return new RateValue { RateCode = "LIBOR-CHF-WEEK", EffectiveDate = DateTime.Now, Value = Convert.ToDecimal(-0.321) };
                case "LIBOR-CHF-1M": return new RateValue { RateCode = "LIBOR-CHF-1M", EffectiveDate = DateTime.Now, Value = Convert.ToDecimal(-0.268) };
                case "LIBOR-CHF-3M": return new RateValue { RateCode = "LIBOR-CHF-3M", EffectiveDate = DateTime.Now, Value = Convert.ToDecimal(-0.183) };
                case "LIBOR-CHF-6M": return new RateValue { RateCode = "LIBOR-CHF-6M", EffectiveDate = DateTime.Now, Value = Convert.ToDecimal(-0.183) };
                case "LIBOR-CHF-12M": return new RateValue { RateCode = "LIBOR-CHF-12M", EffectiveDate = DateTime.Now, Value = Convert.ToDecimal(-0.183) };
                case "BELIBOR-1M": return new RateValue { RateCode = "BELIBOR-1M", EffectiveDate = DateTime.Now, Value = Convert.ToDecimal(1.69) };
                case "BELIBOR-3M": return new RateValue { RateCode = "BELIBOR-3M", EffectiveDate = DateTime.Now, Value = Convert.ToDecimal(1.97) };
                case "BELIBOR-6M":
                    return new RateValue { RateCode = "BELIBOR-6M", EffectiveDate = DateTime.Now, Value = Convert.ToDecimal(2.11) };
                default: throw new NotImplementedException();
            };
        }

        public async Task<RateListModel> GetRateList(string listCode)
        {
            Initialize();
            switch (listCode)
            {
                case "EURIBOR":
                    return new RateListModel
                    {
                        Rates = EuriborList.Rates
                    };
                case "LIBOR":
                    return new RateListModel
                    {
                        Rates = LiborList.Rates
                    };
                case "BELIBOR":
                    return new RateListModel
                    {
                        Rates = BeliborList.Rates
                    };
                default: throw new NotImplementedException();
            };
        }
    }
}
