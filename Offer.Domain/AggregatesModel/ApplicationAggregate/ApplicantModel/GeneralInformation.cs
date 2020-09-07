using JsonSubTypes;
using Newtonsoft.Json;
using System;

namespace Offer.Domain.AggregatesModel.ApplicationAggregate.ApplicantModel
{
    [JsonConverter(typeof(JsonSubtypes), "party-kind")]
    [JsonSubtypes.KnownSubType(typeof(IndividualGeneralInformation), "individual")]
    [JsonSubtypes.KnownSubType(typeof(OrganizationGeneralInformation), "organization")]
    public abstract class GeneralInformation
    {
        public int PartyId { get; set; }
        public PartyKind PartyKind { get; set; }
        public string CustomerNumber { get; set; }
        public string CustomerName { get; set; }
        public PartyRole PartyRole { get; set; }
        public string ProfileImageUrl { get; set; }
        public IdentificationKind IdentificationNumberKind { get; set; }
        public string IdentificationNumber { get; set; }
    }

    public class OrganizationGeneralInformation : GeneralInformation
    {
        public string RegisteredName { get; set; }
        public string CommercialName { get; set; }
        public string LegalStructure { get; set; }
        public string OrganizationPurpose { get; set; }
        public string IsSoleTrader { get; set; }
        public string Established { get; set; }
        public string IndustrySector { get; set; }
        public string Size { get; set; }
    }

    public class IndividualGeneralInformation : GeneralInformation
    {
        public string GivenName { get; set; }
        public string ParentName { get; set; }
        public string Surname { get; set; }
        public string MaidenName { get; set; }
        public string MothersMaidenName { get; set; }

        public Gender Gender { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string PlaceOfBirth { get; set; }

        public MaritalStatus? MaritalStatus { get; set; }
        public string EducationLevel { get; set; }
        public string Occupation { get; set; }
    }
}
