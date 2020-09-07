using System;
using Offer.Domain.AggregatesModel.ApplicationAggregate;
using MediatR;

namespace Offer.API.Application.Commands
{
    public class UpdateCustomerCommand : IRequest<bool>
    {
        public long ApplicationNumber { get; set; }
        public string IdNumber { get; private set; }
        public string IdAuthority { get; private set; }
        public DateTime IdValidFrom { get; private set; }
        public DateTime IdValidTo { get; private set; }
        public string ContentUrls { get; private set; }
        public string CountryResident { get; private set; }
        public string CityResident { get; private set; }
        public string CountyResident { get; private set; }
        public string PostalCodeResident { get; private set; }
        public string StreetNameResident { get; private set; }
        public string StreetNumberResident { get; private set; }
        public string CountryCorrespondent { get; private set; }
        public string CityCorrespondent { get; private set; }
        public string CountyCorrespondent { get; private set; }
        public string PostalCodeCorrespondent { get; private set; }
        public string StreetNameCorrespondent { get; private set; }
        public string StreetNumberCorrespondent { get; private set; }
        public bool AccountOwner { get; private set; }
        public bool RelatedCustomers { get; private set; }
        public bool PoliticallyExposedPerson { get; private set; }
        public bool InfluenceGroup { get; private set; }
        public bool BankAffiliated { get; private set; }
        public bool IsAmericanCitizen { get; private set; }
        public string IdentificationNumber { get; private set; }
        public Gender Gender { get; private set; }
        public DateTime DateOfBirth { get; private set; }


        public UpdateCustomerCommand(long applicationNumber,
                                  string idNumber, string idAuthority, DateTime idValidFrom, DateTime idValidTo, string contentUrls,
                                  string countryResident, string cityResident, string postalCodeResident, string streetNameResident, string streetNumberResident,
                                  string countryCorrespondent, string cityCorrespondent, string postalCodeCorrespondent, string streetNameCorrespondent, string streetNumberCorrespondent,
                                  bool accountOwner, bool relatedCustomers, bool politicallyExposedPerson, bool influenceGroup, bool bankAffiliated, bool isAmericanCitizen,
                                  string identificationNumber, Gender gender, DateTime dateOfBirth)
        {
            ApplicationNumber = applicationNumber;
            IdNumber = idNumber;
            IdAuthority = idAuthority;
            IdValidFrom = idValidFrom;
            IdValidTo = idValidTo;
            ContentUrls = contentUrls;
            CountryResident = countryResident;
            CityResident = cityResident;
            PostalCodeResident = postalCodeResident;
            StreetNameResident = streetNameResident;
            StreetNumberResident = streetNumberResident;
            CountryCorrespondent = countryCorrespondent;
            CityCorrespondent = cityCorrespondent;
            PostalCodeCorrespondent = postalCodeCorrespondent;
            StreetNameCorrespondent = streetNameCorrespondent;
            StreetNumberCorrespondent = streetNumberCorrespondent;
            AccountOwner = accountOwner;
            RelatedCustomers = relatedCustomers;
            PoliticallyExposedPerson = politicallyExposedPerson;
            InfluenceGroup = influenceGroup;
            BankAffiliated = bankAffiliated;
            IsAmericanCitizen = isAmericanCitizen;
            IdentificationNumber = identificationNumber;
            Gender = gender;
            DateOfBirth = dateOfBirth;
        }
    }
}
