using Offer.Domain.AggregatesModel.ApplicationAggregate;
using System;

namespace Offer.Domain.View.AllDataViews
{
    public class CurrentEmployment
    {
        public string CompanyIdNumber { get; set; }
        public EmploymentKind? EmploymentKind { get; set; }
        public DateTime EmploymentStartDate { get; set; }
        public int WorkPeriod
        {
            get
            {
                var sumOfDays = (DateTime.Now - EmploymentStartDate).TotalDays;
                var months = Convert.ToInt32(Math.Round(sumOfDays / (365.25 / 12), 0));
                return months;
            }
        }
    }
}
