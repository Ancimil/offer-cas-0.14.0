using MicroserviceCommon.Domain.SeedWork;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Offer.Domain.AggregatesModel.ExposureModel;
using MicroserviceCommon.Models;
using Offer.Domain.AggregatesModel.ApplicationAggregate.Reporting;
using Offer.Domain.View;
using Newtonsoft.Json.Linq;
using Offer.Domain.View.AllDataViews;

namespace Offer.Domain.AggregatesModel.ApplicationAggregate
{
    public interface IApplicationRepository : IRepository<Application>
    {

        Application Add(Application application);


        Task<ApplicationDetailsView> GetAsyncDetailsView(long applicationNumber, string include = null, string trim = null);
        Task<ApplicationAllDataView> GetApplicationWithAllData(long applicationNumber, string username, bool auditLog);
        Task<Application> GetAsync(long applicationNumber, string include = null, string trim = null);
        Task<Application> GetAsyncTracked(long applicationNumber, string include = null, string trim = null);
        Application GetSync(long applicationNumber, string include = null, string trim = null);
        Application UpdateStatus(long applicationNumber, ApplicationStatus? status, StatusInformation statusInformation, string phase);
        Task<List<Party>> GetInvolvedParties(long applicationNumber);
        void UpdateCustomer(long applicationNumber, string idNumber, string idAuthority, DateTime idValidFrom, DateTime idValidTo,string contentUrls,
                                  string countryResident, string cityResident, string postalCodeResident, string streetNameResident, string streetNumberResident,
                                  string countryCorrespondent, string cityCorrespondent, string postalCodeCorrespondent, string streetNameCorrespondent, string streetNumberCorrespondent,
                                  bool accountOwner, bool relatedCustomers, bool politicallyExposedPerson, bool influenceGroup, bool bankAffiliated, bool isAmericanCitizen,
                                  string identificationNumber, Gender gender, DateTime dateOfBirth);
        Task<ApplicationList> GetApplicationsList(List<ApplicationStatus> statusList, List<ArrangementKind?> kindList, string productCode,
                                string customerData, string statusFromDate, string applicationDateFrom, string applicationDateTo, string include,
                                List<string> trim, int? page, int? pageSize, string sortBy, string sortOrder,
                                string partialApplicationNumber, string customerNumber, List<string> partyRoles, List<string> channels, string expirationDateFrom, string expirationDateTo, string initiator);
        Task<PagedApplicationList> GetApplications(List<ApplicationStatus> statusList, List<ArrangementKind?> kindList, string productCode,
                                        string customerData, string statusFromDate, string applicationDateFrom, string applicationDateTo, string include,
                                        List<string> trim, int? page, int? pageSize, string sortBy, string sortOrder,
                                        string partialApplicationNumber, string customerNumber, List<string> partyRoles, List<string> channels, string expirationDateFrom, string expirationDateTo, string initiator);
        List<ApplicationsByStatus> GetApplicationsByStatus();
        List<Application> CheckExistingOffersForProspect(string username, string email, List<ApplicationStatus> statusList, List<PartyRole> rolesList);
        List<Application> CheckExistingOffersForCustomer(string customerNumber, List<ApplicationStatus> statusList, List<PartyRole> rolesList);
        List<Application> GetProspectOffers(string username, List<ApplicationStatus> statusList);
        string ExportGdprData(PartyMatcher matcher);
        string AnonymizeGdprData(PartyMatcher matcher, bool fake = false);

        Currency CalculateExposureInTargetCurrency(ExposureList exposureList);

        void Update(Application application);
        bool IsMainProduct(long applicationNumber, int request);
        ApplicationDocument CreateApplicationDocument(long applicationNumber, ApplicationDocument document);

        IDictionary<string, object> GetCommercialDetails(long applicationNumber);
        Task<IDictionary<string, IDictionary<string, JToken>>> GetExtendedData(long applicationNumber);
        Task<Application> GetApplicationByLeadId(long? leadId);
        Task<IDictionary<string, JToken>> GetExtendedDataSection(long applicationNumber, string sectionName);
        Task<bool?> DeleteExtendedDataSection(long applicationNumber, string sectionName);
    }
}
