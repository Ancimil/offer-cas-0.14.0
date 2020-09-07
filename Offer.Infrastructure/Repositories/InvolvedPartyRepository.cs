using System;
using System.Collections.Generic;
using MicroserviceCommon.Domain.SeedWork;
using System.Linq;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Offer.Domain.Exceptions;
using Offer.Domain.Repository;
using System.Threading.Tasks;
using Offer.Domain.AggregatesModel.ApplicationAggregate.ApplicantModel;
using Offer.Domain.AggregatesModel.ApplicationAggregate;
using AutoMapper;
using AssecoCurrencyConvertion;
using System.Globalization;
using MicroserviceCommon.Services;
using Offer.Domain.AggregatesModel.CreditBureauModel;
using MicroserviceCommon.Models;
using Offer.Domain.Utils;
using MicroserviceCommon.Extensions.Broker;
using Asseco.EventBus.Abstractions;
using Newtonsoft.Json.Linq;
using AuditClient;
using AuditClient.Model;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;

namespace Offer.Infrastructure.Repositories
{
    public class InvolvedPartyRepository : IInvolvedPartyRepository
    {
        private readonly OfferDBContext _context;
        private readonly IMasterPartyDataService _masterPartyDataService;
        private readonly IApplicationRepository _applicationRepository;
        private readonly IConfigurationService _configurationService;
        private readonly ApplicationDocumentsResolver _documentsResolver;
        private readonly MessageEventFactory _messageEventFactory;
        private readonly IEventBus _eventBus;
        private readonly IAuditClient _auditClient;
        private readonly ILogger<ApplicationRepository> _logger;

        public IUnitOfWork UnitOfWork
        {
            get
            {
                return _context;
            }
        }

        public InvolvedPartyRepository(
            OfferDBContext context,
            IMasterPartyDataService masterPartyDataService,
            IConfigurationService configurationService,
            ApplicationDocumentsResolver documentsResolver,
            MessageEventFactory messageEventFactory,
            IApplicationRepository applicationRepository,
            IEventBus eventBus,
            IAuditClient auditClient,
            ILogger<ApplicationRepository> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _masterPartyDataService = masterPartyDataService ?? throw new ArgumentNullException(nameof(masterPartyDataService));
            _configurationService = configurationService ?? throw new ArgumentNullException(nameof(configurationService));
            _documentsResolver = documentsResolver ?? throw new ArgumentNullException(nameof(documentsResolver));
            _messageEventFactory = messageEventFactory ?? throw new ArgumentNullException(nameof(messageEventFactory));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _auditClient = auditClient ?? throw new ArgumentNullException(nameof(auditClient));
            _applicationRepository = applicationRepository ?? throw new ArgumentNullException(nameof(applicationRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        }

        public void Update(Party party)
        {
            _context.Update(party);
            _context.SaveChanges();
        }

        public async Task<Party> GetPartyGeneralInformation(long applicationNumber, int partyId, string include = null, string trim = null)
        {
            string[] inclusions = string.IsNullOrEmpty(include) ? new string[] { } : include.Split(',');
            var party = GetParty(applicationNumber, partyId).Result;
            if (party == null)
            {
                return null;
            }
            if (!inclusions.Contains("employment-data"))
            {
                if (party is IndividualParty p)
                {
                    p.EmploymentData = null;
                }
            }
            if (!inclusions.Contains("household-info"))
            {
                if (party is IndividualParty p)
                {
                    p.HouseholdInfo = null;
                }
            }
            if (!inclusions.Contains("financial-profile"))
            {
                if (party is IndividualParty p)
                {
                    p.FinancialProfile = null;
                }
            }
            if (!inclusions.Contains("contact-points"))
            {
                if (party is IndividualParty newP)
                {
                    newP.MobilePhone = null;
                    newP.HomePhoneNumber = null;
                }
                party.EmailAddress = null;
                party.ContactAddress = null;
                party.LegalAddress = null;
            }
            if (!inclusions.Contains("credit-bureau"))
            {
                if (party is IndividualParty newP)
                {
                    newP.CreditBureauData = null;
                }
                else
                {
                    var orgParty = (OrganizationParty)party;
                    orgParty.CreditBureauData = null;
                }
            }
            if (!inclusions.Contains("product-usage"))
            {
                party.ProductUsageInfo = null;
            }

            await _auditClient.WriteLogEntry(AuditLogEntryAction.Get, AuditLogEntryStatus.Success, "party-details", applicationNumber.ToString(), "View party details for " + party.PartyId, new { });

            return party;
        }

        public async Task<EmploymentData> GetPartyEmploymentInfo(long applicationNumber, int partyId)
        {
            var party = await GetParty(applicationNumber, partyId);
            if (party != null && party is IndividualParty)
            {
                await _auditClient.WriteLogEntry(AuditLogEntryAction.Get, AuditLogEntryStatus.Success, "party-employement-info", applicationNumber.ToString(), "View party employement info " + party.PartyId, new { });
                return ((IndividualParty)party).EmploymentData;
            }
            else
            {
                return null;
            }
        }

        public async Task<EmploymentData> UpdatePartyEmploymentInfo(long applicationNumber, int partyId, EmploymentData employmentData)
        {
            var party = await GetParty(applicationNumber, partyId);
            if (party == null)
            {
                return null;
            }
            if (party is IndividualParty pNew)
            {
                pNew.EmploymentData = employmentData;
            }
            else
            {
                throw new InvalidDataException("Operation not allowed for organization type of party.");
            }
            await _context.SaveEntitiesAsync();
            await _auditClient.WriteLogEntry(AuditLogEntryAction.Update, AuditLogEntryStatus.Success, "party-employement-info", applicationNumber.ToString(), "Updated party employement info", pNew.EmploymentData);
            return await GetPartyEmploymentInfo(applicationNumber, partyId);
        }

        public async Task<Household> GetPartyHouseholdInfo(long applicationNumber, int partyId)
        {
            var party = GetParty(applicationNumber, partyId).Result;
            if (party != null && party is IndividualParty)
            {
                await _auditClient.WriteLogEntry(AuditLogEntryAction.Get, AuditLogEntryStatus.Success, "party-household-info", applicationNumber.ToString(), "View party household info for: " + party.PartyId, new { });
                return ((IndividualParty)party).HouseholdInfo;
            }
            else
            {
                return null;
            }
        }

        public async Task<List<Relationship>> GetPartyRelationshis(long applicationNumber, int partyId)
        {
            var party = GetParty(applicationNumber, partyId).Result;
            if (party != null && party is OrganizationParty)
            {
                await _auditClient.WriteLogEntry(AuditLogEntryAction.Get, AuditLogEntryStatus.Success, "party-relationships", applicationNumber.ToString(), "Party relationships", ((OrganizationParty)party).Relationships);
                return ((OrganizationParty)party).Relationships;
            }
            else
            {
                return null;
            }
        }

        public async Task<Household> UpdatePartyHouseholdInfo(long applicationNumber, int partyId, Household household)
        {
            var party = await GetParty(applicationNumber, partyId);
            if (party != null && party is IndividualParty pNew)
            {
                pNew.HouseholdInfo = household;
            }
            else
            {
                return null;
            }
            await _context.SaveEntitiesAsync();

            await _auditClient.WriteLogEntry(AuditLogEntryAction.Update, AuditLogEntryStatus.Success, "party-household-info", applicationNumber.ToString(), "Updated party household info", pNew.HouseholdInfo);

            return await GetPartyHouseholdInfo(applicationNumber, partyId);
        }

        public async Task<FinancialData> GetPartyFinancialProfile(long applicationNumber, int partyId)
        {
            var party = GetParty(applicationNumber, partyId).Result;
            if (party != null && party is IndividualParty customer && customer.FinancialProfile != null)
            {
                var financialData =  new FinancialData
                {
                    ExpenseInfo = customer.FinancialProfile.ExpenseInfo,
                    IncomeInfo = customer.FinancialProfile.IncomeInfo,
                    TotalIncomes = CalculateTotal(customer.FinancialProfile.IncomeInfo),
                    TotalExpenses = CalculateTotal(customer.FinancialProfile.ExpenseInfo)
                };
                await _auditClient.WriteLogEntry(AuditLogEntryAction.Get, AuditLogEntryStatus.Success, "financial-profile", applicationNumber.ToString(), "View financial profile", financialData);
                return financialData;

            }
            else
            {
                return null;
            }
        }

        public async Task<FinancialData> UpdatePartyFinancialProfile(long applicationNumber, int partyId, FinancialProfile financialProfile)
        {
            // Preventing Update of CB Data directly from this method (CB Data Update has its own method)
            // var nonCbOriginatedExpenses = financialProfile.ExpenseInfo.Where(fp => fp.Origin != ExpenseOrigin.CreditBureau).ToList();
           var nonCbOriginatedExpenses = financialProfile.ExpenseInfo.ToList();
            
            financialProfile.ExpenseInfo = nonCbOriginatedExpenses;
            var party = await GetParty(applicationNumber, partyId);
            if (party != null && party is IndividualParty newP)
            {
                //if (newP.FinancialProfile != null)
                //{
                //    // Add existing CB Data to updated Financial Profile
                //    var partyCbData = newP.FinancialProfile.ExpenseInfo.Where(fp => fp.Origin == ExpenseOrigin.CreditBureau).ToList();

                //    if (partyCbData != null && partyCbData.Count() > 0)
                //    {
                //        financialProfile.ExpenseInfo.AddRange(partyCbData);
                //    }
                //}
                newP.FinancialProfile = financialProfile;
            }
            else
            {
                return null;
            }
            var res = await _context.SaveEntitiesAsync();

            await _auditClient.WriteLogEntry(AuditLogEntryAction.Update, AuditLogEntryStatus.Success, "financial-profile", applicationNumber.ToString(), "Updated financial profile", newP.FinancialProfile);

            return await GetPartyFinancialProfile(applicationNumber, partyId);
        }

        public async Task<Party> AddParty(long applicationNumber, Party party, bool auditLog)
        {
            var application = _applicationRepository.GetAsync(applicationNumber).Result;
            if (application == null)
            {
                return null;
            }
            var app = _context.Applications
                .Include(a => a.InvolvedParties)
                .Include(a => a.Documents)
                .Include(a => a.ArrangementRequests)
                .Where(a => a.ApplicationId == applicationNumber).FirstOrDefault();
            if (app == null)
            {
                return null;
            }
            Party newParty;
            party.ApplicationId = app.ApplicationId;
            if (string.IsNullOrEmpty(party.CustomerNumber))
            {
                // new party
                app.InvolvedParties.Add(party);
                //_context.Add(party);
            }
            else
            {
                // existing party - do not allow two roles for same party (Sloba)
                if (app.InvolvedParties.Where(a => !string.IsNullOrEmpty(a.CustomerNumber) && a.CustomerNumber.Equals(party.CustomerNumber)).ToList().Count > 0)
                {
                    throw new DuplicateObjectException("Party already exist in application.");
                }
                newParty = await _masterPartyDataService.GetPartyData(party);
                app.InvolvedParties.Add(newParty);
            }

            // fix customer name for individuals - Tanja
            if (string.IsNullOrEmpty(party.CustomerName) && party is IndividualParty)
            {
                var customer = party as IndividualParty;
                party.CustomerName = customer.GivenName + " " + customer.Surname;
            }

            await _context.SaveEntitiesAsync();
            newParty = app.InvolvedParties.Last();
            app = await _documentsResolver.ResolveDocuments(app);
            await _context.SaveEntitiesAsync();
            await _documentsResolver.CreateApplicationDocumentsFolders();

            // Publish party added event
            var messageObjBuilder = _messageEventFactory.CreateBuilder("offer", "involved-party-added")
                .AddBodyProperty("party-id", newParty.PartyId)
                .AddHeaderProperty("party-id", newParty.PartyId)
                .AddHeaderProperty("application-number", "" + applicationNumber + "-ip-" + newParty.PartyId);

            _eventBus.Publish(messageObjBuilder.Build());

            if (auditLog)
            {
                await _auditClient.WriteLogEntry(AuditLogEntryAction.Add, AuditLogEntryStatus.Success, "party", applicationNumber.ToString(), "New involved party added with id: " + newParty.PartyId, new { });
            }

            return newParty;
        }


        public async Task<bool?> DeleteParty(long applicationNumber, int partyId, bool auditLog)
        {
            var application = _applicationRepository.GetAsync(applicationNumber, "involved-parties,arrangement-requests,documents").Result;
            if (application == null)
            {
                return null;
            }
            var party = application.InvolvedParties?.Where(p => p.PartyId == partyId).FirstOrDefault();
            if (party == null)
            {
                return null;
            }
            application.InvolvedParties.Remove(party);
            await _context.SaveEntitiesAsync();
            application = await _documentsResolver.ResolveDocuments(application);
            await _context.SaveEntitiesAsync();

            if (auditLog)
            {
                await _auditClient.WriteLogEntry(AuditLogEntryAction.Delete, AuditLogEntryStatus.Success, "party", application.ApplicationNumber, "Party has been deleted with id: " + party.PartyId, new { });
            }

            return await _documentsResolver.CreateApplicationDocumentsFolders();
        }


        public ContactPoints GetPartyContactPoints(long applicationNumber, int partyId)
        {
            var party = GetParty(applicationNumber, partyId).Result;
            if (party == null)
            {
                return null;
            }
            if (party is IndividualParty)
            {
                return Mapper.Map<IndividualParty, IndividualContactPoints>((IndividualParty)party);
            }
            else
            {
                return Mapper.Map<OrganizationParty, ContactPoints>((OrganizationParty)party);
            }
        }

        public async Task<ContactPoints> UpdatePartyContactPoints(long applicationNumber, int partyId, ContactPoints contactPoints, bool auditLog)
        {
            var party = await GetParty(applicationNumber, partyId);
            if (party == null)
            {
                return null;
            }
            party.EmailAddress = contactPoints.EmailAddress ?? party.EmailAddress;
            party.ContactAddress = contactPoints.ContactAddress ?? party.ContactAddress;
            party.LegalAddress = contactPoints.LegalAddress ?? party.LegalAddress;
            if (party is IndividualParty newP && contactPoints is IndividualContactPoints newContactPoint)
            {
                newP.MobilePhone = newContactPoint.MobilePhone ?? newP.MobilePhone;
                newP.HomePhoneNumber = newContactPoint.HomePhoneNumber ?? newP.HomePhoneNumber;
            }
            await _context.SaveEntitiesAsync();
            if (auditLog)
            {
                await _auditClient.WriteLogEntry(AuditLogEntryAction.Update, AuditLogEntryStatus.Success, "party-contact-points", applicationNumber.ToString(), "Party contact points updated for "+ party.PartyId, new { });
            }

            return GetPartyContactPoints(applicationNumber, partyId);
        }

        public async Task<bool?> UpdatePartyGeneralInformation(long applicationNumber, int partyId, Party newParty)
        {
            if (newParty == null)
            {
                throw new ArgumentNullException("Missing property 'party-role' or some of the properties are invalid");
            }
            if (newParty.PartyId != 0 && partyId != newParty.PartyId)
            {
                throw new InvalidDataException("Provided information does not correspond to requested party.");
            }
            var party = await GetParty(applicationNumber, partyId);
            if (party == null)
            {
                return null;
            }
            if (newParty is IndividualParty newP && party is IndividualParty originalP)
            {
                newP.HouseholdInfo = originalP.HouseholdInfo;
                newP.EmploymentData = originalP.EmploymentData;
                newP.FinancialProfile = originalP.FinancialProfile;
                newP.HomePhoneNumber = originalP.HomePhoneNumber;
                newP.MobilePhone = originalP.MobilePhone;
                newP.CreditBureauData = originalP.CreditBureauData;
            }

            newParty.ProductUsageInfo = party.ProductUsageInfo;
            newParty.ContactAddress = party.ContactAddress;
            newParty.LegalAddress = party.LegalAddress;
            newParty.EmailAddress = party.EmailAddress;
            var financialIndicatorsChanged = party.DebtToIncome != newParty.DebtToIncome ||
                party.RemainingAbilityToPay != newParty.RemainingAbilityToPay ||
                party.DebtToIncome != newParty.DebtToIncome ||
                party.CustomerValue != newParty.CustomerValue ||
                party.CreditRating != newParty.CreditRating;
            _context.Entry(party).CurrentValues.SetValues(newParty);
            var res = await _context.SaveEntitiesAsync();

            if (financialIndicatorsChanged || newParty.PartyRole == PartyRole.Customer)
            {
                var app = await _context.Applications.Include(a => a.InvolvedParties).FirstAsync(a => a.ApplicationId == applicationNumber);
                if (financialIndicatorsChanged && newParty.PartyRole == PartyRole.Customer)
                {
                    app.DebtToIncome = newParty.DebtToIncome;
                    app.CustomerValue = newParty.CustomerValue;
                    app.CreditRating = newParty.CreditRating;
                    app.CustomerRemainingAbilityToPay = newParty.RemainingAbilityToPay;
                }
                if (financialIndicatorsChanged)
                {
                    var minAtp = app.InvolvedParties.Min(p => p.RemainingAbilityToPay);
                    app.EffectiveRemainingAbilityToPay = minAtp;
                }
                res = await _context.SaveEntitiesAsync();
            }

            await _auditClient.WriteLogEntry(AuditLogEntryAction.Put, AuditLogEntryStatus.Success, "party", applicationNumber.ToString(), "Changed party details for party: " + newParty.PartyId, new { });


            return res;
        }

        private Currency CalculateTotal(IReadOnlyList<BaseAmount> amountList)
        {
            // Convert amounts to TargetCurrency and return total
            var targetCurrency = _configurationService.GetEffective("offer/exposure/target-currency", "EUR").Result;
            var conversionMethod = _configurationService.GetEffective("offer/exposure/currency-conversion-method", "Buy to middle").Result;
            Currency total = new Currency { Amount = 0, Code = targetCurrency };
            if (amountList != null)
            {
                foreach (var member in amountList)
                {
                    decimal convertedAmount = member.Amount.Amount;
                    if (!targetCurrency.Equals(member.Amount.Code))
                    {
                        convertedAmount = new CurrencyConverter().CurrencyConvert(member.Amount.Amount, member.Amount.Code, targetCurrency, DateTime.Today.ToString("o", CultureInfo.InvariantCulture), conversionMethod);
                    }
                    total.Amount += convertedAmount;
                }
            }
            return total;
        }

        public async Task<bool?> UpdatePartyCreditBureauData(long applicationNumber, int partyId, CreditBureauData creditBureauData, bool auditLog)
        {
            var party = await GetParty(applicationNumber, partyId);
            if (party == null)
            {
                return null;
            }
            if (party is IndividualParty individualParty)
            {
                var profile = individualParty.FinancialProfile;
                if (profile == null)
                {
                    profile = new FinancialProfile
                    {
                        ExpenseInfo = new List<Expense>(),
                        IncomeInfo = new List<Income>()
                    };
                }
                else if (profile.ExpenseInfo == null)
                {
                    profile.ExpenseInfo = new List<Expense>();
                }
                else
                {
                    var expenses = profile.ExpenseInfo;
                    expenses.RemoveAll(e => e.Origin.Equals(ExpenseOrigin.CreditBureau));
                    profile.ExpenseInfo = expenses;
                }

                if (creditBureauData.Placements != null)
                {
                    var activePlacements = creditBureauData.Placements.Where(p => p.IsActive && p.Annuity.HasValue && p.Annuity > 0).ToList();
                    var expenseInfo = new List<Expense>();
                    foreach (var placement in activePlacements)
                    {
                        expenseInfo.Add(new Expense
                        {
                            Origin = ExpenseOrigin.CreditBureau,
                            Amount = new Currency
                            {
                                Amount = placement.Annuity.Value,
                                Code = creditBureauData.WorkingCurrency
                            },
                            Source = placement.PlacementKind
                        });
                    }
                    profile.ExpenseInfo.AddRange(expenseInfo);
                }
                individualParty.FinancialProfile = profile;
                individualParty.CreditBureauData = creditBureauData;
            }
            else
            {
                // TODO Resolve party's financial profile and update it according to credit bureau data
                var orgParty = (OrganizationParty)party;
                orgParty.CreditBureauData = creditBureauData;
            }
            var res = await _context.SaveEntitiesAsync();
            if (auditLog)
            {
                await _auditClient.WriteLogEntry(AuditLogEntryAction.Update, AuditLogEntryStatus.Success, "party-credit-bureau-data", applicationNumber.ToString(), "Party credit bureau data updated", creditBureauData);
            }
            return res;
        }

        public async Task<CreditBureauData> GetPartyCreditBureauData(long applicationNumber, int partyId)
        {
            var party = GetParty(applicationNumber, partyId).Result;
            if (party == null)
            {
                return null;
            }
            if (party is IndividualParty individualParty)
            {
                await _auditClient.WriteLogEntry(AuditLogEntryAction.Get, AuditLogEntryStatus.Success, "party-credit-bureau-data", applicationNumber.ToString(), "Party credit bureau data", individualParty.CreditBureauData);
                return individualParty.CreditBureauData;
            }
            else
            {
                await _auditClient.WriteLogEntry(AuditLogEntryAction.Get, AuditLogEntryStatus.Success, "party-credit-bureau-data", applicationNumber.ToString(), "Party credit bureau data", ((OrganizationParty)party).CreditBureauData);
                return ((OrganizationParty)party).CreditBureauData;
            }
        }

        public async Task<List<BankAccount>> GetPartyBankAccounts(long applicationNumber, int partyId)
        {
            var party = GetParty(applicationNumber, partyId).Result;
            if (party != null && party is OrganizationParty)
            {
                try
                {
                    var listOfBankAccounts = JsonConvert.SerializeObject(((OrganizationParty)party).BankAccounts);
                    await _auditClient.WriteLogEntry(AuditLogEntryAction.Get, AuditLogEntryStatus.Success, "party-bank-accounts", applicationNumber.ToString(), "Party bank accounts", listOfBankAccounts);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error: Audit write log entry");
                }
               return ((OrganizationParty)party).BankAccounts;
            }
            else
            {
                return null;
            }
        }

        public async Task<List<FinancialStatement>> GetPartyFinancialStatements(long applicationNumber, int partyId)
        {
            var party = await GetParty(applicationNumber, partyId);
            if (party != null & party is OrganizationParty)
            {
                try
                {
                    var listOfFinancialStatements = JsonConvert.SerializeObject(((OrganizationParty)party).FinancialStatements);
                    await _auditClient.WriteLogEntry(AuditLogEntryAction.Get, AuditLogEntryStatus.Success, "party-financial-statements", applicationNumber.ToString(), "Party financial statements", listOfFinancialStatements);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error: Audit write log entry");
                }
                return ((OrganizationParty)party).FinancialStatements;
            }
            else
            {
                return null;
            }
        }

        public async Task<bool?> SetSuppliersBuyersReportForParty(long applicationNumber, string customerNumber, long? reportId)
        {
            var application = _applicationRepository.GetAsync(applicationNumber).Result;
            if (application == null)
            {
                return null;
            }
            var party = _context.Parties.Where(p => p.ApplicationId == applicationNumber && p.CustomerNumber == customerNumber).FirstOrDefault();
            if (party == null)
            {
                return null;
            }
            if (party is OrganizationParty orgParty)
            {
                orgParty.SuppliersBuyersReportId = reportId;
                await _context.SaveChangesAsync();
                PublishSuppliersBuyersChangeEvent(applicationNumber, party.PartyId, customerNumber, reportId);
                return true;
            }
            else
            {
                return false;
            }
        }

        private void PublishSuppliersBuyersChangeEvent(long applicationNumber, long partyId,
            string customerNumber, long? suppliersBuyersReportId)
        {
            var appNumberString = "0000000000" + applicationNumber;
            appNumberString = appNumberString.Substring(appNumberString.Length - 10);
            var messageObjBuilder = _messageEventFactory.CreateBuilder("offer", "suppliers-and-buyers-report-changed")
                .AddBodyProperty("party-id", partyId)
                .AddBodyProperty("customer-number", customerNumber)
                .AddBodyProperty("application-number", appNumberString)
                .AddHeaderProperty("application-number", appNumberString)
                .AddHeaderProperty("username", "ALL");
            if (suppliersBuyersReportId.HasValue)
            {
                messageObjBuilder = messageObjBuilder.AddBodyProperty("suppliers-buyers-report-id", suppliersBuyersReportId);
            }

            _eventBus.Publish(messageObjBuilder.Build());
        }

        public void PublishFinancialStatementsChangeEvent(string applicationNumber, string customerNumber)
        {
            var messageObjBuilder = _messageEventFactory.CreateBuilder("offer", "financial-statements-report-added")
                .AddBodyProperty("party-id", customerNumber)
                .AddBodyProperty("application-number", applicationNumber)
                .AddHeaderProperty("application-number", applicationNumber)
                .AddHeaderProperty("username", "ALL");

            _eventBus.Publish(messageObjBuilder.Build());
        }

        public async Task<IDictionary<string, IDictionary<string, JToken>>> GetExtendedPartyData(long applicationNumber, int partyId)
        {
            var party = GetParty(applicationNumber, partyId).Result;
            await _auditClient.WriteLogEntry(AuditLogEntryAction.Get, AuditLogEntryStatus.Success, "party-extended-data", applicationNumber.ToString(), "Extended party data for " + party.PartyId, new { });
            return party.Extended;
        }

        public async Task<IDictionary<string, JToken>> GetExtendedPartyDataSection(long applicationNumber, int partyId, string sectionName)
        {
            var party = GetParty(applicationNumber, partyId).Result;

            await _auditClient.WriteLogEntry(AuditLogEntryAction.Get, AuditLogEntryStatus.Success, "party-extended-data-section", applicationNumber.ToString(), "Extended party section data for " + party.PartyId, new { });

            if (party.Extended == null || !party.Extended.ContainsKey(sectionName))
            {
                return new Dictionary<string, JToken>();
            }
            
            return party.Extended[sectionName];
        }

        public async Task<bool?> DeleteExtendedPartyDataSection(long applicationNumber, int partyId, string sectionName)
        {
            var party = GetParty(applicationNumber, partyId).Result;
            if (party == null)
            {
                return null;
            }
            if (party.Extended == null || !party.Extended.ContainsKey(sectionName))
            {
                return null;
            }

            await _auditClient.WriteLogEntry(AuditLogEntryAction.Delete, AuditLogEntryStatus.Success, "party", applicationNumber.ToString(), "Extended party data section deleted", party.Extended);

            party.Extended.Remove(sectionName);
            await _context.SaveEntitiesAsync();

            
            return true;
        }

        public async Task<Party> GetParty(long applicationNumber, int partyId, string include = null, string trim = null)
        {
            var application = _applicationRepository.GetAsync(applicationNumber).Result;
            if (application == null)
            {
                return null;
            }
            var party = await _context.Parties.Where(p =>
                                p.PartyId == partyId &&
                                p.ApplicationId == applicationNumber).FirstOrDefaultAsync();
            return party;
        }
    }
}
