using Offer.Domain.AggregatesModel.ApplicationAggregate;
using Offer.Infrastructure.View;
using System;
using System.Collections.Generic;

namespace Offer.Domain.View
{
    public class ApplicationDetailsView
    {
        public ApplicationDetailsView()
        {
            StatusInformation = new StatusInformation();
        }
        public string ApplicationNumber { get; set; }

        public ExposureInfo ExposureInfo { get; set; }
        public ApplicationStatus Status { get; set; }

        public string ProductCode { get; set; } // Product which has been selected in product catalog. There might be other products implicitely open through onboarding (such as current account) and also there might be other products in the bundle.

        public string ProductName { get; set; }

        public string CustomerNumber { get; set; }

        public string CustomerName { get; set; }

        public string CustomerSegment { get; set; }

        public string OrganizationUnitCode { get; set; }

        public bool PortfolioRequestExist { get; set; }

        public string ChannelCode { get; set; } // Application creation channel

        public string PortfolioId { get; set; }

        public string CampaignCode { get; set; } // Also promotional code

        public string DecisionNumber { get; set; }

        public string SettlementAccount { get; set; }

        public string ArrangementNumber { get; set; }

        public string Initiator { get; set; }

        public string CountryCode { get; set; }

        public string PrefferedCulture { get; set; }

        public string SigningOption { get; set; }

        public string CollateralModel { get; set; }

        public string ProductBundling { get; set; }

        public decimal? RiskScore { get; set; }
        public string CreditRating { get; set; }
        public decimal? CustomerValue { get; set; }

        public StatusInformation StatusInformation { get; set; }

        public DateTime? RequestDate { get; set; }

        public DateTime? ExpirationDate { get; set; }

        public string CancelationReason { get; set; }

        public string CancelationComment { get; set; }

        public DateTime? Created { get; set; }

        public DateTime? StatusChangeDate { get; set; }

        public DateTime? LastModified { get; set; }

        public string CreatedByName { get; set; }

        public DateTime? RequestedActivationDate { get; set; }

        public string Comments { get; set; }

        public List<ArrangementRequest> ArrangementRequests { get; set; }

        public List<Party> InvolvedParties { get; set; }

        public List<Questionnaire> Questionnaires { get; set; }

        public List<ApplicationDocumentView> Documents { get; set; }

        public bool TermLimitBreached { get; set; }

        public bool AmountLimitBreached { get; set; }

        public bool PreferencialPrice { get; set; }

        public decimal? LoanToValue { get; set; }
        public decimal? MaximalAnnuity { get; set; }
        public decimal? MaximalAmount { get; set; }
        public decimal? DebtToIncome { get; set; }
        public decimal? CustomerRemainingAbilityToPay { get; set; }
        public decimal? EffectiveRemainingAbilityToPay { get; set; }
        public bool IsPreApproved { get; set; } = false;
        public string PreApprovalType { get; set; }
        public bool? OriginatesBundle { get; set; } = false;
        public string Phase { get; set; }
        public bool? CustomerApplied { get; set; }
    }
}
