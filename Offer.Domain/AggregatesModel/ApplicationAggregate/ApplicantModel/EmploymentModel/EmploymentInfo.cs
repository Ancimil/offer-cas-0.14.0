using System;
using System.Collections.Generic;

namespace Offer.Domain.AggregatesModel.ApplicationAggregate
{
    public class EmploymentInfo
    {
        public string CompanyIdNumber { get; set; }
        public string EmployerName { get; set; }
        public DateTime EmploymentStartDate { get; set; }
        public DateTime? EmploymentEndDate { get; set; }
        public string PositionCategory { get; set; }
        public EmploymentKind? EmploymentKind { get; set; }
        public EmployerKinds? EmployerLegalStructure { get; set; }
    }

    public class EmploymentData
    {
        public string TotalWorkPeriod { get; set; }
        public string EmploymentStatus { get; set; }
        public DateTime? EmploymentStatusDate { get; set; }
        public string PreviousWorkPeriod { get; set; }
        public List<EmploymentInfo> Employments { get; set; }
    }
}
