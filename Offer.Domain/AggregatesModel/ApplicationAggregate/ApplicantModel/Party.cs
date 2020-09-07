using JsonSubTypes;
using MicroserviceCommon.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Offer.Domain.AggregatesModel.ApplicationAggregate.ApplicantModel;
using Offer.Domain.AggregatesModel.CreditBureauModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using Enumeration = MicroserviceCommon.Contracts.Enumeration;

namespace Offer.Domain.AggregatesModel.ApplicationAggregate
{

    [Enumeration("identification-kind", "Identification Kind", "Identification Kind")]
    public enum IdentificationKind
    {

        [EnumMember(Value = "registration-number")]
        [Description("Registration Number")]
        RegistrationNumber,
        [EnumMember(Value = "tax-id-number")]
        [Description("Tax Id Number")]
        TaxIdNumber,
        [EnumMember(Value = "personal-id-number")]
        [Description("Personal Id Number")]
        PersonalIdNumber,
        [EnumMember(Value = "identity-card-number")]
        [Description("Identity Card Number")]
        IdentityCardNumber,
        [EnumMember(Value = "passport-number")]
        [Description("Passport Number")]
        PassportNumber,
        [EnumMember(Value = "driver-license-number")]
        [Description("Driver License Number")]
        DriverLicenseNumber,
        [EnumMember(Value = "social-security-number")]
        [Description("Social Security Number")]
        SocialSecurityNumber,
    }

    [Enumeration("party-role", "Party Role", "Enumeration that distinguishes between role kinds of the party related to the arrangement.")]
    public enum PartyRole
    {
        [EnumMember(Value = "customer")]
        [Description("Customer")]
        Customer,

        [EnumMember(Value = "co-debtor")]
        [Description("Co-debtor")]
        CoDebtor,

        [EnumMember(Value = "customer-representative")]
        [Description("Customer Representative")]
        CustomerRepresentative,

        [EnumMember(Value = "guarantor")]
        [Description("Guarantor")]
        Guarantor,

        [EnumMember(Value = "authorized-person")]
        [Description("Authorized Person")]
        AuthorizedPerson,

        [EnumMember(Value = "other")]
        [Description("Other")]
        Other
    }

    [Enumeration("party-kind", "Party Kind", "Enumeration that distinguishes between party kinds.")]
    public enum PartyKind
    {
        [EnumMember(Value = "individual")]
        Individual,
        [EnumMember(Value = "organization")]
        Organization
    }

    [JsonConverter(typeof(JsonSubtypes), "party-kind")]
    [JsonSubtypes.KnownSubType(typeof(IndividualParty), "individual")]
    [JsonSubtypes.KnownSubType(typeof(OrganizationParty), "organization")]
    public abstract class Party
    {
        public long ApplicationId { get; set; } // references parent app number, part of key
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long PartyId { get; set; } // key in combination with parent app number

        [NotMapped]
        public string ApplicationNumber
        {
            get
            {
                var result = "0000000000" + ApplicationId;
                return result.Substring(result.Length - 10);
            }
        }

        [JsonIgnore]
        [ForeignKey("ApplicationId")]
        public Application Application { get; set; }
        [MaxLength(200)]
        public string Username { get; set; }
        [MaxLength(200)]
        public string CustomerNumber { get; set; }
        [MaxLength(256)]
        public string CustomerName { get; set; }
        [MaxLength(256)]
        public string EmailAddress { get; set; }
        [Required]
        public PartyRole PartyRole { get; set; } // Customer, Customer Representative, CoDebtor, Guarantor, etc.
        [Required]
        public PartyKind PartyKind { get; set; }
        //[JsonProperty(PropertyName = "LegalAddress")]
        public PostalAddress LegalAddress { get; set; }
        //[JsonProperty(PropertyName = "ContactAddress")]
        public PostalAddress ContactAddress { get; set; }

        [MaxLength(100)]
        public string PrimarySegment { get; set; }
        [MaxLength(100)]
        public string CustomerSegment { get; set; }
        public string CreditRating { get; set; }
        public decimal? CustomerValue { get; set; }

        [NotMapped]
        public String PreferredCulture { get; set; }
        public string CountryOfResidence { get; set; }

        //public List<Questionnaire> Questionnaires { get; set; }
        [JsonIgnore]
        public string _ProductUsageInfo { get; set; }
        [NotMapped]
        public ProductUsage ProductUsageInfo
        {
            get { return _ProductUsageInfo == null ? null : JsonConvert.DeserializeObject<ProductUsage>(_ProductUsageInfo); }
            set
            {
                _ProductUsageInfo = JsonConvert.SerializeObject(value);
            }
        } // only for existing customers
        [JsonIgnore]
        public string _Extended
        {
            get
            {
                return Extended == null ? null : JsonConvert.SerializeObject(Extended);
            }
            set
            {
                Extended = value == null ? null : JsonConvert.DeserializeObject<IDictionary<string, IDictionary<string, JToken>>>(value);
            }

        }
        [NotMapped]
        public IDictionary<string, IDictionary<string, JToken>> Extended { get; set; }
        [NotMapped]
        public string OrganizationUnitCode { get; set; }

        public IdentificationKind IdentificationNumberKind { get; set; } // personal-identification-number, passport-number, id-number, social-security-number...
        [MaxLength(64)]
        public string IdentificationNumber { get; set; }
        [JsonIgnore]
        public string _IdentificationDocument { get; set; }


        [NotMapped]
        public IdentificationDocument IdentificationDocument
        {
            get { return _IdentificationDocument == null ? null : JsonConvert.DeserializeObject<IdentificationDocument>(_IdentificationDocument); }
            set { _IdentificationDocument = value == null ? null : JsonConvert.SerializeObject(value); }
        }
        [NotMapped]
        public bool IdDataComplete {
            get
            {
                return !string.IsNullOrEmpty(_IdentificationDocument);
            }
        }
        public string ProfileImageUrl { get; set; }

        public decimal? DebtToIncome { get; set; }
        public decimal? RemainingAbilityToPay { get; set; }

        [JsonIgnore]
        public string _CbData { get; set; }

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

    public class IndividualParty : Party
    {
        // Identification
        [MaxLength(256)]
        public string GivenName { get; set; } // First Name
        [MaxLength(256)]
        public string ParentName { get; set; } // non-mandatory
        [MaxLength(256)]
        public string Surname { get; set; } // Last Name
        [MaxLength(256)]
        public string MaidenName { get; set; } // non-mandatory
        [MaxLength(256)]
        public string MothersMaidenName { get; set; } // Security question, non-mandatory

        public IndividualLifecycleStatus LifecycleStatus { get; set; }

        // Contact
        //[MaxLength(256)]
        //public string EmailAddress { get; set; }
        [MaxLength(256)]
        public string MobilePhone { get; set; }
        [MaxLength(256)]
        public string HomePhoneNumber { get; set; }

        [JsonIgnore]
        [Column("Relationships")]
        public string _Relationships { get; set; }
        [NotMapped]
        public List<Relationship> Relationships
        {
            get { return _Relationships == null ? null : JsonConvert.DeserializeObject<List<Relationship>>(_Relationships); }
            set { _Relationships = JsonConvert.SerializeObject(value); }
        }

        // Questions
        public Gender Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        [MaxLength(256)]
        public string PlaceOfBirth { get; set; }

        [MaxLength(256)]
        public string ResidentialStatus { get; set; }
        [MaxLength(256)]

        public DateTime? ResidentialStatusDate { get; set; }
        public DateTime? ResidentialAddressDate { get; set; }

        public MaritalStatus? MaritalStatus { get; set; }
       // public EducationLevel? EducationLevel { get; set; }
        public string EducationLevel { get; set; }
        public HomeOwnership? HomeOwnership { get; set; }
        public CarOwnership? CarOwnership { get; set; }

        [MaxLength(256)]
        public string Occupation { get; set; }

        [JsonIgnore]
        [Column("EmploymentData")]
        public string _EmploymentData { get; set; }

        [NotMapped]
        public EmploymentData EmploymentData
        {
            get { return _EmploymentData == null ? null : JsonConvert.DeserializeObject<EmploymentData>(_EmploymentData); }
            //get
            //{
            //    var data = _EmploymentData == null ? null : JsonConvert.DeserializeObject<EmploymentData>(_EmploymentData);
            //    if (data != null && data.Employments != null && data.Employments.Count > 0)
            //    {
            //        //var employmentDays = data.Employments.Select(e => Utility.DaysBetween(e.EmploymentStartDate, e.EmploymentEndDate)).Sum();
            //        //var periodBuilder = new PeriodBuilder();
            //        //var durationStruct = periodBuilder.ToString(new TimeSpan(employmentDays * 24, 0, 0));
            //        // data.TotalWorkPeriod = durationStruct;
            //    }
            //    return data;
            //}
            set { _EmploymentData = value == null ? null : JsonConvert.SerializeObject(value); }
        }
        [NotMapped]
        public bool EmploymentDataStated { 
            get 
            {
                return !string.IsNullOrEmpty(_EmploymentData);
            } 
        }

        [JsonIgnore]
        [Column("FinancialProfile")]
        public string _FinancialProfile { get; set; }

        [NotMapped]
        public FinancialProfile FinancialProfile
        {
            get { return _FinancialProfile == null ? null : JsonConvert.DeserializeObject<FinancialProfile>(_FinancialProfile); }
            set { _FinancialProfile = JsonConvert.SerializeObject(value); }
        }
        [NotMapped]
        public bool IncomeStated {
            get {
                return !string.IsNullOrEmpty(_FinancialProfile) &&
                  (FinancialProfile?.IncomeInfo != null && FinancialProfile.IncomeInfo.Count > 0) ? true : false;
            }
        }

        [JsonIgnore]
        [Column("HouseholdInfo")]
        public string _HouseholdInfo { get; set; }
        [NotMapped]
        public Household HouseholdInfo
        {
            get { return _HouseholdInfo == null ? null : JsonConvert.DeserializeObject<Household>(_HouseholdInfo); }
            set { _HouseholdInfo = value == null ? null : JsonConvert.SerializeObject(value); }
        }
        [NotMapped]
        public bool HouseholdInformationStated { 
            get {
                return !string.IsNullOrEmpty(_HouseholdInfo);
            }
        }

        public IndividualParty()
        {
            this.ContactAddress = new PostalAddress
            {
                Coordinates = new Coordinates { }
            };
            this.LegalAddress = new PostalAddress
            {
                Coordinates = new Coordinates { }
            };
        }

        [NotMapped]
        public CreditBureauData CreditBureauData
        {
            get { return _CbData == null ? null : JsonConvert.DeserializeObject<CreditBureauData>(_CbData); }
            set
            {
                _CbData = JsonConvert.SerializeObject(value);
            }
        }
    }

    [ComplexType]
    public class Household
    {
        public Currency TotalHouseholdIncome { get; set; }
        public Int32 SizeOfHousehold { get; set; }
        public Int32 EmployedHousholdMembers { get; set; }
        public Int32 DependentChildren { get; set; }
        public Int32 DependentAdults { get; set; }
    }

    [ComplexType]
    public class IdentificationDocument
    {
        public string Kind { get; set; }
        public string SerialNumber { get; set; }
        public DateTime? IssuedDate { get; set; }
        public DateTime? ValidUntil { get; set; }
        public string Status { get; set; }
        public string PlaceOfIssue { get; set; }
        public string IssuingAuthority { get; set; }
        public string ContentUrls { get; set; }
    }

    public class OrganizationParty : Party
    {
        [MaxLength(256)]
        public string RegisteredName { get; set; }
        public string CommercialName { get; set; }
        public string LegalStructure { get; set; }
        public string OrganizationPurpose { get; set; }
        public bool IsSoleTrader { get; set; }
        public DateTime Established { get; set; }
        public string IndustrySector { get; set; }

        [JsonIgnore]
        [Column("OwnershipInfo")]
        public string _OwnershipInfo { get; set; }
        [NotMapped]
        public Ownership OwnershipInfo
        {
            get { return _OwnershipInfo == null ? null : JsonConvert.DeserializeObject<Ownership>(_OwnershipInfo); }
            set { _OwnershipInfo = JsonConvert.SerializeObject(value); }
        }

        [JsonIgnore]
        [Column("Relationships")]
        public string _Relationships { get; set; }
        [NotMapped]
        public List<Relationship> Relationships
        {
            get { return _Relationships == null ? null : JsonConvert.DeserializeObject<List<Relationship>>(_Relationships); }
            set { _Relationships = JsonConvert.SerializeObject(value); }
        }

        [JsonIgnore]
        [Column("BankAccounts")]
        public string _BankAccounts { get; set; }
        [NotMapped]
        public List<BankAccount> BankAccounts
        {
            get { return _BankAccounts == null ? null : JsonConvert.DeserializeObject<List<BankAccount>>(_BankAccounts); }
            set { _BankAccounts = JsonConvert.SerializeObject(value); }
        }
        [JsonIgnore]

        [Column("IdNumbers")]
        public string _IdNumbers { get; set; }
        [NotMapped]
        public List<IdNumber> IdNumbers
        {
            get { return _IdNumbers == null ? null : JsonConvert.DeserializeObject<List<IdNumber>>(_IdNumbers); }
            set { _IdNumbers = JsonConvert.SerializeObject(value); }
        }
        public string Size { get; set; }
        public string FileKind { get; set; }

        [NotMapped]
        public CreditBureauData CreditBureauData
        {
            get { return _CbData == null ? null : JsonConvert.DeserializeObject<CreditBureauData>(_CbData); }
            set
            {
                _CbData = JsonConvert.SerializeObject(value);
            }
        }
        public OrganizationParty()
        {
            this.ContactAddress = new PostalAddress
            {
                Coordinates = new Coordinates { }
            };
            this.LegalAddress = new PostalAddress
            {
                Coordinates = new Coordinates { }
            };
        }
        [MaxLength(256)]
        public string Phone { get; set; }
        public string LegalBasisForRegistration { get; set; }
        public string LegalStatus { get; set; }
        public string DocumentationStatus { get; set; }

        public string AccountingMethod { get; set; }

        [JsonIgnore]
        [Column("FinancialStatements")]
        public string _FinancialStatements { get; set; }
        [NotMapped]
        public List<FinancialStatement> FinancialStatements
        {
            get { return _FinancialStatements == null ? null : JsonConvert.DeserializeObject<List<FinancialStatement>>(_FinancialStatements); }
            set { _FinancialStatements = JsonConvert.SerializeObject(value); }
        }

        public long? SuppliersBuyersReportId { get; set; }

    }
    [ComplexType]
    public class Ownership
    {
        public string Kind { get; set; }
        public string ResidentalStatus { get; set; }
    }


}
