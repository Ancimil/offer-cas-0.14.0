using MediatR;
using MicroserviceCommon.Models;
using Newtonsoft.Json;
using Offer.Domain.AggregatesModel.ApplicationAggregate;
using System;
using System.ComponentModel.DataAnnotations;

namespace Offer.API.Application.Commands
{
    public class UpdateApplicationCommand : IRequest<CommandStatus>
    {
        public long ApplicationNumber { get; set; }

        [MaxLength(128)]
        [Required]
        public string ProductCode { get; set; } // Product which has been selected in product catalog. There might be other products implicitely open through onboarding (such as current account) and also there might be other products in the bundle.

        [MaxLength(128)]
        public string ProductName { get; set; }

        [MaxLength(256)]
        public string CustomerNumber { get; set; }

        [MaxLength(256)]
        public string CustomerName { get; set; }

        [MaxLength(256)]
        public string CustomerSegment { get; set; }

        [MaxLength(1024)]
        public string OrganizationUnitCode { get; set; }

        [MaxLength(128)]
        [Required]
        public string ChannelCode { get; set; } // Application creation channel

        [MaxLength(1024)]
        public string PortfolioId { get; set; }

        [MaxLength(128)]
        public string CampaignCode { get; set; } // Also promotional code

        [MaxLength(256)]
        public string DecisionNumber { get; set; }

        [MaxLength(256)]
        public string SettlementAccount { get; set; }

        [MaxLength(128)]
        public string ArrangementNumber { get; set; }

        [MaxLength(128)]
        public string Initiator { get; set; }

        [MaxLength(128)]
        public string CountryCode { get; set; }

        [MaxLength(128)]
        public string PrefferedCulture { get; set; }

        [MaxLength(256)]
        public string SigningOption { get; set; }

        [MaxLength(256)]
        public string CollateralModel { get; set; }

        public decimal? RiskScore { get; set; }

        public StatusInformation StatusInformation { get; set; }

        public DateTime? RequestedActivationDate { get; set; }

        public string Comments { get; set; }

        public bool TermLimitBreached { get; set; }

        public bool AmountLimitBreached { get; set; }

        public bool PreferencialPrice { get; set; }

        public bool AuditLog { get; set; }

        // update partija i aranzmana, a app da povlaci te vrednosti
        // racunati max amount na osnovu max annuity
    }
}
