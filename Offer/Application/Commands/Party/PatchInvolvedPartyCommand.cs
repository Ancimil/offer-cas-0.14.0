using MediatR;
using MicroserviceCommon.Models;
using Offer.Domain.AggregatesModel.ApplicationAggregate;
using Offer.Domain.AggregatesModel.ApplicationAggregate.ApplicantModel;
using Offer.Domain.AggregatesModel.CreditBureauModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Offer.API.Application.Commands
{
    public class PatchInvolvedPartyCommand : IRequest<CommandStatus>
    {
        public long ApplicationNumber { get; set; }
        public int PartyId { get; set; }
        public string Username { get; set; }
        public string CustomerNumber { get; set; }
        public string CustomerName { get; set; }
        public string EmailAddress { get; set; }
        public PartyRole? PartyRole { get; set; }
        public PartyKind? PartyKind { get; set; }
        public PostalAddress LegalAddress { get; set; }
        public PostalAddress ContactAddress { get; set; }
        public string PrimarySegment { get; set; }
        public string CustomerSegment { get; set; }
        public string CreditRating { get; set; }
        public decimal? CustomerValue { get; set; }
        public String PreferredCulture { get; set; }
        public string CountryOfResidence { get; set; }
        public ProductUsage ProductUsageInfo { get; set; }
        public IdentificationKind IdentificationNumberKind { get; set; }
        public string IdentificationNumber { get; set; }
        public IdentificationDocument IdentificationDocument { get; set; }
        public string ProfileImageUrl { get; set; }

        // Individual Party
        public string GivenName { get; set; }
        public string ParentName { get; set; }
        public string Surname { get; set; }
        public string MaidenName { get; set; }
        public string MothersMaidenName { get; set; }
        public IndividualLifecycleStatus? LifecycleStatus { get; set; }
        public string MobilePhone { get; set; }
        public string HomePhoneNumber { get; set; }
        public List<Relationship> Relationships { get; set; }
        public Gender? Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string PlaceOfBirth { get; set; }
        public string ResidentialStatus { get; set; }
        public DateTime? ResidentialStatusDate { get; set; }
        public DateTime? ResidentialAddressDate { get; set; }
        public MaritalStatus? MaritalStatus { get; set; }
        public string EducationLevel { get; set; }
        public HomeOwnership? HomeOwnership { get; set; }
        public CarOwnership? CarOwnership { get; set; }
        public string Occupation { get; set; }
        public EmploymentData EmploymentData { get; set; }
        public FinancialProfile FinancialProfile { get; set; }
        public Household HouseholdInfo { get; set; }
        // Organization party
        public string RegisteredName { get; set; }
        public string CommercialName { get; set; }
        public string LegalStructure { get; set; }
        public string OrganizationPurpose { get; set; }
        public DateTime? Established { get; set; }
        public string IndustrySector { get; set; }
        public Ownership OwnershipInfo { get; set; }
        public List<BankAccount> BankAccounts { get; set; }
        public List<IdNumber> IdNumbers { get; set; }
        public string Size { get; set; }
        public string FileKind { get; set; }
        public CreditBureauData CreditBureauData { get; set; }
        public bool? IsSoleTrader { get; set; }
        public string Phone { get; set; }  
        public string LegalBasisForRegistration { get; set; }
        public string LegalStatus { get; set; }
        public string DocumentationStatus { get; set; }
        public string AccountingMethod { get; set; }
        public List<FinancialStatement> FinancialStatements { get; set; }
        public long? SuppliersBuyersReportId { get; set; }
        public decimal? DebtToIncome { get; set; }
        public decimal? RemainingAbilityToPay { get; set; }
        public bool? IsRegisteredAsCustomer { get; set; }
        public bool? IsRegisteredAsProspect { get; set; }
        public bool? EmailVerfied { get; set; }
        public bool? PartyDataLoaded { get; set; }
        public bool? ProfileDataLoaded { get; set; }
        public string ProspectNumber { get; set; }
        public bool? HasRepresentative { get; set; }
        public bool? IdDataVerified { get; set; }
        public bool? IncomeVerified { get; set; }
        public bool? EmploymentDataVerified { get; set; }
        public bool? HouseholdInformationVerified { get; set; }
        public bool? KycQuestionnaireFilled { get; set; }
        public string ConsentsGiven { get; set; }
    }
}
