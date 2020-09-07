using Microsoft.EntityFrameworkCore;
using Offer.Domain.AggregatesModel.ApplicationAggregate;
using Offer.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MicroserviceCommon.Domain.SeedWork;
using System.Security.Cryptography;
using System.Text;
using AutoMapper;
using Offer.Infrastructure.Utils;
using MicroserviceCommon.API.ApiUtils;
using Offer.Domain.AggregatesModel.ExposureModel;
using AssecoCurrencyConvertion;
using System.Globalization;
using MicroserviceCommon.Services;
using MicroserviceCommon.Models;
using Offer.Domain.AggregatesModel.ApplicationAggregate.Reporting;
using Microsoft.Extensions.Logging;
using Offer.Domain.View;
using Newtonsoft.Json.Linq;
using Offer.Domain.View.AllDataViews;
using Microsoft.AspNetCore.Http;
using MicroserviceCommon.Extensions.Http;
using Offer.Infrastructure.View;
using Offer.Infrastructure.Mappings;
using Microsoft.EntityFrameworkCore.Storage;
using AuditClient;
using AuditClient.Model;
using Newtonsoft.Json;

namespace Offer.Infrastructure.Repositories
{
    public class ApplicationRepository : IApplicationRepository
    {
        private readonly OfferDBContext _context;
        private readonly IConfigurationService _configurationService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAuditClient _auditClient;
        private readonly ILogger<ApplicationRepository> _logger;

        public ApplicationRepository(
            OfferDBContext context,
            IConfigurationService configurationService,
            IHttpContextAccessor httpContextAccessor,
            IAuditClient auditClient,
            ILogger<ApplicationRepository> logger
            )
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _configurationService = configurationService ?? throw new ArgumentNullException(nameof(configurationService));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _auditClient = auditClient ?? throw new ArgumentNullException(nameof(auditClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public IUnitOfWork UnitOfWork
        {
            get
            {
                return _context;
            }
        }

        public Application Add(Application application)
        {
            Application applicationCreated = _context.Applications.Add(application).Entity;
            return applicationCreated;
        }

        public List<ApplicationsByStatus> GetApplicationsByStatus()
        {
            var statuses = _context.Applications
               .GroupBy(p => new { p.Status })
               .Select(g => new ApplicationsByStatus { Status = g.Key.Status, Count = g.Count() }).ToList();
            return statuses;
        }

        public List<Application> CheckExistingOffersForCustomer(string customerNumber, List<ApplicationStatus> statusList, List<PartyRole> rolesList)
        {
            var applications = _context.Applications.Where(x => x.CustomerNumber.Equals(customerNumber)
                                                            || x.InvolvedParties.Where(z =>
                                rolesList.Contains(z.PartyRole) && z.CustomerNumber.Equals(customerNumber)).Any())
                                .ToList();
            if (statusList != null && statusList.Count > 0)
            {
                applications = applications.Where(x => statusList.Contains(x.Status)).ToList();
            }
            return applications;
        }

        public List<Application> CheckExistingOffersForProspect(string username, string email, List<ApplicationStatus> statusList, List<PartyRole> rolesList)
        {
            var applications = _context.Applications.Where(
                x => x.Initiator.ToLower().Equals(username.ToLower())
                || x.InvolvedParties.Where(
                    z => rolesList.Contains(z.PartyRole) &&
                         !string.IsNullOrEmpty(z.EmailAddress) &&
                         z.EmailAddress.ToLower().Equals(email.ToLower())
                    ).Any()).ToList();
            if (statusList != null && statusList.Count > 0)
            {
                applications = applications.Where(x => statusList.Contains(x.Status)).ToList();
            }
            return applications;
        }

        List<Application> IApplicationRepository.GetProspectOffers(string username, List<ApplicationStatus> statusList)
        {
            var applications = _context.Applications.Where(x =>
                x.Initiator.ToLower().Equals(username.ToLower())
                && statusList.Contains(x.Status)
            ).ToList();
            foreach (Application app in applications)
            {
                _context.Entry(app).Collection(i => i.InvolvedParties).Load();
            }
            return applications;
        }


        private class ApplicationQueryResult
        {
            public int PageSize { get; set; }
            public int TotalCount { get; set; }
            public int TotalPages { get; set; }
            public int Page { get; set; }
            public IQueryable<Application> Query { get; set; }
        }

        public async Task<List<Party>> GetInvolvedParties(long applicationNumber)
        {
            var application = await GetAsync(applicationNumber, "involved-parties");
            if (application == null)
            {
                return null;
            }

            await _auditClient.WriteLogEntry(AuditLogEntryAction.Get, AuditLogEntryStatus.Success, "parties", application.ApplicationNumber, "View involved parties");

            return application.InvolvedParties;
        }

        private ApplicationQueryResult GetQuery(List<ApplicationStatus> statusList, List<ArrangementKind?> kindList, string productCode,
            string customerData, string statusFromDate, string applicationDateFrom, string applicationDateTo, string include, List<string> trim,
            int? page, int? pageSize, string sortBy, string sortOrder, string partialApplicationNumber, string customerNumber,
            List<string> partyRoles, List<string> channels, string expirationDateFrom, string expirationDateTo, string initiator, bool useIncludes)
        {

            string[] inclusions = string.IsNullOrEmpty(include) ? new string[] { } : include.Split(',');
            var query = _context.Applications.AsQueryable();
            if (useIncludes)
            {
                if (inclusions.Contains("arrangement-requests"))
                {
                    query = query.Include(x => x.ArrangementRequests).ThenInclude(ar => (ar as ArrangementRequest).ProductSnapshotDb);
                }
                if (inclusions.Contains("involved-parties"))
                {
                    query = query.Include(x => x.InvolvedParties);
                }
                if (inclusions.Contains("documents"))
                {
                    query = query.Include(x => x.Documents);
                }
                if (inclusions.Contains("questionnaires"))
                {
                    query = query.Include(x => x.Questionnaires);
                }
            }
            var organizationUnit = _httpContextAccessor.GetQueryParameter("organization-unit");
            if (!string.IsNullOrEmpty(organizationUnit))
            {
                query = query.Include(x => x.OrganizationUnit);
            }
            if (statusList != null && statusList.Count > 0)
                query = query.Where(x => statusList.Contains(x.Status));
            if (kindList != null && kindList.Count > 0)
                query = query.Where(x => x.ArrangementRequests.Where(a => (a.Enabled ?? false) && kindList.Contains(a.ArrangementKind)).Any());
            if (!string.IsNullOrEmpty(productCode))
                query = query.Where(x => x.ProductCode == productCode);
            if (!string.IsNullOrEmpty(initiator))
                query = query.Where(x => x.Initiator.ToLower() == initiator.ToLower());
            if (!string.IsNullOrEmpty(customerNumber))
                query = query.Where(x => x.CustomerNumber == customerNumber);
            if (!string.IsNullOrEmpty(customerData))
                query = query.Where(x => (
                        x.CustomerName.ToLower().Contains(customerData.ToLower()) ||
                        x.CustomerNumber.Contains(customerData) ||
                        x.InvolvedParties.Where(z =>
                            (z.PartyRole == PartyRole.CustomerRepresentative || z.PartyRole == PartyRole.Customer)
                            && z.IdentificationNumber.Contains(customerData)
                            || z.EmailAddress.ToLower().Contains(customerData.ToLower()))
                        .Any() ||
                        x.InvolvedParties.OfType<IndividualParty>().Where(z =>
                            (z.PartyRole == PartyRole.CustomerRepresentative || z.PartyRole == PartyRole.Customer)
                            && z.MobilePhone.Contains(customerData))
                        .Any()
                    ));
            if (!string.IsNullOrEmpty(partialApplicationNumber))
                query = query.Where(x => x.ApplicationNumber.Contains(partialApplicationNumber));
            if (partyRoles != null && partyRoles.Count > 0)
                query = query.Where(x => x.InvolvedParties.Where(a => partyRoles.Contains(a.PartyRole.ToString().ToLower())).Any());
            if (channels != null && channels.Count > 0)
                query = query.Where(x => channels.Contains(x.ChannelCode));

            if (!string.IsNullOrEmpty(applicationDateFrom))
                query = query.Where(x => x.RequestDate >= DateTime.Parse(applicationDateFrom));
            if (!string.IsNullOrEmpty(expirationDateFrom))
                query = query.Where(x => x.ExpirationDate >= DateTime.Parse(expirationDateFrom));
            if (!string.IsNullOrEmpty(expirationDateTo))
                query = query.Where(x => x.ExpirationDate < DateTime.Parse(expirationDateTo));
            if (!string.IsNullOrEmpty(applicationDateTo))
                query = query.Where(x => x.RequestDate < DateTime.Parse(applicationDateTo).AddDays(1));
            if (!string.IsNullOrEmpty(statusFromDate))
                query = query.Where(x => x.StatusChangeDate >= DateTime.Parse(statusFromDate));
            // Handle OU filter security
            if (!string.IsNullOrEmpty(organizationUnit))
            {

                List<string> filterList = new List<string>();
                string[] organizationUnitSpplited = organizationUnit.Split(",");
                if (organizationUnitSpplited.Length > 0)
                {
                    foreach (string ou in organizationUnitSpplited)
                    {
                        var ouData = _context.OrganizationUnits.Where(x => x.Code.Equals(ou)).FirstOrDefault();
                        if (ouData != null)
                        {
                            filterList.Add(ouData.NavigationCode + "%");
                        }
                    }
                    query = query.WhereAny(filterList.Select(w => (Expression<Func<Application, bool>>)(x => EF.Functions.Like(x.OrganizationUnit.NavigationCode, w))).ToArray());
                }
            }
            // sorting
            if (string.IsNullOrWhiteSpace(sortBy))
            {
                if (sortOrder == "desc")
                    query = query.OrderByDescending(it => it.ApplicationId);
                else
                {
                    query = query.OrderBy(it => it.ApplicationId);
                    sortOrder = "asc";  // set to default
                }
                sortBy = "application-id";
            }
            else
            {
                System.Linq.Expressions.Expression<Func<Application, object>> sort;
                if (sortBy == "application-number")
                {
                    sort = it => it.ApplicationNumber;
                }
                else if (sortBy == "customer-name")
                {
                    sort = it => it.CustomerName;
                }
                else if (sortBy == "product-name")
                {
                    sort = it => it.ProductName;
                }
                else if (sortBy == "request-date")
                {
                    sort = it => it.RequestDate;
                }
                else if (sortBy == "status")
                {
                    sort = it => it.Status;
                }
                //else if (sortBy == "amount")
                //{
                //    sort = it => it.ArrangementRequests
                //    .FirstOrDefault(ar => (ar.Enabled ?? false) &&
                //        ar.ProductCode == (productCode ?? it.ProductCode))
                //    .GetPropertyDynamic("Amount");
                //}
                else
                {
                    sortBy = "application-id";
                    sort = it => it.ApplicationId;
                }

                if (sortOrder == "desc")
                    query = query.OrderByDescending(sort);
                else
                {
                    query = query.OrderBy(sort);
                    sortOrder = "asc";  // default
                }
            }
            // paging
            int pSize = pageSize.HasValue && pageSize.Value > 0 ? pSize = pageSize.GetValueOrDefault() : 10;
            int pNumber = page.HasValue && page.Value > 0 ? page.GetValueOrDefault() : 1;
            // TODO: count async
            int total = query.Count();
            query = query.Skip((pNumber - 1) * pSize).Take(pSize);
            int totalPages = (int)Math.Ceiling(total * 1.0 / pSize);
            return new ApplicationQueryResult
            {
                Page = pNumber,
                PageSize = pSize,
                TotalCount = total,
                TotalPages = totalPages,
                Query = query
            };
        }
        public async Task<ApplicationList> GetApplicationsList(List<ApplicationStatus> statusList, List<ArrangementKind?> kindList, string productCode,
            string customerData, string statusFromDate, string applicationDateFrom, string applicationDateTo, string include, List<string> trim,
            int? page, int? pageSize, string sortBy, string sortOrder, string partialApplicationNumber, string customerNumber, List<string> partyRoles, List<string> channels, string expirationDateFrom, string expirationDateTo, string initiator)
        {
            include = string.IsNullOrEmpty(include) ? "arrangement-requests" : ",arrangement-requests";
            var queryResult = GetQuery(statusList, kindList, productCode, customerData, statusFromDate, applicationDateFrom, applicationDateTo, include, trim, page, pageSize, sortBy, sortOrder, partialApplicationNumber, customerNumber, partyRoles, channels, expirationDateFrom, expirationDateTo, initiator, true);


            var query = queryResult.Query;
            List<ApplicationView> selectedApplications = SortApplicationViewList(query, sortBy, sortOrder);

            ApplicationList result = new ApplicationList
            {
                Applications = selectedApplications,
                TotalCount = queryResult.TotalCount,
                TotalPages = queryResult.TotalPages,
                SortOrder = sortOrder,
                SortBy = sortBy,
                Page = queryResult.Page,
                PageSize = queryResult.PageSize
            };

            await _auditClient.WriteLogEntry(AuditLogEntryAction.Get, AuditLogEntryStatus.Success, "application-list", "", "View application list", new { });
            
            return result;
        }

        public async Task<PagedApplicationList> GetApplications(List<ApplicationStatus> statusList, List<ArrangementKind?> kindList, string productCode,
            string customerData, string statusFromDate, string applicationDateFrom, string applicationDateTo, string include, List<string> trim,
            int? page, int? pageSize, string sortBy, string sortOrder, string partialApplicationNumber, string customerNumber, List<string> partyRoles, List<string> channels, string expirationDateFrom, string expirationDateTo, string initiator)
        {
            var queryResult = GetQuery(statusList, kindList, productCode, customerData, statusFromDate, applicationDateFrom, applicationDateTo, include, trim, page, pageSize, sortBy, sortOrder, partialApplicationNumber, customerNumber, partyRoles, channels, expirationDateFrom, expirationDateTo, initiator, true);
            var query = queryResult.Query;
            List<Application> selectedApplications = query.ToList();
            PagedApplicationList result = new PagedApplicationList
            {
                Applications = selectedApplications,
                TotalCount = queryResult.TotalCount,
                TotalPages = queryResult.TotalPages,
                SortOrder = sortOrder,
                SortBy = sortBy,
                Page = queryResult.Page,
                PageSize = queryResult.PageSize
            };

            await _auditClient.WriteLogEntry(AuditLogEntryAction.Get, AuditLogEntryStatus.Success, "applications", "Retrieved paged application list");

            return result;
        }
        /// <summary>
        /// This method is made static because of Memory Leak issue in EF Core
        /// https://github.com/aspnet/EntityFrameworkCore/issues/13048
        /// Still this static method will be hold in Memory with DbContext but only once.
        /// </summary>
        /// <param name="document"></param>
        /// <returns></returns>
        private static ApplicationView GetApplicationView(Application u)
        {
            var applicationView = new ApplicationView
            {
                ApplicationNumber = u.ApplicationNumber,
                Kind = u.ArrangementRequests.FirstOrDefault(ar => ar.ProductCode == u.ProductCode)?.ArrangementKind,
                Status = u.Status,
                ProductCode = u.ProductCode,
                ProductName = u.ProductName,
                CustomerNumber = u.CustomerNumber,
                CustomerName = u.CustomerName,
                OrganizationUnitCode = u.OrganizationUnitCode,
                ChannelCode = u.ChannelCode,
                PortfolioId = u.PortfolioId,
                CampaignCode = u.CampaignCode,
                LeadId = u.LeadId,
                SigningOption = u.SigningOption,
                RequestDate = u.RequestDate,
                ExpirationDate = u.ExpirationDate,
                StatusChangeDate = u.StatusChangeDate,
                LastModified = u.LastModified,
                CreatedByName = u.CreatedByName
            };

            var productCodeRequest = u.ArrangementRequests.FirstOrDefault(ar => ar.ProductCode == u.ProductCode);
            if (productCodeRequest is TermLoanRequest)
            {
                var request = (productCodeRequest as TermLoanRequest);
                applicationView.Term = request.Term;
                applicationView.Amount = request.Amount;
                applicationView.Currency = request.Currency;
            }
            else if (productCodeRequest is TermDepositRequest)
            {
                var request = (productCodeRequest as TermDepositRequest);
                applicationView.Term = request.Term;
                applicationView.Amount = request.Amount;
                applicationView.Currency = request.Currency;
            }
            return applicationView;
        }

        public List<ApplicationView> SortApplicationViewList(IQueryable<Application> query, string sortBy, string sortOrder)
        {
            List<Application> queryResult = query.ToList();
            List<ApplicationView> selectedApplications = new List<ApplicationView>();
            foreach (var app in queryResult)
            {
                try
                {
                    ApplicationView applicationView = GetApplicationView(app);
                    selectedApplications.Add(applicationView);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "GetApplicationView error");
                    continue;
                }
                
            }
            // List<ApplicationView> selectedApplications = queryResult.Select(u => GetApplicationView(u)).ToList();
            // TODO: Alex check this part of code as it does not make sense. 
            //if (sortBy == "amount")
            //{
            //    if (sortOrder == "desc")
            //    {
            //        selectedApplications = selectedApplications.OrderByDescending(a => a.Amount).ToList();
            //    }
            //    else
            //    {
            //        selectedApplications = selectedApplications.OrderBy(a => a.Amount).ToList();
            //    }
            //}
            return selectedApplications;
        }

        public async Task<Application> GetAsync(long applicationNumber, string include = null, string trim = null)
        {
            include = include ?? "";
            string[] inclusions = string.IsNullOrEmpty(include) ? new string[] { } : include.Split(',');
            var applicationExists = _context.Applications.Any(a => a.ApplicationId == applicationNumber);
            if (applicationExists)
            {
                var application = _context.Applications
                    .Where(a => a.ApplicationId == applicationNumber);

                if (inclusions.Contains("arrangement-requests.collateral-requirements"))
                {
                    application = application.Include(a => a.ArrangementRequests)
                        .ThenInclude(ar => (ar as FinanceServiceArrangementRequest).CollateralRequirements);
                }
                else if (inclusions.Contains("arrangement-requests"))
                {
                    application = application.Include(a => a.ArrangementRequests).ThenInclude(ar => (ar as ArrangementRequest).ProductSnapshotDb);
                }
                if (inclusions.Contains("involved-parties"))
                {
                    application = application.Include(i => i.InvolvedParties);
                }
                if (inclusions.Contains("documents"))
                {
                    application = application.Include(i => i.Documents);
                }
                if (inclusions.Contains("questionnaires"))
                {
                    application = application.Include(i => i.Questionnaires);
                }
                application = ApplySecurityFilters(application);
                var app = await application.FirstOrDefaultAsync();
                return app;
            }
            return null;
        }

        private IQueryable<Application> ApplySecurityFilters(IQueryable<Application> application)
        {

            var organizationUnit = _httpContextAccessor.GetQueryParameter("organization-unit");
            if (!string.IsNullOrEmpty(organizationUnit))
            {
                application = application.Include(x => x.OrganizationUnit);
            }
            // START: Security Filters
            var customerNumber = _httpContextAccessor.GetQueryParameter("customer-number");
            if (customerNumber != null)
            {
                application = application.Where(x => x.CustomerNumber == customerNumber);
            }

            var initiator = _httpContextAccessor.GetQueryParameter("initiator");
            if (initiator != null)
            {
                application = application.Where(x => x.Initiator == initiator);
            }

            if (!string.IsNullOrEmpty(organizationUnit))
            {
                List<string> filterList = new List<string>();
                string[] organizationUnitSpplited = organizationUnit.Split(",");
                foreach (string ou in organizationUnitSpplited)
                {
                    var ouData = _context.OrganizationUnits.Where(x => x.Code.Equals(ou)).First();
                    filterList.Add(ouData.NavigationCode + "%");
                }
                application = application.WhereAny(filterList.Select(w => (Expression<Func<Application, bool>>)(x => EF.Functions.Like(x.OrganizationUnit.NavigationCode, w))).ToArray());
            }
            // END: Security Filters
            return application;
        }

        public List<ApplicationDocumentView> GetApplicationDocuments(long applicationNumber)
        {
            var query = _context.ApplicationDocuments.AsQueryable();
            query = query.Include(x => x.ArrangementRequest).Include(x => x.Party);
            query = query.Where(d => d.ApplicationId == applicationNumber)
                .Where(d => d.DocumentContextKind != DocumentContextKind.ArrangementRequestEnum ||
                    (d.ArrangementRequest != null && (d.ArrangementRequest.Enabled == true)));

            return query.Select(x => x.GetDocumentView()).ToList();
        }
        public async Task<ApplicationDetailsView> GetAsyncDetailsView(long applicationNumber, string include = null, string trim = null)
        {
            include = include ?? "";
            string[] inclusions = string.IsNullOrEmpty(include) ? new string[] { } : include.Split(',');
            var applicationExists = _context.Applications.Any(a => a.ApplicationId == applicationNumber);
            if (applicationExists)
            {
                var application = _context.Applications
                    .Where(a => a.ApplicationId == applicationNumber);
                if (inclusions.Contains("arrangement-requests.collateral-requirements"))
                {
                    application = application.Include(a => a.ArrangementRequests)
                        .ThenInclude(ar => (ar as FinanceServiceArrangementRequest).CollateralRequirements);
                }
                else if (inclusions.Contains("arrangement-requests"))
                {
                    application = application.Include(a => a.ArrangementRequests).ThenInclude(ar => (ar as ArrangementRequest).ProductSnapshotDb);
                }
                if (inclusions.Contains("involved-parties"))
                {
                    application = application.Include(i => i.InvolvedParties);
                }
                /*if (inclusions.Contains("documents"))
                {
                    application = application.Include(i => i.Documents)
                        .ThenInclude(d => d.Party);
                }*/
                if (inclusions.Contains("questionnaires"))
                {
                    application = application.Include(i => i.Questionnaires);
                }
                application = ApplySecurityFilters(application);
                var app = await application.FirstOrDefaultAsync();
                if (app == null)
                {
                    return null;
                }
                if (app.ArrangementRequests != null)
                {
                    app.ArrangementRequests = app.ArrangementRequests.Where(r => r.Enabled ?? false).ToList();
                }
                var appDetailsView = Mapper.Map<Application, ApplicationDetailsView>(app);

                if (inclusions.Contains("documents"))
                {
                    var documentList = GetApplicationDocuments(app.ApplicationId);
                    appDetailsView.Documents = documentList;
                }

                appDetailsView.PortfolioRequestExist = _context.PortfolioChangeRequests.Where(p => p.ApplicationNumber.Equals(appDetailsView.ApplicationNumber) && p.Status == ChangeRequestsKindApp.Update).Any();

                await _auditClient.WriteLogEntry(AuditLogEntryAction.Get, AuditLogEntryStatus.Success, "application", app.ApplicationNumber, "View application details", new { });

                return appDetailsView;
            }
            return null;
        }

        public Application GetSync(long applicationNumber, string include = null, string trim = null)
        {
            include = include ?? "";
            string[] inclusions = string.IsNullOrEmpty(include) ? new string[] { } : include.Split(',');
            var application = _context.Applications.Find(applicationNumber);
            if (application != null)
            {
                if (inclusions.Contains("arrangement-requests"))
                {
                    _context.Entry(application)
                    .Collection(i => i.ArrangementRequests).Load();
                }
                if (inclusions.Contains("involved-parties"))
                {
                    _context.Entry(application)
                    .Collection(i => i.InvolvedParties).Load();
                }
                if (inclusions.Contains("documents"))
                {
                    _context.Entry(application)
                    .Collection(i => i.Documents).Load();
                }
                if (inclusions.Contains("questionnaires"))
                {
                    _context.Entry(application)
                    .Collection(i => i.Questionnaires).Load();
                }
            }
            return application;
        }

        public void Update(Application application)
        {
            _context.Update(application);
            // _context.SaveChanges();
        }
        public Application UpdateStatus(long applicationNumber, ApplicationStatus? status, StatusInformation statusInformation, string phase)
        {
            var application = _context.Applications.FindAsync(applicationNumber).Result;
            application.Status = status ?? application.Status;
            application.StatusInformation = statusInformation ?? application.StatusInformation;
            application.Phase = phase ?? application.Phase;
            application.StatusChangeDate = DateTime.UtcNow;
            return _context.Update(application).Entity;
        }

        public void UpdateCustomer(long applicationNumber, string idNumber, string idAuthority, DateTime idValidFrom, DateTime idValidTo, string contentUrls,
                                  string countryResident, string cityResident, string postalCodeResident, string streetNameResident, string streetNumberResident,
                                  string countryCorrespondent, string cityCorrespondent, string postalCodeCorrespondent, string streetNameCorrespondent, string streetNumberCorrespondent,
                                  bool accountOwner, bool relatedCustomers, bool politicallyExposedPerson, bool influenceGroup, bool bankAffiliated, bool isAmericanCitizen,
                                  string identificationNumber, Gender gender, DateTime dateOfBirth)
        {
            var parties = _context.Parties.Where(x => x.ApplicationId == applicationNumber && x.PartyRole == PartyRole.Customer).ToList();
            if (parties != null)
            {
                if (parties.Count == 1)
                {
                    if (parties[0] is IndividualParty party)
                    {
                        party.LegalAddress.Country = countryResident;
                        party.LegalAddress.Locality = cityResident;
                        party.LegalAddress.PostalCode = postalCodeResident;
                        party.LegalAddress.Street = streetNameResident;
                        party.LegalAddress.StreetNumber = streetNumberResident;
                        party.ContactAddress.Country = countryCorrespondent;
                        party.ContactAddress.Locality = cityCorrespondent;
                        party.ContactAddress.PostalCode = postalCodeCorrespondent;
                        party.ContactAddress.Street = streetNameCorrespondent;
                        party.ContactAddress.StreetNumber = streetNumberCorrespondent;

                        party.IdentificationNumber = identificationNumber;
                        party.Gender = gender;
                        party.DateOfBirth = dateOfBirth;
                    }
                    else
                    {
                        parties[0].LegalAddress.Country = countryResident;
                        parties[0].LegalAddress.Locality = cityResident;
                        parties[0].LegalAddress.PostalCode = postalCodeResident;
                        parties[0].LegalAddress.Street = streetNameResident;
                        parties[0].LegalAddress.StreetNumber = streetNumberResident;
                        parties[0].ContactAddress.Country = countryCorrespondent;
                        parties[0].ContactAddress.Locality = cityCorrespondent;
                        parties[0].ContactAddress.PostalCode = postalCodeCorrespondent;
                        parties[0].ContactAddress.Street = streetNameCorrespondent;
                        parties[0].ContactAddress.StreetNumber = streetNumberCorrespondent;
                        parties[0].IdentificationNumber = identificationNumber;
                    }


                    IdentificationDocument idDocument = new IdentificationDocument
                    {
                        SerialNumber = idNumber,
                        IssuingAuthority = idAuthority,
                        ContentUrls = contentUrls,
                        IssuedDate = idValidFrom,
                        ValidUntil = idValidTo
                    };
                    parties[0].IdentificationDocument = idDocument;
                    _context.Update(parties[0]);

                    //FATCAQuestionnaire fatca = new FATCAQuestionnaire
                    //{
                    //    ApplicationId = applicationNumberLong,
                    //    PartyId = parties[0].PartyId,
                    //    Purpose = "FATCA",
                    //    Date = DateTime.Now,
                    //    QuestionnaireName = "FATCA"
                    //};

                    //FATCAEntry fatcaEntry = new FATCAEntry
                    //{
                    //    AccountOwner = accountOwner,
                    //    RelatedCustomers = relatedCustomers,
                    //    PoliticallyExposedPerson = politicallyExposedPerson,
                    //    InfluenceGroup = influenceGroup,
                    //    BankAffiliated = bankAffiliated,
                    //    IsAmericanCitizen = isAmericanCitizen
                    //};

                    //fatca.Entries = new List<FATCAEntry> { fatcaEntry };

                    //_context.Questionnaires.Add(fatca);
                }
                else
                {
                    throw new System.Exception("More than one custommer in application: " + applicationNumber);
                }
            }
            else
            {
                throw new System.Exception("No custommer for application: " + applicationNumber);
            }
        }

        public string AnonymizeGdprData(PartyMatcher matcher, bool fake = false)
        {
            var activeStatuses = GetActiveApplicationStatuses();
            #region Anonymize parties
            var erasableDataFullMatchTemp = _context.IndividualParties.Where(p => matcher.Matches(p) == 100 && !activeStatuses.Contains(p.Application.Status))
                .Include(x => x.Application).ToList();
            var erasableDataFullMatch = new List<GdprParty>();
            foreach (var gP in erasableDataFullMatchTemp)
            {
                erasableDataFullMatch.Add(Mapper.Map<IndividualParty, GdprParty>(gP));
            }
            var dataAfterAnonymization = GdprAnonymizeParties(erasableDataFullMatchTemp, fake);
            var erasableDataPartialMatchTemp = _context.IndividualParties.Include(a => a.Application).Where(p => matcher.Matches(p) < 100 && matcher.Matches(p) > 0 && !activeStatuses.Contains(p.Application.Status)).ToList();
            List<GdprParty> erasableDataPartialMatch = null;
            if (erasableDataPartialMatchTemp.Count() > 0)
            {
                erasableDataPartialMatch = Mapper.Map<List<IndividualParty>, List<GdprParty>>(erasableDataPartialMatchTemp);
                for (int i = 0; i < erasableDataPartialMatchTemp.Count(); i++)
                {
                    erasableDataPartialMatch.ElementAt(0).MatchingPercentage = matcher.Matches(erasableDataPartialMatchTemp.ElementAt(0));
                }
            }
            var nonErasableDataPartialMatchTemp = _context.IndividualParties.Include(a => a.Application).Where(p => matcher.Matches(p) < 100 && matcher.Matches(p) > 0 && activeStatuses.Contains(p.Application.Status)).ToList();
            List<GdprParty> nonErasableDataPartialMatch = null;
            if (nonErasableDataPartialMatchTemp.Count() > 0)
            {
                nonErasableDataPartialMatch = Mapper.Map<List<IndividualParty>, List<GdprParty>>(nonErasableDataPartialMatchTemp);
                for (int i = 0; i < nonErasableDataPartialMatchTemp.Count(); i++)
                {
                    nonErasableDataPartialMatch.ElementAt(0).MatchingPercentage = matcher.Matches(nonErasableDataPartialMatchTemp.ElementAt(0));
                }
            }
            var nonErasableDataFullMatch = _context.IndividualParties.Include(a => a.Application).Where(p => matcher.Matches(p) == 100 && activeStatuses.Contains(p.Application.Status)).ToList();
            #endregion

            #region Add document and questionnaire data to export
            var gdprResult = new GdprData
            {
                ErasableDataFullMatch = erasableDataFullMatch,
                NonErasableDataFullMatch = Mapper.Map<List<IndividualParty>, List<GdprParty>>(nonErasableDataFullMatch),
                ErasableDataPartialMatch = erasableDataPartialMatch,
                NonErasableDataPartialMatch = nonErasableDataPartialMatch,
                DataAfterAnonymization = dataAfterAnonymization
            };
            AppendDataToGdprResult(gdprResult);
            #endregion
            return RepositoryUtil.SerializeObjectWithoutNull(gdprResult);
        }

        public string ExportGdprData(PartyMatcher matcher)
        {
            var activeStatuses = GetActiveApplicationStatuses();

            #region Get parties
            var erasableDataPartialMatchTemp = _context.IndividualParties.Include(a => a.Application).Where(p => matcher.Matches(p) < 100 && matcher.Matches(p) > 0).ToList();
            List<GdprParty> erasableDataPartialMatch = null;
            if (erasableDataPartialMatchTemp.Count() > 0)
            {
                erasableDataPartialMatch = Mapper.Map<List<IndividualParty>, List<GdprParty>>(erasableDataPartialMatchTemp);
                for (int i = 0; i < erasableDataPartialMatchTemp.Count(); i++)
                {
                    erasableDataPartialMatch.ElementAt(0).MatchingPercentage = matcher.Matches(erasableDataPartialMatchTemp.ElementAt(0));
                }
            }
            var nonErasableDataPartialMatchTemp = _context.IndividualParties.Include(a => a.Application).Where(p => matcher.Matches(p) < 100 && matcher.Matches(p) > 0 && activeStatuses.Contains(p.Application.Status)).ToList();
            List<GdprParty> nonErasableDataPartialMatch = null;
            if (nonErasableDataPartialMatchTemp.Count() > 0)
            {
                nonErasableDataPartialMatch = Mapper.Map<List<IndividualParty>, List<GdprParty>>(nonErasableDataPartialMatchTemp);
                for (int i = 0; i < nonErasableDataPartialMatchTemp.Count(); i++)
                {
                    nonErasableDataPartialMatch.ElementAt(0).MatchingPercentage = matcher.Matches(nonErasableDataPartialMatchTemp.ElementAt(0));
                }
            }
            var erasableDataFullMatch = _context.IndividualParties.Include(a => a.Application).Where(p => matcher.Matches(p) == 100 && !activeStatuses.Contains(p.Application.Status)).ToList();
            var nonErasableDataFullMatch = _context.IndividualParties.Include(a => a.Application).Where(p => matcher.Matches(p) == 100 && activeStatuses.Contains(p.Application.Status)).ToList();

            var erasableDataFullMatchObj = Mapper.Map<List<IndividualParty>, List<GdprParty>>(erasableDataFullMatch);
            var nonEresableDataFullMatchObj = Mapper.Map<List<IndividualParty>, List<GdprParty>>(nonErasableDataFullMatch);
            #endregion

            var gdprExport = new GdprData
            {
                ErasableDataFullMatch = erasableDataFullMatchObj,
                NonErasableDataFullMatch = nonEresableDataFullMatchObj,
                ErasableDataPartialMatch = erasableDataPartialMatch,
                NonErasableDataPartialMatch = nonErasableDataPartialMatch
            };

            #region Add document and questionnaire data to export
            AppendDataToGdprResult(gdprExport);
            #endregion

            return RepositoryUtil.SerializeObjectWithoutNull(gdprExport);
        }

        public Currency CalculateExposureInTargetCurrency(ExposureList exposureList)
        {
            // Convert amounts to TargetCurrency 
            var targetCurrency = _configurationService.GetEffective("offer/exposure/target-currency", "EUR").Result;
            var conversionMethod = _configurationService.GetEffective("offer/exposure/currency-conversion-method", "Buy to middle").Result;
            var riskCategory = _configurationService.GetEffective("offer/exposure/risk-category", "1").Result.
                            Split(",").Where(c => !string.IsNullOrWhiteSpace(c)).Select(p => p.Trim()).ToList();
            decimal total = 0;
            decimal totalDebt = 0;
            if (exposureList != null)
            {
                foreach (var exp in exposureList.Exposures)
                {
                    if (targetCurrency.Equals(exp.Currency))
                    {
                        exp.ExposureApprovedInTargetCurrency = exp.ExposureApprovedInSourceCurrency;
                        exp.AnnuityInTargetCurrency = exp.AnnuityInSourceCurrency;
                        exp.ExposureOutstandingAmountInTargetCurrency = exp.ExposureOutstandingAmountInSourceCurrency;
                    }
                    else
                    {
                        exp.ExposureApprovedInTargetCurrency = new CurrencyConverter().CurrencyConvert(exp.ExposureApprovedInSourceCurrency, exp.Currency, targetCurrency,
                                                                            DateTime.Today.ToString("o", CultureInfo.InvariantCulture), conversionMethod);
                        exp.AnnuityInTargetCurrency = new CurrencyConverter().CurrencyConvert(exp.AnnuityInSourceCurrency, exp.Currency, targetCurrency,
                                                                            DateTime.Today.ToString("o", CultureInfo.InvariantCulture), conversionMethod);
                        exp.ExposureOutstandingAmountInTargetCurrency = new CurrencyConverter().CurrencyConvert(exp.ExposureOutstandingAmountInSourceCurrency, exp.Currency, targetCurrency,
                                                                            DateTime.Today.ToString("o", CultureInfo.InvariantCulture), conversionMethod);
                    }
                    if (riskCategory.Contains(exp.RiskCategory))
                    {
                        total += exp.ExposureApprovedInTargetCurrency;
                        totalDebt += exp.ExposureOutstandingAmountInTargetCurrency;
                    }
                }
                exposureList.TotalApprovedAmountInTargetCurrency = total;
                exposureList.TotalOutstandingAmountInTargetCurrency = totalDebt;
            }
            return new Currency { Amount = total, Code = targetCurrency };
        }

        private void AppendDataToGdprResult(GdprData gdprData)
        {
            if (gdprData.ErasableDataFullMatch != null)
            {
                foreach (GdprParty data in gdprData.ErasableDataFullMatch)
                {
                    //AddDocumentsToExport(data);
                    AddQuestionnairesToExport(data);
                }
            }
            if (gdprData.NonErasableDataFullMatch != null)
            {
                foreach (GdprParty data in gdprData.NonErasableDataFullMatch)
                {
                    //AddDocumentsToExport(data);
                    AddQuestionnairesToExport(data);
                }
            }
            if (gdprData.ErasableDataPartialMatch != null)
            {
                foreach (GdprParty data in gdprData.ErasableDataPartialMatch)
                {
                    //AddDocumentsToExport(data);
                    AddQuestionnairesToExport(data);
                }
            }
            if (gdprData.NonErasableDataPartialMatch != null)
            {
                foreach (GdprParty data in gdprData.NonErasableDataPartialMatch)
                {
                    //AddDocumentsToExport(data);
                    AddQuestionnairesToExport(data);
                }
            }
        }

        //private void AddDocumentsToExport(GdprParty party)
        //{
        //    var appDocuments = _context.ApplicationDocuments.Where(d => d.ApplicationId == party.ApplicationId).Where(d => d.Attachments.Count > 0).ToList();
        //    if (appDocuments != null)
        //    {
        //        var documents = Mapper.Map<List<ApplicationDocument>, List<GdprApplicationDocument>>(appDocuments);
        //        party.Documents = documents;
        //    }
        //}

        private void AddQuestionnairesToExport(GdprParty party)
        {
            var questionnaires = _context.Questionnaires.Where(q => q.ApplicationId == party.ApplicationId && party.PartyRole.Equals(PartyRole.Customer)).ToList();
            if (questionnaires != null)
            {
                party.Questionnaires = Mapper.Map<List<Questionnaire>, List<GdprQuestionnaire>>(questionnaires);
            }
        }

        private List<GdprParty> GdprAnonymizeParties(List<IndividualParty> parties, bool fake = false, bool withApplication = true, bool withDocuments = true)
        {
            if (parties == null || parties.Count() == 0)
            {
                return null;
            }
            foreach (var party in parties)
            {
                #region Application Data
                if (withApplication)
                {
                    AnonymizeApplication(party, fake);
                }
                #endregion

                #region Questionnaire data
                var questionnaires = _context.Questionnaires.Where(q => q.ApplicationId == party.ApplicationId && party.PartyRole.Equals(PartyRole.Customer)).ToList();
                foreach (var content in questionnaires)
                {
                    if (content is GenericQuestionnaire)
                    {
                        ((GenericQuestionnaire)content).Entries = null;
                    }
                }
                if (!fake)
                {
                    _context.UpdateRange(questionnaires);
                }
                #endregion

                #region Party data
                party.CustomerName = "******* *********";
                if (party.LegalAddress != null)
                {
                    party.LegalAddress.Formatted = party.LegalAddress.Formatted == null ? null : "***************";
                    party.LegalAddress.Street = party.LegalAddress.Street == null ? null : "*********";
                    party.LegalAddress.StreetNumber = party.LegalAddress.StreetNumber == null ? null : "***";
                    party.LegalAddress.PostalCode = party.LegalAddress.PostalCode == null ? null : "*****";
                    party.LegalAddress.Locality = party.LegalAddress.Locality == null ? null : "*********";
                    party.LegalAddress.Country = party.LegalAddress.Country == null ? null : "*****";
                    party.LegalAddress.AddressCode = party.LegalAddress.AddressCode == null ? null : "*****";
                    party.LegalAddress.Coordinates = new Coordinates();
                }
                if (party.ContactAddress != null)
                {
                    party.ContactAddress.Formatted = party.ContactAddress.Formatted == null ? null : "***************";
                    party.ContactAddress.Street = party.ContactAddress.Street == null ? null : "*********";
                    party.ContactAddress.StreetNumber = party.ContactAddress.StreetNumber == null ? null : "***";
                    party.ContactAddress.PostalCode = party.ContactAddress.PostalCode == null ? null : "*****";
                    party.ContactAddress.Locality = party.ContactAddress.Locality == null ? null : "*********";
                    party.ContactAddress.Country = party.ContactAddress.Country == null ? null : "*****";
                    party.ContactAddress.AddressCode = party.ContactAddress.AddressCode == null ? null : "*****";
                    party.ContactAddress.Coordinates = new Coordinates();
                }
                party.IdentificationNumber = party.IdentificationNumber == null ? null : "***********";
                party.IdentificationDocument = null;
                party.Username = party.Username == null ? null : Sha256(party.Username);
                #endregion

                #region Individual data
                party.GivenName = party.GivenName == null ? null : "*******";
                party.ParentName = party.ParentName == null ? null : "*******";
                party.Surname = party.Surname == null ? null : "**********";
                party.MaidenName = party.MaidenName == null ? null : "*******";
                party.MothersMaidenName = party.MothersMaidenName == null ? null : "*******";
                if (!string.IsNullOrEmpty(party.LifecycleStatus.ToString()))
                {
                    party.LifecycleStatus = default;
                }
                party.EmailAddress = party.EmailAddress == null ? null : "***************";
                party.MobilePhone = party.MobilePhone == null ? null : "**********";
                party.HomePhoneNumber = party.HomePhoneNumber == null ? null : "**********";
                if (!string.IsNullOrEmpty(party.Gender.ToString()))
                {
                    party.Gender = Gender.Unknown;
                }
                party.DateOfBirth = party.DateOfBirth == null ? party.DateOfBirth : default(DateTime);
                party.PlaceOfBirth = party.PlaceOfBirth == null ? null : "********";
                party.ResidentialStatus = party.ResidentialStatus == null ? null : "******";
                party.CountryOfResidence = party.CountryOfResidence == null ? null : "********";
                party.ResidentialStatusDate = party.ResidentialStatusDate == null ? party.ResidentialStatusDate : default(DateTime);
                party.ResidentialAddressDate = party.ResidentialAddressDate == null ? party.ResidentialAddressDate : default(DateTime);
                if (!string.IsNullOrEmpty(party.MaritalStatus?.ToString()))
                {
                    party.MaritalStatus = default(MaritalStatus);
                }
                if (!string.IsNullOrEmpty(party.EducationLevel?.ToString()))
                {
                   party.EducationLevel = party.EducationLevel == null ? null : "*********";
                }
                if (!string.IsNullOrEmpty(party.HomeOwnership?.ToString()))
                {
                    party.HomeOwnership = default(HomeOwnership);
                }
                if (!string.IsNullOrEmpty(party.CarOwnership?.ToString()))
                {
                    party.CarOwnership = default(CarOwnership);
                }
                party.Occupation = party.Occupation == null ? null : "*********";
                party.EmploymentData = new EmploymentData
                {
                    Employments = null,
                    EmploymentStatus = null, // default(EmploymentStatus),
                    EmploymentStatusDate = default(DateTime),
                    TotalWorkPeriod = party.EmploymentData?.TotalWorkPeriod == null ? null : "*********"
                };
                party.HouseholdInfo = null;
                party.PreferredCulture = party.PreferredCulture == null ? null : "*****";
                #endregion
            }
            if (!fake)
            {
                _context.UpdateRange(parties);
            }
            return Mapper.Map<List<IndividualParty>, List<GdprParty>>(parties);
        }

        private void AnonymizeParties(List<OrganizationParty> parties, bool fake = false, bool withApplication = true, bool withDocuments = true)
        {
            if (parties == null || parties.Count() == 0)
            {
                return;
            }
            foreach (var party in parties)
            {
                #region Application Data
                if (withApplication)
                {
                    AnonymizeApplication(party, fake);
                }
                #endregion

                #region Questionnaire data
                var questionnaires = _context.Questionnaires.Where(q => q.ApplicationId == party.ApplicationId && party.PartyRole.Equals(PartyRole.Customer)).ToList();
                foreach (var content in questionnaires)
                {
                    if (content is GenericQuestionnaire)
                    {
                        ((GenericQuestionnaire)content).Entries = null;
                    }
                }
                if (!fake)
                {
                    _context.UpdateRange(questionnaires);
                }
                #endregion

                #region Party data
                party.CustomerName = "******* *********";
                if (party.LegalAddress != null)
                {
                    party.LegalAddress.Formatted = party.LegalAddress.Formatted == null ? null : "***************";
                    party.LegalAddress.Street = party.LegalAddress.Street == null ? null : "*********";
                    party.LegalAddress.StreetNumber = party.LegalAddress.StreetNumber == null ? null : "***";
                    party.LegalAddress.PostalCode = party.LegalAddress.PostalCode == null ? null : "*****";
                    party.LegalAddress.Locality = party.LegalAddress.Locality == null ? null : "*********";
                    party.LegalAddress.Country = party.LegalAddress.Country == null ? null : "*****";
                    party.LegalAddress.AddressCode = party.LegalAddress.AddressCode == null ? null : "*****";
                    party.LegalAddress.Coordinates = new Coordinates();
                }
                if (party.ContactAddress != null)
                {
                    party.ContactAddress.Formatted = party.ContactAddress.Formatted == null ? null : "***************";
                    party.ContactAddress.Street = party.ContactAddress.Street == null ? null : "*********";
                    party.ContactAddress.StreetNumber = party.ContactAddress.StreetNumber == null ? null : "***";
                    party.ContactAddress.PostalCode = party.ContactAddress.PostalCode == null ? null : "*****";
                    party.ContactAddress.Locality = party.ContactAddress.Locality == null ? null : "*********";
                    party.ContactAddress.Country = party.ContactAddress.Country == null ? null : "*****";
                    party.ContactAddress.AddressCode = party.ContactAddress.AddressCode == null ? null : "*****";
                    party.ContactAddress.Coordinates = new Coordinates();
                }
                // party.RegistrationProfile = default(RegistrationProfile);
                party.IdentificationNumber = party.IdentificationNumber == null ? null : "***********";
                party.IdentificationDocument = null;
                #endregion

                #region Organization data
                party.RegisteredName = party.RegisteredName == null ? null : "***********";
                #endregion
            }
            if (!fake)
            {
                _context.UpdateRange(parties);
            }
        }

        private void AnonymizeParties(List<Party> parties, bool fake = false, bool withApplication = true, bool withDocuments = true)
        {
            if (parties == null || parties.Count() == 0)
            {
                return;
            }
            foreach (var party in parties)
            {
                #region Application Data
                if (withApplication)
                {
                    AnonymizeApplication(party, fake);
                }
                #endregion

                #region Questionnaire data
                var questionnaires = _context.Questionnaires.Where(q => q.ApplicationId == party.ApplicationId && party.PartyRole.Equals(PartyRole.Customer)).ToList();
                foreach (var content in questionnaires)
                {
                    if (content is GenericQuestionnaire)
                    {
                        ((GenericQuestionnaire)content).Entries = null;
                    }
                }
                if (!fake)
                {
                    _context.UpdateRange(questionnaires);
                }
                #endregion

                #region Party data
                party.CustomerName = "******* *********";
                if (party.LegalAddress != null)
                {
                    party.LegalAddress.Formatted = party.LegalAddress.Formatted == null ? null : "***************";
                    party.LegalAddress.Street = party.LegalAddress.Street == null ? null : "*********";
                    party.LegalAddress.StreetNumber = party.LegalAddress.StreetNumber == null ? null : "***";
                    party.LegalAddress.PostalCode = party.LegalAddress.PostalCode == null ? null : "*****";
                    party.LegalAddress.Locality = party.LegalAddress.Locality == null ? null : "*********";
                    party.LegalAddress.Country = party.LegalAddress.Country == null ? null : "*****";
                    party.LegalAddress.AddressCode = party.LegalAddress.AddressCode == null ? null : "*****";
                    party.LegalAddress.Coordinates = new Coordinates();
                }
                if (party.ContactAddress != null)
                {
                    party.ContactAddress.Formatted = party.ContactAddress.Formatted == null ? null : "***************";
                    party.ContactAddress.Street = party.ContactAddress.Street == null ? null : "*********";
                    party.ContactAddress.StreetNumber = party.ContactAddress.StreetNumber == null ? null : "***";
                    party.ContactAddress.PostalCode = party.ContactAddress.PostalCode == null ? null : "*****";
                    party.ContactAddress.Locality = party.ContactAddress.Locality == null ? null : "*********";
                    party.ContactAddress.Country = party.ContactAddress.Country == null ? null : "*****";
                    party.ContactAddress.AddressCode = party.ContactAddress.AddressCode == null ? null : "*****";
                    party.ContactAddress.Coordinates = new Coordinates();
                }
                // party.RegistrationProfile = default(RegistrationProfile);
                party.IdentificationNumber = party.IdentificationNumber == null ? null : "***********";
                party.IdentificationDocument = null;
                #endregion
            }
            if (!fake)
            {
                _context.UpdateRange(parties);
            }
        }

        private void AnonymizeApplication(Application application, string username, bool fake = false)
        {
            if (application == null)
            {
                return;
            }
            application.CustomerName = "******* *********";
            if (username.Equals(application.Initiator))
            {
                application.Initiator = Sha256(username);
            }
            if (!fake)
            {
                _context.Update(application);
            }
        }

        private void AnonymizeApplication(Party party, bool fake = false)
        {
            var application = party?.Application;
            if (application == null)
            {
                application = _context.Applications.Where(a => a.ApplicationId == party.ApplicationId).FirstOrDefault();
            }
            AnonymizeApplication(application, party.Username, fake);
        }

        private static string Sha256(string randomString)
        {
            using (var crypt = new SHA256Managed())
            {
                string hash = String.Empty;
                byte[] crypto = crypt.ComputeHash(Encoding.ASCII.GetBytes(randomString));
                foreach (byte theByte in crypto)
                {
                    hash += theByte.ToString("x2");
                }
                return hash;
            }
        }

        private List<ApplicationStatus> GetActiveApplicationStatuses()
        {
            var rawStatuses = _configurationService.GetEffective("offer/active-statuses", "draft,active,approved,accepted").Result;

            return EnumUtils.GetEnumPropertiesForListString<ApplicationStatus>(rawStatuses);
        }

        public ApplicationDocument CreateApplicationDocument(long applicationNumber, ApplicationDocument document)
        {
            var app = _context.Applications.Include(a => a.Documents).Where(a => a.ApplicationId == applicationNumber).FirstOrDefault();
            if (app == null)
            {
                throw new ApplicationNotFoundException("Application not found");
            }
            if (app.Documents == null)
            {
                app.Documents = new List<ApplicationDocument>();
            }
            else
            {
                var doc = app.Documents.Where(d => d.DocumentName.Equals(document.DocumentName)).FirstOrDefault();
                if (doc != null)
                {
                    Console.WriteLine("Document with name '" + document.DocumentName + "' already exists");
                    return doc;
                }
            }
            app.Documents.Add(document);
            _context.Update(app);
            return app.Documents.Last();
        }

        public bool IsMainProduct(long applicationNumber, int arrangementRequestId)
        {
            ArrangementRequest request = _context.ArrangementRequests.Include(ar => (ar as ArrangementRequest).ProductSnapshotDb).Where(x => x.ApplicationId.Equals(applicationNumber) && x.ArrangementRequestId.Equals(arrangementRequestId)).FirstOrDefault();
            if (request == null)
            {
                return false;
            }
            var count = _context.Applications.Include(k => k.ArrangementRequests).ThenInclude(ar => (ar as ArrangementRequest).ProductSnapshotDb).Where(x => x.ApplicationId.Equals(applicationNumber)
           && x.ProductCode.Equals(request.ProductCode)
                                                && x.ArrangementRequests.Any(y => y.ProductCode.Equals(request.ProductCode)))
                                                 .Count();
            return count == 1;
        }

        public Task<Application> GetAsyncTracked(long applicationNumber, string include = null, string trim = null)
        {
            throw new NotImplementedException();
        }

        public IDictionary<string, object> GetCommercialDetails(long applicationNumber)
        {
            var appDetails = _context.Applications
                .Where(a => a.ApplicationId == applicationNumber)
                .Select(a => new { a.ProductCode })
                .FirstOrDefault();
            if (appDetails == null)
            {
                throw new ApplicationNotFoundException();
            }
            var commercialDetails = _context.ArrangementRequests
                .Where(r => r.ApplicationId == applicationNumber &&
                        r.ProductCode.Equals(appDetails.ProductCode) &&
                        r is FinanceServiceArrangementRequest)
                .Select(r => new
                {
                    (r as FinanceServiceArrangementRequest).Amount,
                    (r as FinanceServiceArrangementRequest).Currency,
                    (r as FinanceServiceArrangementRequest).Term
                })
                .FirstOrDefault();
            if (commercialDetails != null)
            {
                var details = new Dictionary<string, object>
                {
                    { "amount", commercialDetails.Amount },
                    { "currency", commercialDetails.Currency },
                    { "term", commercialDetails.Term }
                };
                return details;
            }
            return null;
        }

        public async Task<IDictionary<string, IDictionary<string, JToken>>> GetExtendedData(long applicationNumber)
        {
            var app = await GetAsync(applicationNumber);
            if (app == null)
            {
                return null;
            }
            if (app.Extended == null)
            {
                return new Dictionary<string, IDictionary<string, JToken>>();
            }
            await _auditClient.WriteLogEntry(AuditLogEntryAction.Get, AuditLogEntryStatus.Success, "extended-data", app.ApplicationNumber, "Extended data of application retrieved", new { });
            return app.Extended;
        }

        public async Task<IDictionary<string, JToken>> GetExtendedDataSection(long applicationNumber, string sectionName)
        {
            var app = await GetAsync(applicationNumber);

            if (app == null)
            {
                return null;
            }

            if (app.Extended == null || !app.Extended.ContainsKey(sectionName))
            {
                 return new Dictionary<string, JToken>();
            }
            await _auditClient.WriteLogEntry(AuditLogEntryAction.Get, AuditLogEntryStatus.Success, "extended-data-section", app.ApplicationNumber, "Extended data section of application retrieved", app.Extended[sectionName]);
            return app.Extended[sectionName];
        }

        public async Task<Application> GetApplicationByLeadId(long? leadId)
        {
            var app = await _context.Applications.Where(a => a.LeadId == leadId).FirstOrDefaultAsync();
            if (app == null)
            {
                return null;
            }
            return app;
        }

        public async Task<bool?> DeleteExtendedDataSection(long applicationNumber, string sectionName)
        {
            var app = await GetAsync(applicationNumber);
            if (app == null)
            {
                return null;
            }
            if (app.Extended == null || !app.Extended.ContainsKey(sectionName))
            {
                return null;
            }
            var deletedItem = app.Extended;
            app.Extended.Remove(sectionName);
            await _context.SaveEntitiesAsync();
        
            await _auditClient.WriteLogEntry(AuditLogEntryAction.Delete, AuditLogEntryStatus.Success, "extended-section", app.ApplicationNumber, "Extended section of application deleted", deletedItem);
            return true;
        }

        public async Task<ApplicationAllDataView> GetApplicationWithAllData(long applicationNumber, string username, bool auditLog)
        {
            var application = await GetAsync(applicationNumber, "involved-parties,arrangement-requests,documents,questionnaires");
            var appAllDataView = Mapper.Map<ApplicationAllDataView>(application);

            JObject auditData = new JObject(
                        new JProperty("username", username),
                        new JProperty("action", "Retrieved application with all data")
                        );

            if (auditLog)
            {
                await _auditClient.WriteLogEntry(AuditLogEntryAction.Get, AuditLogEntryStatus.Success, "application", application.ApplicationNumber, auditData);
            }

            return appAllDataView;
        }

    }




    public static class Extensions
    {
        public static IQueryable<T> WhereAny<T>(this IQueryable<T> queryable, params Expression<Func<T, bool>>[] predicates)
        {
            var parameter = Expression.Parameter(typeof(T));
            return queryable.Where(Expression.Lambda<Func<T, bool>>(predicates.Aggregate<Expression<Func<T, bool>>, Expression>(null,
                    (current, predicate) =>
                    {
                        var visitor = new ParameterSubstitutionVisitor(predicate.Parameters[0], parameter);
                        return current != null ? Expression.OrElse(current, visitor.Visit(predicate.Body)) : visitor.Visit(predicate.Body);
                    }),
             parameter));
        }
    }
    public class ParameterSubstitutionVisitor : ExpressionVisitor
    {
        private readonly ParameterExpression _destination;
        private readonly ParameterExpression _source;
        public ParameterSubstitutionVisitor(ParameterExpression source, ParameterExpression destination)
        {
            _source = source;
            _destination = destination;
        }
        protected override Expression VisitParameter(ParameterExpression node)
        {
            return ReferenceEquals(node, _source) ? _destination : base.VisitParameter(node);
        }
    }
}