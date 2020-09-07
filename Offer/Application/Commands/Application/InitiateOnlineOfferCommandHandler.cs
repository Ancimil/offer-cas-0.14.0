#region Imports
using Asseco.EventBus.Abstractions;
using MediatR;
using MicroserviceCommon.Application.Commands;
using MicroserviceCommon.Infrastructure.Idempotency;
using Offer.Domain.AggregatesModel.ApplicationAggregate;
using Offer.Domain.AggregatesModel.ApplicationAggregate.ProductModel;
using Offer.Domain.Calculations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AssecoCurrencyConvertion;
using System.Globalization;
using Microsoft.Extensions.Logging;
using MicroserviceCommon.API.ApiUtils;
using Offer.Domain.Exceptions;
using MicroserviceCommon.Services;
using MicroserviceCommon.Extensions.Broker;
using AutoMapper;
using Offer.Domain.Utils;
using OfferApplication = Offer.Domain.AggregatesModel.ApplicationAggregate.Application;
using System.Threading;
using PriceCalculation.Calculations;
using AuditClient;
using AuditClient.Model;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
#endregion

namespace Offer.API.Application.Commands
{
    public class InitiateOnlineOfferCommandHandler : IRequestHandler<InitiateOnlineOfferCommand, IntialOnlineOfferCommandResult>
    {
        private readonly IApplicationRepository _applicationRepository;
        private readonly IMediator _mediator;
        private readonly IEventBus _eventBus;
        private readonly ILogger<InitiateOnlineOfferCommand> _logger;
        private readonly OfferPriceCalculation _priceCalculator;
        private readonly IConfigurationService _configurationService;
        private readonly IContentService _contentService;
        private readonly IMasterPartyDataService _partyDataService;
        private readonly MessageEventFactory _messageEventFactory;
        private readonly ApplicationDocumentsResolver _documentsResolver;
        private readonly OfferUtility _offerUtility;
        private readonly ArrangementRequestFactory _requestFactory;
        private readonly IAuditClient _auditClient;

        public InitiateOnlineOfferCommandHandler(IMediator mediator,
            IApplicationRepository applicationRepository,
            IEventBus eventBus,
            ILogger<InitiateOnlineOfferCommand> logger,
            OfferPriceCalculation priceCalculator,
            IConfigurationService configurationService,
            IContentService contentService,
            MessageEventFactory messageEventFactory,
            IMasterPartyDataService partyDataService,
            ApplicationDocumentsResolver applicationDocumentsResolver,
            OfferUtility offerUtility,
            ArrangementRequestFactory requestFactory,
            IAuditClient auditClient
            )
        {
            _applicationRepository = applicationRepository;
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _priceCalculator = priceCalculator;
            _messageEventFactory = messageEventFactory;
            _configurationService = configurationService ?? throw new ArgumentNullException(nameof(configurationService));
            _contentService = contentService ?? throw new ArgumentNullException(nameof(contentService));
            _partyDataService = partyDataService;
            _documentsResolver = applicationDocumentsResolver ?? throw new ArgumentNullException(nameof(applicationDocumentsResolver));
            _offerUtility = offerUtility ?? throw new ArgumentNullException(nameof(offerUtility));
            _requestFactory = requestFactory ?? throw new ArgumentNullException(nameof(requestFactory));
            _auditClient = auditClient ?? throw new ArgumentNullException(nameof(auditClient));
        }

        private static string InitiationLoader()
        {
            System.Text.StringBuilder loader = new System.Text.StringBuilder();
            loader.AppendLine("<br><br><br>");
            loader.AppendLine("<svg xmlns=\"http://www.w3.org/2000/svg\" xmlns:xlink=\"http://www.w3.org/1999/xlink\" style=\"margin: auto; background: none; display: block; shape-rendering: auto;\" width=\"200px\" height=\"200px\" viewBox=\"0 0 100 100\" preserveAspectRatio=\"xMidYMid\">");
            loader.AppendLine("<g transform=\"translate(26.666666666666668,26.666666666666668)\">");
            loader.AppendLine("  <rect x=\"-20\" y=\"-20\" width=\"40\" height=\"40\" fill=\"#eb0255\" transform=\"scale(1.1488 1.1488)\">");
            loader.AppendLine("    <animateTransform attributeName=\"transform\" type=\"scale\" repeatCount=\"indefinite\" dur=\"1s\" keyTimes=\"0;1\" values=\"1.1500000000000001;1\" begin=\"-0.3s\"></animateTransform>");
            loader.AppendLine("  </rect>");
            loader.AppendLine("</g>");
            loader.AppendLine("<g transform=\"translate(73.33333333333333,26.666666666666668)\">");
            loader.AppendLine("  <rect x=\"-20\" y=\"-20\" width=\"40\" height=\"40\" fill=\"#AAA\" transform=\"scale(1.0138 1.0138)\">");
            loader.AppendLine("    <animateTransform attributeName=\"transform\" type=\"scale\" repeatCount=\"indefinite\" dur=\"1s\" keyTimes=\"0;1\" values=\"1.1500000000000001;1\" begin=\"-0.2s\"></animateTransform>");
            loader.AppendLine("  </rect>");
            loader.AppendLine("</g>");
            loader.AppendLine("<g transform=\"translate(26.666666666666668,73.33333333333333)\">");
            loader.AppendLine("  <rect x=\"-20\" y=\"-20\" width=\"40\" height=\"40\" fill=\"#55c45f\" transform=\"scale(1.0438 1.0438)\">");
            loader.AppendLine("    <animateTransform attributeName=\"transform\" type=\"scale\" repeatCount=\"indefinite\" dur=\"1s\" keyTimes=\"0;1\" values=\"1.1500000000000001;1\" begin=\"0s\"></animateTransform>");
            loader.AppendLine("  </rect>");
            loader.AppendLine("</g>");
            loader.AppendLine("<g transform=\"translate(73.33333333333333,73.33333333333333)\">");
            loader.AppendLine("  <rect x=\"-20\" y=\"-20\" width=\"40\" height=\"40\" fill=\"#209ac7\" transform=\"scale(1.0288 1.0288)\">");
            loader.AppendLine("    <animateTransform attributeName=\"transform\" type=\"scale\" repeatCount=\"indefinite\" dur=\"1s\" keyTimes=\"0;1\" values=\"1.1500000000000001;1\" begin=\"-0.1s\"></animateTransform>");
            loader.AppendLine("  </rect>");
            loader.AppendLine("</g>");

            return loader.ToString();
        }

        public async Task<IntialOnlineOfferCommandResult> Handle(InitiateOnlineOfferCommand message, CancellationToken cancellationToken)
        {            
            Console.WriteLine("Offer - IntialOnlineOfferCommandResult - Handle start !!!");
            var dateBeforeCall = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff",
                                            CultureInfo.InvariantCulture);
            Console.WriteLine("START {0}", dateBeforeCall);
            string defaultLoader = await _configurationService.GetEffective("offer/html/default-loader", "");
            var application = new OfferApplication
            {
                ArrangementRequests = new List<ArrangementRequest>(),
                Status = ApplicationStatus.Draft,
                StatusInformation = new StatusInformation() { Title = "Initiating...",  Description = "We are initiating the application. Please wait.", Html = defaultLoader },
                RequestDate = DateTime.UtcNow,
                ChannelCode = await AppendChannel(message),
                ProductCode = message.ProductCode,
                ProductName = message.ProductName,
                Initiator = message.Username,
                CollateralModel = message.CollateralModel,
                Phase = EnumUtils.ToEnumString(ApplicationStatus.Draft)
            };

            application.LeadId = message.LeadId ?? application.LeadId;

            if (message.LeadId != null)
            {
                var app = await _applicationRepository.GetApplicationByLeadId(message.LeadId);
                if (app != null)
                {
                    return new IntialOnlineOfferCommandResult { ApplicationNumber = "", Result = CommandResult.LEAD_ALREADY_EXIST };
                }
            }

            if (!string.IsNullOrEmpty(message.CustomerNumber))
            {
                application.CustomerNumber = message.CustomerNumber;
            }
            else
            {
                application.CustomerNumber = "";
                application.CustomerName = message.GivenName + " " + message.Surname;
                application.CountryCode = message.CountryCode;
                application.PrefferedCulture = message.PrefferedCulture;
            }

            int activeOffers = 0;
            try
            {
                activeOffers = await HandleParties(application, message);
                var financialStatement = new IdentifiedCommand<UpdateFinancialStatementsCommand, bool>(new UpdateFinancialStatementsCommand(application), new Guid());
                bool commandResult = await _mediator.Send(financialStatement);
            }
            catch (InvalidCastException e)
            {
                _logger.LogError(e, "An error occurred while handling parties for new application");
                throw e;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An error occurred while handling involved parties for new application");
                return new IntialOnlineOfferCommandResult { ApplicationNumber = "", Result = CommandResult.INVALID_CALCULATION };
            }
            if (!string.IsNullOrEmpty(application.ProductCode))
            {
                ArrangementRequest arrangementRequest;
                try
                {
                    var handlingResponse = await HandleArrangementRequests(application, message);
                    arrangementRequest = handlingResponse.MainRequest;
                    List<string> productsList = new List<string>();
                    foreach (var arr in application.ArrangementRequests)
                    {
                        if (!productsList.Contains(arr.ProductCode))
                        {
                            productsList.Add(arr.ProductCode);
                        }
                        if (!string.IsNullOrEmpty(arr.ProductSnapshot.RelatedProducts) && application.ProductCode == arr.ProductCode)
                        {
                            productsList.AddRange(arr.ProductSnapshot.RelatedProducts.Split(",").Where(x => !productsList.Contains(x)).ToList());
                        }
                        //if (arr.ProductSnapshot.BundledProducts != null)
                        //{
                        //    list.AddRange(arr.ProductSnapshot.BundledProducts.Where(b => b.ProductKind != ProductKinds.AbstractProduct).Select(b => b.ProductCode).
                        //        Where(x => !list.Contains(x)).ToList());
                        //}
                    }
                    application.AvailableProducts = productsList;
                }
                catch (InvalidCalculationException e)
                {
                    throw e;
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "An error occurred while adding arrangement requests for new application");
                    return new IntialOnlineOfferCommandResult { ApplicationNumber = "", Result = CommandResult.INVALID_CALCULATION };
                }
                if (string.IsNullOrEmpty(application.ProductName))
                {
                    application.ProductName = application.ArrangementRequests
                        .Where(r => r.ProductCode == application.ProductCode)
                        .Select(r => r.ProductName)
                        .FirstOrDefault();
                }

                application.PreferencialPrice = _priceCalculator.HasPreferentialPrice(arrangementRequest.Conditions);
            }            

            OfferApplication addedApplication = _applicationRepository.Add(application);
            try
            {
                await _applicationRepository.UnitOfWork.SaveEntitiesAsync();
                await _auditClient.WriteLogEntry(AuditLogEntryAction.Apply, AuditLogEntryStatus.Success, "application", application.ApplicationNumber, "Applied for product", new { });
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An error occurred while saving application");
                await _auditClient.WriteLogEntry(AuditLogEntryAction.Apply, AuditLogEntryStatus.Error, "application", application.ApplicationNumber, "An error occurred while applying for product");
                return new IntialOnlineOfferCommandResult { ApplicationNumber = "", Result = CommandResult.INVALID_CALCULATION };
            }
            // await HandleApplicationDocuments(application, message);
            await _applicationRepository.UnitOfWork.SaveEntitiesAsync();
            /////////////////////////////////////////////
            Console.WriteLine("Offer - IntialOnlineOfferCommandResult - CreateContentFolders start !!!");
            var dateBeforeCallStart = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff",
                                            CultureInfo.InvariantCulture);

            var createFolders = await _configurationService.GetEffective("offer/create-content-folders-automatically", "true");
            
            if (createFolders.Equals("true"))
            {
                Console.WriteLine("START CreateContentFolders {0}", dateBeforeCallStart);

                await CreateContentFolders(application);
                var dateAfterCallEnd = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff",
                                                CultureInfo.InvariantCulture); ;
                Console.WriteLine("END CreateContentFolders: {0} ", dateAfterCallEnd);

                double milDiffCreateContentFolders = DateTime.Parse(dateAfterCallEnd).Subtract(DateTime.Parse(dateBeforeCallStart)).TotalMilliseconds;
                Console.WriteLine("Offer - CreateContentFolders - COUNT: {0} !!!", milDiffCreateContentFolders);
            }
            ///////////////////////////////////////
            application.Documents = application.Documents ?? new List<ApplicationDocument>();
            

            application = await _documentsResolver.ResolveDocuments(application);
            await _applicationRepository.UnitOfWork.SaveEntitiesAsync();

            //////////////////////
            ///
            if (createFolders.Equals("true"))
            {
                Console.WriteLine("Offer - IntialOnlineOfferCommandResult - CreateContentFolders start !!!");
                var dateBeforeCallDocumentsFolders = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff",
                                                CultureInfo.InvariantCulture);
                Console.WriteLine("START CreateApplicationDocumentsFolders {0}", dateBeforeCallDocumentsFolders);



                await _documentsResolver.CreateApplicationDocumentsFolders();
                var dateAfterDocumentsFoldersEnd = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff",
                                               CultureInfo.InvariantCulture); ;
                Console.WriteLine("END CreateApplicationDocumentsFolders: {0} ", dateAfterDocumentsFoldersEnd);

                double milDiffCreateDocumentsFolders = DateTime.Parse(dateAfterDocumentsFoldersEnd).Subtract(DateTime.Parse(dateBeforeCallDocumentsFolders)).TotalMilliseconds;
                Console.WriteLine("Offer - CreateContentFolders - COUNT: {0} !!!", milDiffCreateDocumentsFolders);
                ///////////////////////////////////////
            }
            var messageObj = _messageEventFactory.CreateBuilder("offer", "offer-initiated");
            messageObj = messageObj.AddBodyProperty("initiator", message.Username)
                        .AddBodyProperty("channel", application.ChannelCode)
                        .AddBodyProperty("product-code", application.ProductCode)
                        .AddBodyProperty("product-name", application.ProductName)
                        .AddBodyProperty("email", message.EmailAddress)
                        .AddBodyProperty("mobile-phone", message.MobilePhone)
                        .AddBodyProperty("active-offers", activeOffers)
                        .AddBodyProperty("preferential-price", application.PreferencialPrice)
                        .AddBodyProperty("term-limit-breached", application.TermLimitBreached)
                        .AddBodyProperty("amount-limit-breached", application.AmountLimitBreached)
                        .AddBodyProperty("originates-bundle", application.OriginatesBundle)
                        .AddBodyProperty("party-id", application.InvolvedParties.Where(x => x.PartyRole == PartyRole.Customer).FirstOrDefault()?.PartyId)
                        .AddBodyProperty("is-proposal", message.IsProposal);
            if (application.LeadId != null)
            {
                var stat = Enum.GetName(typeof(ApplicationStatus), application.Status);
                messageObj.AddBodyProperty("lead-id", application.LeadId)
                          .AddBodyProperty("status", stat)
                          .AddBodyProperty("initiation-point", "targeted-by-campaign");
            }
            else if (message.IsProposal)
            {
                messageObj.AddBodyProperty("initiation-point", "proposal-initiated");
            }
            else
            {
                messageObj.AddBodyProperty("initiation-point", "applied");
            }
            var customerSegment = application.InvolvedParties.Where(x => x.PartyRole == PartyRole.Customer).FirstOrDefault()?.CustomerSegment;
            if (customerSegment == null)
            {
                customerSegment = await _configurationService.GetEffective("party/default-segment/individual", "professional");
            }
            if (!string.IsNullOrEmpty(message.CustomerNumber))
            {
                messageObj = messageObj.AddBodyProperty("customer-number", application.CustomerNumber)
                                       .AddBodyProperty("customer-segment", customerSegment);
            }
            else
            {
                messageObj = messageObj.AddBodyProperty("customer-name", application.CustomerName)
                                       .AddBodyProperty("given-name", message.GivenName)
                                       .AddBodyProperty("family-name", message.Surname)
                                       .AddBodyProperty("personal-identification-number", message.IdentificationNumber)
                                       .AddBodyProperty("country-code", message.CountryCode)
                                       .AddBodyProperty("customer-segment", customerSegment);
            }
            messageObj = messageObj.AddHeaderProperty("application-number", addedApplication.ApplicationNumber);
            // _logger.LogInformation("Sending offer initiated event to broker on topic {BrokerTopicName} for application: {ApplicationNumber}", "offer", addedApplication.ApplicationNumber);
            _eventBus.Publish(messageObj.Build());
            //////////////////
            var dateAfterCall = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff",
                                            CultureInfo.InvariantCulture); ;
            Console.WriteLine("Offer - IntialOnlineOfferCommandResult - Handle END {0} ", dateAfterCall);
            double milDiff = DateTime.Parse(dateAfterCall).Subtract(DateTime.Parse(dateBeforeCall)).TotalMilliseconds;
            Console.WriteLine("Offer - IntialOnlineOfferCommandResult - Handle - COUNT: {0}", milDiff);
            /////////////
            ///
            return new IntialOnlineOfferCommandResult() { ApplicationNumber = addedApplication.ApplicationNumber, Result = CommandResult.OK };
        }

        private async Task<int> HandleParties(OfferApplication application, InitiateOnlineOfferCommand message)
        {
            // Checking active offers
            string activeStatuses = await _configurationService.GetEffective("offer/active-statuses", "draft,active,approved,accepted");
            List<ApplicationStatus> statusList = null;
            if (!string.IsNullOrEmpty(activeStatuses))
            {
                statusList = EnumUtils.GetEnumPropertiesForListString<ApplicationStatus>(activeStatuses);
            }
            var rolesList = EnumUtils.GetEnumPropertiesForListString<PartyRole>("customer");
            Party party;

            int activeOffers;
            if (string.IsNullOrEmpty(message.CustomerNumber))
            {
                if (string.IsNullOrEmpty(message.EmailAddress))
                {
                    activeOffers = 0;
                }
                else
                {
                    activeOffers = _applicationRepository.CheckExistingOffersForProspect(message.Username, message.EmailAddress, statusList, rolesList).Count;
                }
                party = new IndividualParty
                {
                    GivenName = message.GivenName,
                    Surname = message.Surname,
                    ParentName = message.ParentName,
                    CustomerName = message.GivenName + " " + message.Surname,
                    IdentificationNumberKind = message.IdentificationNumberKind,
                    IdentificationNumber = message.IdentificationNumber,
                    EmailAddress = message.EmailAddress,
                    MobilePhone = message.MobilePhone,
                    Gender = Gender.Unknown,
                    PartyRole = PartyRole.Customer,
                    Username = message.Username
                };

                party.LegalAddress = new PostalAddress
                {
                    Coordinates = new Coordinates()
                };
                party.ContactAddress = new PostalAddress
                {
                    Coordinates = new Coordinates()
                };
                application.InvolvedParties = new List<Party> { party };
                _logger.LogInformation("New customer: {givenName} {surname} ({emailAddress})", message.GivenName, message.Surname, message.EmailAddress);
            }
            else
            {
                activeOffers = _applicationRepository.CheckExistingOffersForCustomer(message.CustomerNumber, statusList, rolesList).Count;
                if (message.PartyKind == PartyKind.Organization)
                {
                    party = new OrganizationParty
                    {
                        CustomerNumber = message.CustomerNumber,
                        EmailAddress = message.EmailAddress,
                        PartyKind = PartyKind.Organization
                    };
                }
                else
                {
                    party = new IndividualParty
                    {
                        CustomerNumber = message.CustomerNumber,
                        EmailAddress = message.EmailAddress
                    };
                }

                var partyData = await _partyDataService.GetPartyData(party);
                string defaultOrganizationUnit = await _configurationService.GetEffective("offer/default-organization-unit", null);
                if (!string.IsNullOrEmpty(message.AgentOrganizationUnit))
                {
                    application.OrganizationUnitCode = message.AgentOrganizationUnit;
                }
                else
                {
                    application.OrganizationUnitCode = partyData.OrganizationUnitCode ?? defaultOrganizationUnit;
                }

                application.InvolvedParties = new List<Party> { party };

                //if (partyData is OrganizationParty orgParty && orgParty.Relationships.Count() > 0)
                //{
                //    List<Relationships> listRelationships = orgParty.Relationships;
                //    foreach (var item in listRelationships)
                //    {
                //        Party customerRepresentative;
                //        if (item.ToParty.Kind == PartyKind.Individual)
                //        {
                //            customerRepresentative = new IndividualParty
                //            {
                //                CustomerNumber = item.ToParty.Number.ToString(),
                //                PartyRole = PartyRole.AuthorizedPerson
                //            };
                //        }
                //        else
                //        {
                //            customerRepresentative = new OrganizationParty
                //            {
                //                CustomerNumber = item.ToParty.Number.ToString(),
                //                PartyRole = PartyRole.AuthorizedPerson
                //            };
                //        }

                //        var customerRelationship = await _partyDataService.GetPartyData(customerRepresentative);
                //        application.InvolvedParties.Add(customerRepresentative);
                //    }
                //}

                if (party.PartyRole == PartyRole.Customer)
                {
                    application.CustomerName = partyData.CustomerName;
                    application.CustomerNumber = partyData.CustomerNumber;
                    party.EmailAddress = partyData.EmailAddress;


                    if (string.IsNullOrEmpty(application.CountryCode) && party != null)
                    {
                        application.CountryCode = party.CountryOfResidence;
                    }
                    if (string.IsNullOrEmpty(application.PrefferedCulture) && party != null)
                    {
                        application.PrefferedCulture = party.PreferredCulture;
                    }
                }

                _logger.LogInformation("Existing customer: {customerNumber} ({emailAddress})", party.CustomerNumber, party.EmailAddress);
            }

            return activeOffers;
        }

        private async Task<bool> HandleApplicationDocuments(OfferApplication application, InitiateOnlineOfferCommand message)
        {
            #region Adding documents for application
            List<ApplicationDocument> applicationDocuments = new List<ApplicationDocument>();
            application.Documents = new List<ApplicationDocument>();
            var getProductDocumentation = new IdentifiedCommand<GetProductDocumentationCommand, List<ProductDocumentation>>(
                new GetProductDocumentationCommand { ProductCode = message.ProductCode }, new Guid());
            List<ProductDocumentation> productDocuments = await _mediator.Send(getProductDocumentation);
            foreach (var document in productDocuments)
            {
                if (document.DocumentContextKind.Equals(DocumentContextKind.ApplicationEnum))
                {
                    var appDocument = Mapper.Map<ProductDocumentation, ApplicationDocument>(document);
                    applicationDocuments.Add(appDocument);
                }
                else if (document.DocumentContextKind.Equals(DocumentContextKind.PartyEnum) && application.InvolvedParties.Count() > 0)
                {
                    var customer = application.InvolvedParties.FirstOrDefault(c => c.PartyRole.Equals(PartyRole.Customer));
                    var appDocument = Mapper.Map<ProductDocumentation, ApplicationDocument>(document);
                    if (customer != null)
                    {
                        appDocument.PartyId = customer.PartyId;
                    }

                    if (document.PartyRole.Equals(Domain.AggregatesModel.ApplicationAggregate.ProductModel.PartyRoleEnum.CustomerEnum) ||
                        (document.PartyRole.Equals(Domain.AggregatesModel.ApplicationAggregate.ProductModel.PartyRoleEnum.NewCustomerEnum) && string.IsNullOrEmpty(message.CustomerNumber)))
                    {
                        applicationDocuments.Add(appDocument);
                    }
                }
            }
            application.Documents.AddRange(applicationDocuments);
            #endregion
            return true;
        }

        private async Task<bool> CreateContentFolders(OfferApplication application)
        {
            try
            {
                await _contentService.CreateFolder("" + application.ApplicationNumber, "/offer", "folder", "application-documents-folder");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An error occurred while creating application folder for application {ApplicationNumber}", application.ApplicationNumber);
             }

            try
            {
                if (application.Documents != null)
                {
                    _logger.LogDebug("Create slots ");
                    var applicationDocuments = application.Documents.ToList();
                    foreach (var document in applicationDocuments)
                    {
                        var path = "/offer/" + document.ApplicationNumber;
                        try
                        {
                            await _contentService.CreateFolder("" + document.DocumentId, path, "folder", "generic-folder");
                        }
                        catch (Exception e)
                        {
                            _logger.LogError(e, "An error occurred while creating folder for document {DocumentId} on path {ContentPath}",
                                document.DocumentId, path);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An error occurred while creating documents folder for application {ApplicationNumber}", application.ApplicationNumber);
            }
            return true;
        }

        private async Task<ArrangementRequestHandlingResponse> HandleArrangementRequests(OfferApplication application,
            InitiateOnlineOfferCommand message)
        {
            #region handle main product inclusion
            var parameters = Mapper.Map<InitiateOnlineOfferCommand, ArrangementRequestInitializationParameters>(message);
            var factoryResponse = await _requestFactory.AddToApplication(application, message.ProductCode, parameters);
            application.OriginatesBundle = factoryResponse.Any(r => r.BundleInfo != null);
            var arrangementRequest = factoryResponse.FirstOrDefault();
            arrangementRequest.IsOptional = false;
            #endregion

            if (arrangementRequest.IsFinanceService())
            {
                #region Check calculation
                var annuityIsDifferent = false;
                var error = false;
                try
                {
                    annuityIsDifferent = Math.Abs(Math.Round((((TermLoanRequest)arrangementRequest).Annuity - message.Annuity), 0)) > 0;
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "An unknown error occured while checking calculated and provided annuity");
                    error = true;
                }
                if (message.Annuity > 0 && !error && annuityIsDifferent)
                {
                    var e = new InvalidCalculationException("Received and calculated annuity are not equal");
                    _logger.LogError(e, "Error in calculation: Received and calculated annuity are not equal");
                    throw e;
                }
                #endregion
                // Add additional product for new customer
                if (string.IsNullOrEmpty(application.CustomerNumber))
                {
                    var applyDefinition = Mapper.Map<InitiateOnlineOfferCommand, ApplyForProductDefinition>(message);
                    var resolved = await _offerUtility.ResolveAdditionalProduct(application, applyDefinition);
                    if (resolved != null)
                    {
                        var additional = resolved.FirstOrDefault();
                        if (additional != null)
                        {
                            additional.IsOptional = false;
                        }
                        factoryResponse.AddRange(resolved);
                    }
                }
            }

            application.TermLimitBreached = TermLimitsBreached(message.Term, arrangementRequest.ProductSnapshot);
            application.AmountLimitBreached = AmountLimitsBreached(message.Amount, message.Currency, arrangementRequest.ProductSnapshot);
            arrangementRequest.OverrideProductLimits = false;
            if (application.TermLimitBreached || application.AmountLimitBreached)
            {
                arrangementRequest.OverrideProductLimits = true;
            }

            if (message.IsProposal)
            {
                if (!string.IsNullOrEmpty(arrangementRequest.ProductSnapshot.ProposalValidityPeriod))
                {
                    var validityProposalPeriod = arrangementRequest.ProductSnapshot.ProposalValidityPeriod;

                    DateTime expirationDate = Utility.GetEndDateFromPeriod(validityProposalPeriod, DateTime.UtcNow);
                    application.ExpirationDate = expirationDate;
                }
            }
            else if (!String.IsNullOrEmpty(arrangementRequest.ProductSnapshot.OfferValidityPeriod))
            {
                var validityPeriod = arrangementRequest.ProductSnapshot.OfferValidityPeriod;

                DateTime expirationDate = Utility.GetEndDateFromPeriod(validityPeriod, DateTime.UtcNow);
                application.ExpirationDate = expirationDate;
            }
            return new ArrangementRequestHandlingResponse
            {
                FactoryResponse = factoryResponse,
                MainRequest = arrangementRequest
            };
        }

        private async Task<string> AppendChannel(InitiateOnlineOfferCommand message)
        {
            if (message.IsCustomer || string.IsNullOrEmpty(message.Channel))
            {
                return await _configurationService.GetEffective("product/online-channel-code", "web");
            }
            else
            {
                return message.Channel;
            }
        }

        private bool TermLimitsBreached(string term, ProductSnapshot productData)
        {
            if (string.IsNullOrEmpty(term) || term == "0")
            {
                return false;
            }
            var termInMonths = Utility.GetMonthsFromPeriod(term);
            var breached = false;
            if (!string.IsNullOrEmpty(productData.MinimalTerm))
            {
                var lowerTermInMonths = Utility.GetMonthsFromPeriod(productData.MinimalTerm);
                breached = termInMonths < lowerTermInMonths;
            }
            if (!breached && !string.IsNullOrEmpty(productData.MaximalTerm))
            {
                var upperTermInMonths = Utility.GetMonthsFromPeriod(productData.MaximalTerm);
                breached = termInMonths > upperTermInMonths;
            }
            return breached;
        }

        private bool AmountLimitsBreached(decimal amount, string currency, ProductSnapshot productData)
        {
            if (productData.MaximalAmount != null)
            {
                var maxLimitInCurrency = productData.MaximalAmount.Amount;
                if (!string.IsNullOrEmpty(currency) && !productData.MaximalAmount.Code.Equals(currency))
                {
                    var conversionMethod = _configurationService.GetEffective("offer/fee-currency-conversion-method", "Buy to middle").Result;
                    CurrencyConverter currencyConverter = new CurrencyConverter();
                    maxLimitInCurrency = currencyConverter.CurrencyConvert(productData.MaximalAmount.Amount, productData.MaximalAmount.Code, currency, DateTime.Today.ToString("o", CultureInfo.InvariantCulture), conversionMethod);
                }
                if (amount > maxLimitInCurrency)
                {
                    return true;
                }
            }
            if (productData.MinimalAmount != null)
            {
                var minLimitInCurrency = productData.MinimalAmount.Amount;
                if (!string.IsNullOrEmpty(currency) && !productData.MinimalAmount.Code.Equals(currency))
                {
                    var conversionMethod = _configurationService.GetEffective("offer/fee-currency-conversion-method", "Buy to middle").Result;
                    CurrencyConverter currencyConverter = new CurrencyConverter();
                    minLimitInCurrency = currencyConverter.CurrencyConvert(productData.MinimalAmount.Amount, productData.MinimalAmount.Code, currency, DateTime.Today.ToString("o", CultureInfo.InvariantCulture), conversionMethod);
                }
                if (amount < minLimitInCurrency)
                {
                    return true;
                }
            }

            return false;
        }
    }

    public class InitiateOnlineOfferIdentifiedCommandHandler : IdentifiedCommandHandler<InitiateOnlineOfferCommand, IntialOnlineOfferCommandResult>
    {
        public InitiateOnlineOfferIdentifiedCommandHandler(IMediator mediator, IRequestManager requestManager) : base(mediator, requestManager)
        {
        }

        protected override IntialOnlineOfferCommandResult CreateResultForDuplicateRequest()
        {
            return null;
        }
    }

    class ArrangementRequestHandlingResponse
    {
        public List<ArrangementRequest> FactoryResponse { get; set; }
        public ArrangementRequest MainRequest { get; set; }
    }

}
