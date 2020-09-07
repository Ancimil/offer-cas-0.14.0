using MediatR;
using MicroserviceCommon.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Offer.API.Application.Commands
{
    public class RetrieveCreditBureauExposureCommand : IRequest<CommandStatus>
    {
        public long ApplicationNumber { get; set; }
        public List<PostCreditBureauExposure> CreditBureauExposures { get; set; }
    }

    public class PostCreditBureauExposure
    {
        [Required]
        public string PartyId { get; set; }
        public string Currency { get; set; }
        public decimal? ExposureInSourceCurrency { get; set; }
        public decimal? ExposureDebtInSourceCurrency { get; set; }
        public decimal? AnnuityInSourceCurrency { get; set; }
        public string Kind { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string RiskCategory { get; set; }
    }
}
