using System;
using System.Collections.Generic;

namespace PriceCalculation.Models.LeadModel
{
    public class LeadModel
    {
        public long LeadId { get; set; }
        public string ExternalLeadId { get; set; }
        public string PartyId { get; set; }
        public string CampaignCode { get; set; }
        public string ProductCode { get; set; }
        public CommercialDetails InitialTerms { get; set; }
        public CommercialDetails ApprovedTerms { get; set; }
        public CommercialDetails ContractedTerms { get; set; }
        public LeadStatus LeadStatus { get; set; }
        public string OfferStatus { get; set; }
        public DateTime StatusChangeDate { get; set; }
        public string DisqualificationReason { get; set; }
        public string CancelationReason { get; set; }
        public string AssignedTo { get; set; }
        public string Description { get; set; }
        public bool PreApproved { get; set; } = false;
        public bool SkipCb { get; set; } = false;
        public bool SkipScoring { get; set; } = false;
        public bool SkipEligibility { get; set; } = false;
        public ICollection<BenefitModel> Benefits { get; set; }
        public DateTime ExpirationDate { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
