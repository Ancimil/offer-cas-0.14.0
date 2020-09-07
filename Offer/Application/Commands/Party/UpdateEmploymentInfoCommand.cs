using MediatR;
using Offer.Domain.AggregatesModel.ApplicationAggregate;
using System;
using System.Collections.Generic;

namespace Offer.API.Application.Commands
{
    public class UpdateEmploymentInfoCommand : IRequest<EmploymentData>
    {
        public long ApplicationNumber { get; set; }
        public int PartyId { get; set; }
        public string TotalWorkPeriod { get; set; }
        public string EmploymentStatus { get; set; }
        public DateTime? EmploymentStatusDate { get; set; }
        public List<EmploymentInfo> Employments { get; set; }

        public UpdateEmploymentInfoCommand(string totalWorkPeriod, List<EmploymentInfo> employments)
        {
            TotalWorkPeriod = totalWorkPeriod;
            Employments = employments;
        }
    }
}
