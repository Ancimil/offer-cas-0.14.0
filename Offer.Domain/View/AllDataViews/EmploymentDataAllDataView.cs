using Offer.Domain.AggregatesModel.ApplicationAggregate;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Offer.Domain.View.AllDataViews
{
    public class EmploymentDataAllDataView
    {
        public int TotalWorkPeriod
        {
            get
            {
                if (Employments == null || Employments.Count == 0)
                {
                    return 0;
                }

                var sumOfDays = Employments.Sum(e => ((e.EmploymentEndDate ?? DateTime.Now) - e.EmploymentStartDate).TotalDays);
                var months = Convert.ToInt32(Math.Round(sumOfDays / (365.25 / 12), 0));
                return months;
            }
        }
        public string EmploymentStatus { get; set; }
        public DateTime? EmploymentStatusDate { get; set; }
        public string PreviousWorkPeriod { get; set; }
        public List<EmploymentInfo> Employments { get; set; }
        public int ContinousWorkPeriod
        {
            get
            {
                if (Employments == null || Employments.Count == 0)
                {
                    return 0;
                }

                var tempEmployments = Employments.OrderByDescending(e => e.EmploymentEndDate).ToList();
                var oldestEmploymentDate = tempEmployments[0].EmploymentStartDate;
                
                // Calculation supports only one currently active employment
                if (tempEmployments.Where(e => e.EmploymentEndDate.HasValue).Count() > 1)
                {
                    throw new ArgumentException("More than one employment is active.");
                }
                for (var i = 1; i < tempEmployments.Count; i++)
                {
                    if (tempEmployments[i].EmploymentEndDate == oldestEmploymentDate ||
                        (oldestEmploymentDate - tempEmployments[i].EmploymentEndDate.Value).TotalDays <= 31)
                    {
                        oldestEmploymentDate = tempEmployments[i].EmploymentEndDate.Value;
                    }
                    else
                    {
                        break;
                    }
                }
                var timeSpan = ((tempEmployments[0].EmploymentEndDate ?? DateTime.Now) - oldestEmploymentDate);
                var months = Convert.ToInt32(Math.Round(timeSpan.TotalDays / (365.25 / 12), 0));
                return months;
            }
        }
        public int EmploymentCount
        {
            get
            {
                return Employments == null ? 0 : Employments.Count;
            }
        }
    }
}
