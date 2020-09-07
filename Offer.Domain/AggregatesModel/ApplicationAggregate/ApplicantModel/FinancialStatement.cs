using System;
using System.Collections.Generic;

namespace Offer.Domain.AggregatesModel.ApplicationAggregate.ApplicantModel
{
    public class FinancialStatement
    {
            public int Year { get; set; }

            public List<Report> Reports { get; set; }

    }

    public class Report
    {
        public long? ReportId { get; set; }
        public string ReportType { get; set; }

        public string AccountingMethod { get; set; }
        public DateTime? ReportDate { get; set; }

    }
    
}
