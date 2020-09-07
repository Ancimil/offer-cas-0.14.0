using System;

namespace Offer.Domain.AggregatesModel.ApplicationAggregate
{
    public class FinancialStatementsData
    {
        public long? ReportId { get; set; }

        public string ReportType { get; set; }
        public DateTime? ReportDate { get; set; }
                                           //   public string AccountingModel { get; set; }
    }

}
