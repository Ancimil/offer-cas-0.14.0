using JsonSubTypes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Offer.Domain.AggregatesModel.ApplicationAggregate;
using Offer.Domain.AggregatesModel.ApplicationAggregate.ApplicantModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Offer.Domain.View.AllDataViews
{
    [JsonConverter(typeof(JsonSubtypes), "party-kind")]
    [JsonSubtypes.KnownSubType(typeof(IndividualPartyAllDataView), "individual")]
    [JsonSubtypes.KnownSubType(typeof(OrganizationPartyAllDataView), "organization")]
    public abstract class PartyAllDataView
    {
        // Common
        public PostalAddress ContactAddress { get; set; }
        public string CustomerName { get; set; }
        public string CustomerNumber { get; set; }
        public string EmailAddress { get; set; }
        public PostalAddress LegalAddress { get; set; }
        public long PartyId { get; set; }
        public PartyKind PartyKind { get; set; }
        public PartyRole PartyRole { get; set; }
        public string Username { get; set; }

        // CustomerInfo
        public string CountryOfResidence { get; set; }
        public string CreditRating { get; set; }
        public string CustomerSegment { get; set; }
        public decimal? CustomerValue { get; set; }
        public string OrganizationUnit { get; set; }
        public string PreferredCulture { get; set; }
        public string PrimarySegment { get; set; }

        // Identification
        public IdentificationDocument IdentificationDocument { get; set; }
        public string IdentificationNumber { get; set; }
        public IdentificationKind IdentificationNumberKind { get; set; }
        public string ProfileImageUrl { get; set; }

        // Relationships
        public List<Relationship> Relationships { get; set; }
        public int RelationshipCount
        {
            get
            {
                return Relationships?.Count ?? 0;
            }
        }

      
        // Extended
        public IDictionary<string, IDictionary<string, JToken>> Extended { get; set; }
        public ProductUsage ProductUsageInfo { get; set; }

        // Exposure
        public ExposureInfo Exposure { get; set; }
        public decimal? DebtToIncome { get; set; }
        // Unmapped
        /*
        public decimal? RemainingAbilityToPay { get; set; }*/

    }

    public class IndividualPartyAllDataView : PartyAllDataView
    {
        // AsIndividual
        public AsIndividualPartyView AsIndividual { get; set; }

    }

    public class OrganizationPartyAllDataView : PartyAllDataView
    {
        // As Organization

        public AsOrganizationPartyView AsOrganization { get; set; }

        // Unmapped
        /*public string Size { get; set; }
        public string FileKind { get; set; }*/
    }

    public class AsIndividualPartyView
    {
        public CarOwnership? CarOwnership { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string EducationLevel { get; set; }
        public string GivenName { get; set; }
        public Gender Gender { get; set; }
        public string HomePhoneNumber { get; set; }
        public HomeOwnership? HomeOwnership { get; set; }
        public IndividualLifecycleStatus LifecycleStatus { get; set; }
        public string MaidenName { get; set; }
        public MaritalStatus? MaritalStatus { get; set; }
        public string MobilePhone { get; set; }
        public string MothersMaidenName { get; set; }
        public string Occupation { get; set; }
        public string ParentName { get; set; }
        public string PlaceOfBirth { get; set; }
        public DateTime? ResidentialAddressDate { get; set; }
        public string ResidentialStatus { get; set; }
        public DateTime? ResidentialStatusDate { get; set; }
        public string Surname { get; set; }
        public int Age
        {
            get
            {
                if (!DateOfBirth.HasValue)
                {
                    return 0;
                }

                var daysDouble = (DateTime.Now - DateOfBirth.Value).TotalDays;
                var years = Convert.ToInt32(Math.Round(daysDouble / 365.25, 0));
                return years;
            }
        }
        public int AgeInMonths
        {
            get
            {
                if (!DateOfBirth.HasValue)
                {
                    return 0;
                }
                var days = (DateTime.Now - DateOfBirth.Value).TotalDays;
                var months = Convert.ToInt32(Math.Round(days / (365.25 / 12), 0));
                return months;
            }
        }
        // Employment
        public EmploymentDataAllDataView EmploymentData { get; set; }
        public CurrentEmployment CurrentEmployment
        {
            get
            {
                if (EmploymentData == null || EmploymentData.Employments == null || EmploymentData.Employments.Count == 0)
                {
                    return null;
                }

                var current = EmploymentData.Employments.OrderByDescending(e => e.EmploymentEndDate).Last();
                return new CurrentEmployment
                {
                    CompanyIdNumber = current.CompanyIdNumber,
                    EmploymentKind = current.EmploymentKind,
                    EmploymentStartDate = current.EmploymentStartDate
                };
            }
        }

        // FinancialProfile
        public FinancialData FinancialProfile { get; set; }

        // Household
        public Household HouseholdInfo { get; set; }
    }

    public class AsOrganizationPartyView
    {
        public string AccountingMethod { get; set; }
        public List<BankAccount> BankAccounts { get; set; }
        public string CommercialName { get; set; }
        public string DocumentationStatus { get; set; }
        public DateTime Established { get; set; }
        public List<FinancialStatement> FinancialStatements { get; set; }
        public List<IdNumber> IdNumbers { get; set; }
        public string IndustrySector { get; set; }
        public bool IsSoleTrader { get; set; }
        public string LegalBasisForRegistration { get; set; }
        public string LegalStatus { get; set; }
        public string LegalStructure { get; set; }
        public string OrganizationPurpose { get; set; }
        public Ownership OwnershipInfo { get; set; }
        public string Phone { get; set; }
        public string RegisteredName { get; set; }
        public long? SuppliersBuyersReportId { get; set; }
    }

}
