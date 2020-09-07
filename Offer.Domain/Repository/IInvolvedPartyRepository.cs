using Newtonsoft.Json.Linq;
using Offer.Domain.AggregatesModel.ApplicationAggregate;
using Offer.Domain.AggregatesModel.ApplicationAggregate.ApplicantModel;
using Offer.Domain.AggregatesModel.CreditBureauModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Offer.Domain.Repository
{
    public interface IInvolvedPartyRepository
    {
        void Update(Party party);
        Task<Party> GetParty(long applicationNumber, int partyId, string include = null, string trim = null);
        Task<Party> GetPartyGeneralInformation(long applicationNumber, int partyId, string include = null, string trim = null);
        Task<bool?> UpdatePartyGeneralInformation(long applicationNumber, int partyId, Party generalInformation);
        Task<EmploymentData> GetPartyEmploymentInfo(long applicationNumber, int partyId);
        Task<EmploymentData> UpdatePartyEmploymentInfo(long applicationNumber, int partyId, EmploymentData employmentData);
        Task<Household> GetPartyHouseholdInfo(long applicationNumber, int partyId);
        Task<List<Relationship>> GetPartyRelationshis(long applicationNumber, int partyId);
        Task<List<BankAccount>> GetPartyBankAccounts(long applicationNumber, int partyId);
        Task<Household> UpdatePartyHouseholdInfo(long applicationNumber, int partyId, Household household);
        Task<bool?> UpdatePartyCreditBureauData(long applicationNumber, int partyId, CreditBureauData creditBureauData, bool auditLog);
        Task<CreditBureauData> GetPartyCreditBureauData(long applicationNumber, int partyId);
        Task<FinancialData> GetPartyFinancialProfile(long applicationNumber, int partyId);
        Task<FinancialData> UpdatePartyFinancialProfile(long applicationNumber, int partyId, FinancialProfile financialProfile);
        Task<Party> AddParty(long applicationNumber, Party party, bool auditLog);
        Task<bool?> DeleteParty(long applicationNumber, int partyId, bool auditLog);
        ContactPoints GetPartyContactPoints(long applicationNumber, int partyId);
        Task<ContactPoints> UpdatePartyContactPoints(long applicationNumber, int partyId, ContactPoints contactPoints, bool auditLog);
        Task<List<FinancialStatement>> GetPartyFinancialStatements(long applicationNumber, int partyId);
        Task<bool?> SetSuppliersBuyersReportForParty(long applicationNumber, string customerNumber, long? reportId);
        void PublishFinancialStatementsChangeEvent(string applicationNumber, string customerNumber);
        Task<IDictionary<string, IDictionary<string, JToken>>> GetExtendedPartyData(long applicationNumber, int partyId);
        Task<IDictionary<string, JToken>> GetExtendedPartyDataSection(long applicationNumber, int partyId, string sectionName);
        Task<bool?> DeleteExtendedPartyDataSection(long applicationNumber, int partyId, string sectionName);
    }
}
