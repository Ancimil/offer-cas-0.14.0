using Newtonsoft.Json.Linq;
using Offer.Domain.AggregatesModel.ApplicationAggregate;
using System;
using System.Collections.Generic;

namespace Offer.Domain.View.AllDataViews
{
    public class ApplicationAllDataView
    {
        // Root
        public string ApplicationNumber { get; set; }
        public string CampaignCode { get; set; }
        public string CancelationComment { get; set; }
        public string CancelationReason { get; set; }
        public string Channel { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedByName { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public decimal? DebtToIncome { get; set; }
        public string Initiator { get; set; }
        public List<PartyAllDataView> InvolvedParties { get; set; }
        public int InvolvedPartyCount
        {
            get
            {
                return InvolvedParties?.Count ?? 0;
            }
        }
        public DateTime? LastModified { get; set; }
        public string OrganizationUnit { get; set; }
        public string Phase { get; set; }
        public DateTime? RequestDate { get; set; }
        public string SigningOption { get; set; }
        public ApplicationStatus Status { get; set; }
        public DateTime? StatusChangeDate { get; set; }
        public StatusInformation StatusInformation { get; set; }

        // CustomerDataAtRoot
        public string CreditRating { get; set; }
        public string CountryCode { get; set; }
        public string CustomerName { get; set; }
        public string CustomerNumber { get; set; }
        public string CustomerSegment { get; set; }
        public decimal? CustomerValue { get; set; }
        public string PrefferedCulture { get; set; }

        // RequestDataAtRoot
        public bool AmountLimitBreached { get; set; }
        public bool? OriginatesBundle { get; set; }
        public string PreApprovalType { get; set; }
        public bool PreferencialPrice { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public bool TermLimitBreached { get; set; }

        public List<ArrangementRequestAllDataView> ArrangementRequests { get; set; }

        public IDictionary<string, IDictionary<string, JToken>> Extended { get; set; }

        // UnmappedAtRoot
        /*public string PortfolioId { get; set; }
        public long? LeadId { get; set; }
        public string DecisionNumber { get; set; }
        public decimal? RiskScore { get; set; }
        public decimal? LoanToValue { get; set; }
        public decimal? MaximalAnnuity { get; set; }
        public decimal? MaximalAmount { get; set; }
        public decimal? DebtToIncome { get; set; }
        public decimal? CustomerRemainingAbilityToPay { get; set; }
        public decimal? EffectiveRemainingAbilityToPay { get; set; }
        public ExposureInfo ExposureInfo { get; set; }
        
        public List<Questionnaire> Questionnaires { get; set; }
        public List<ApplicationDocument> Documents { get; set; }*/
    }
}
