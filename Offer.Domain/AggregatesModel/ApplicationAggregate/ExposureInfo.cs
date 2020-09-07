using MicroserviceCommon.Models;
using Offer.Domain.AggregatesModel.ExposureModel;
using System;
using System.Collections.Generic;

namespace Offer.Domain.AggregatesModel.ApplicationAggregate
{
    public class ExposureInfo
    {
        public ExposureList CurrentExposure { get; set; }
        public ExposureList NewExposureInOtherApplications { get; set; }
        public ExposureList NewExposureInCurrentApplication { get; set; }
        public ExposureList CreditBureauExposure { get; set; }
        public decimal? TotalExposureApprovedAmount { get; set; }
        public decimal? TotalExposureOutstandingAmount { get; set; }
        public decimal? TotalCbApprovedExposureAmount { get; set; }
        public decimal? TotalCbOutstandingAmount { get; set; }
        public string TotalExposureCurrency { get; set; }
        public DateTime? CalculatedDate { get; set; }
        public IDictionary<string, Currency> Calculated { get; set; }
        public IDictionary<string, decimal> CurrencyExchangeRates { get; set; }
    }

    public class ListConfigurationCalculateExposure
    {
        public IList<ConfigurationCalculateExposure> ConfigurationCalculateExposure { get; set; }
    }

    public class ConfigurationCalculateExposure
    {
        public string Name { get; set; }
        public string Column { get; set; }
        public string RiskCategories { get; set; }
        public string Sources { get; set; }
        public string Currency { get; set; }
    }
}
