using Asseco.EventBus.Abstractions;
using AuditClient;
using AuditClient.Model;
using MediatR;
using MicroserviceCommon.API.ApiUtils;
using MicroserviceCommon.Application.Commands;
using MicroserviceCommon.Extensions.Broker;
using MicroserviceCommon.Infrastructure.Idempotency;
using MicroserviceCommon.Models;
using MicroserviceCommon.Services;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Offer.Domain.AggregatesModel.ApplicationAggregate;
using Offer.Domain.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Offer.API.Application.Commands.Application
{
    public class ContinueApplicationCommandHandler : IRequestHandler<ContinueApplicationCommand, CommandStatus>
    {
        private readonly IApplicationRepository _applicationRepository;
        private readonly IInvolvedPartyRepository _involvedPartyRepository;
        private readonly IAuditClient _auditClient;
        private readonly IMediator _mediator;
        private readonly IEventBus _eventBus;
        private readonly ILogger<ContinueApplicationCommand> _logger;
        private readonly MessageEventFactory _messageEventFactory;
        private readonly IConfigurationService _configurationService;

        public ContinueApplicationCommandHandler(IApplicationRepository applicationRepository,
            IMediator mediator,
            IEventBus eventBus,
            ILogger<ContinueApplicationCommand> logger,
            MessageEventFactory messageEventFactory,
            IConfigurationService configurationService,
            IInvolvedPartyRepository involvedPartyRepository,
            IAuditClient auditClient)
        {
            _applicationRepository = applicationRepository ?? throw new ArgumentNullException(nameof(applicationRepository)); ;
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator)); ;
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus)); ;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger)); ;
            _messageEventFactory = messageEventFactory ?? throw new ArgumentNullException(nameof(messageEventFactory));
            _configurationService = configurationService ?? throw new ArgumentNullException(nameof(configurationService));
            _involvedPartyRepository = involvedPartyRepository ?? throw new ArgumentNullException(nameof(involvedPartyRepository));
            _auditClient = auditClient ?? throw new ArgumentNullException(nameof(involvedPartyRepository));
        }

        public async Task<CommandStatus> Handle(ContinueApplicationCommand request, CancellationToken cancellationToken)
        {
            var application = await _applicationRepository.GetAsync(request.ApplicationNumber, "involved-parties");

            if (application == null)
            {
                return new CommandStatus { CommandResult = StandardCommandResult.NOT_FOUND };
            }
            application.CustomerApplied = true;
            _applicationRepository.Update(application);
            var result = await _applicationRepository.UnitOfWork.SaveEntitiesAsync();

            if (request.ArrangementRequests != null)
            {
                foreach (var arrangementRequest in request.ArrangementRequests)
                {
                    var updateArrangementRequest = new UpdateArrangementRequestCommand
                    {
                        ArrangementRequest = arrangementRequest,
                        ApplicationNumber = request.ApplicationNumber,
                        ArrangementRequestId = arrangementRequest.ArrangementRequestId
                    };
                    var updateCustomerCommand = new IdentifiedCommand<UpdateArrangementRequestCommand, CommandStatus<bool>>(updateArrangementRequest, new Guid());

                    var commandResult = await _mediator.Send(updateCustomerCommand);

                    if (commandResult.CommandResult != StandardCommandResult.OK)
                    {
                        _logger.LogWarning("Unsuccessfully update ArrangementRequest from continue application with application number: {0} and arrangement request id: {1}",
                                        request.ApplicationNumber, arrangementRequest.ArrangementRequestId);
                        return new CommandStatus
                        {
                            CommandResult = StandardCommandResult.BAD_REQUEST,
                            CustomError = "Unsuccessfully update ArrangementRequest with application number: " + request.ApplicationNumber + " and arrangement request id: " + arrangementRequest.ArrangementRequestId
                        };
                    }
                }
            }

            string activeStatuses = await _configurationService.GetEffective("offer/active-statuses", "draft,active,approved,accepted");
            List<ApplicationStatus> statusList = null;
            if (!string.IsNullOrEmpty(activeStatuses))
            {
                statusList = EnumUtils.GetEnumPropertiesForListString<ApplicationStatus>(activeStatuses);
            }
            var rolesList = EnumUtils.GetEnumPropertiesForListString<PartyRole>("customer");

           
            Party party;
            if (!string.IsNullOrEmpty(application.CustomerNumber))
            {
                long partyId = application.InvolvedParties.Where(p => p.CustomerNumber.Equals(application.CustomerNumber)).FirstOrDefault().PartyId;
                party = await _involvedPartyRepository.GetPartyGeneralInformation(application.ApplicationId, int.Parse(partyId.ToString()));
            }
            else
            {
                party = application.InvolvedParties.FirstOrDefault();
            }            

            int activeOffers = 0;
            if (party.Username != null)
            {
                activeOffers = _applicationRepository.CheckExistingOffersForProspect(party.Username, party.EmailAddress, statusList, rolesList).Count;
            }            
            var messageObj = _messageEventFactory.CreateBuilder("offer", "offer-initiated");
            messageObj = messageObj.AddBodyProperty("initiator", request.Username)
                        .AddBodyProperty("channel", application.ChannelCode)
                        .AddBodyProperty("product-code", application.ProductCode)
                        .AddBodyProperty("product-name", application.ProductName)
                        .AddBodyProperty("email", party.EmailAddress)
                        .AddBodyProperty("active-offers", activeOffers)
                        .AddBodyProperty("preferential-price", application.PreferencialPrice)
                        .AddBodyProperty("term-limit-breached", application.TermLimitBreached)
                        .AddBodyProperty("amount-limit-breached", application.AmountLimitBreached)
                        .AddBodyProperty("originates-bundle", application.OriginatesBundle)
                        .AddBodyProperty("party-id", application.InvolvedParties.Where(x => x.PartyRole == PartyRole.Customer).FirstOrDefault()?.PartyId);
            if (application.Status == ApplicationStatus.Approved)
            {
                messageObj.AddBodyProperty("initiation-point", "offered");
            } 
            else
            {
                messageObj.AddBodyProperty("initiation-point", "preapproved");
            }

            if (application.LeadId != null)
            {
                var status = Enum.GetName(typeof(ApplicationStatus), application.Status);
                messageObj.AddBodyProperty("lead-id", application.LeadId)
                          .AddBodyProperty("status", status);
            }
            var customerSegment = application.InvolvedParties.Where(x => x.PartyRole == PartyRole.Customer).FirstOrDefault()?.CustomerSegment;
            if (customerSegment == null)
            {
                customerSegment = await _configurationService.GetEffective("party/default-segment/individual", "professional");
            }
            if (!string.IsNullOrEmpty(application.CustomerNumber))
            {
                messageObj = messageObj.AddBodyProperty("customer-number", application.CustomerNumber)
                                       .AddBodyProperty("customer-segment", customerSegment);
            }
            else
            {
                if (party.PartyKind == PartyKind.Individual)
                {
                    messageObj.AddBodyProperty("given-name", ((IndividualParty)party).GivenName)
                              .AddBodyProperty("family-name", ((IndividualParty)party).Surname);
                }
                else
                {
                    messageObj.AddBodyProperty("given-name", null)
                              .AddBodyProperty("family-name", null);
                }

                messageObj = messageObj.AddBodyProperty("customer-name", party.CustomerName)
                                       .AddBodyProperty("personal-identification-number", party.IdentificationNumber)
                                       .AddBodyProperty("country-code", application.CountryCode)
                                       .AddBodyProperty("customer-segment", customerSegment);
            }
            messageObj = messageObj.AddHeaderProperty("application-number", application.ApplicationNumber);
            _logger.LogInformation("Sending offer initiated event to broker on topic {BrokerTopicName} for application: {ApplicationNumber}", "offer", application.ApplicationNumber);
            _eventBus.Publish(messageObj.Build());

            if (request.AuditLog)
            {
                await _auditClient.WriteLogEntry(AuditLogEntryAction.Apply, AuditLogEntryStatus.Success, "application", application.ApplicationNumber, "Applied for application", new { });
            }


            return new CommandStatus { CommandResult = StandardCommandResult.OK };
        }

        public class ContinueApplicationIdentifiedCommandHandler : IdentifiedCommandHandler<ContinueApplicationCommand, CommandStatus>
        {
            public ContinueApplicationIdentifiedCommandHandler(IMediator mediator, IRequestManager requestManager) : base(mediator, requestManager)
            {
            }
            protected override CommandStatus CreateResultForDuplicateRequest()
            {
                return new CommandStatus { CommandResult = StandardCommandResult.OK };
            }
        }

    }
}
