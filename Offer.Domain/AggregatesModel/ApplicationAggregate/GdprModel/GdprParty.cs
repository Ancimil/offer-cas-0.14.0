using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Offer.Domain.AggregatesModel.ApplicationAggregate
{

    public class GdprApplicationDocument
    {
        public string DocumentType { get; set; }
        public string DocumentName { get; set; }
        public DocumentStatus Status { get; set; }
    }

    public class GdprQuestionnaire
    {
        public string QuestionnaireId { get; set; }
        public string Purpose { get; set; }
        public DateTime Date { get; set; }
        public string QuestionnaireName { get; set; }
        public Dictionary<string, object> Entries { get; set; }
    }

    public class GdprParty
    {
        // Application Related
        [JsonIgnore]
        public long ApplicationId { get; set; }
        
        public string ApplicationNumber
        {
            get
            {
                var result = "0000000000" + ApplicationId;
                return result.Substring(result.Length - 10);
            }
        }
        public ApplicationStatus ApplicationStatus { get; set; }

        // Party Related
        public string CustomerName { get; set; }
        public string Username { get; set; }
        public PostalAddress LegalAddress { get; set; }
        public PostalAddress ContactAddress { get; set; }
        public string IdentificationNumber { get; set; }
        public IdentificationDocument IdentificationDocument { get; set; }
        public List<GdprApplicationDocument> Documents { get; set; }
        [JsonIgnore]
        public PartyRole PartyRole { get; set; }
        public List<GdprQuestionnaire> Questionnaires { get; set; }
        // Individual Related
        public string GivenName { get; set; }
        public string ParentName { get; set; }
        public string Surname { get; set; }
        public string MaidenName { get; set; }
        public string MothersMaidenName { get; set; }
        public IndividualLifecycleStatus LifecycleStatus { get; set; }
        public string EmailAddress { get; set; }
        public string MobilePhone { get; set; }
        public string HomePhoneNumber { get; set; }
        public Gender Gender { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string PlaceOfBirth { get; set; }
        public string ResidentialStatus { get; set; }
        public string CountryOfResidence { get; set; }
        public DateTime? ResidentialStatusDate { get; set; }
        public DateTime? ResidentialAddressDate { get; set; }
        public MaritalStatus? MaritalStatus { get; set; }
        public EducationLevel? EducationLevel { get; set; }
        public HomeOwnership? HomeOwnership { get; set; }
        public CarOwnership? CarOwnership { get; set; }
        public EmploymentData EmploymentInfo { get; set; }
        public string Occupation { get; set; }
        public string PreviousWorkPeriod { get; set; }
        public Household HouseholdInfo { get; set; }
        public String PreferredCulture { get; set; }
        public decimal? MatchingPercentage { get; set; }
    }
}
