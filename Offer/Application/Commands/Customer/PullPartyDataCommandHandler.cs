using MediatR;
using MicroserviceCommon.Application.Commands;
using MicroserviceCommon.Infrastructure.Idempotency;
using Offer.Domain.AggregatesModel.ApplicationAggregate;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MicroserviceCommon.ApiUtil;
using Offer.Domain.Repository;
using System.Threading;
using AuditClient;
using AuditClient.Model;

namespace Offer.API.Application.Commands
{
    public class PullPartyDataCommandHandler : IRequestHandler<PullPartyDataCommand, ApplicationView>
    {
        private readonly IApplicationRepository _applicationRepository;
        private readonly IInvolvedPartyRepository _partyRepository;
        private readonly IMasterPartyDataService _partyDataService;
        private readonly IAuditClient _auditClient;
        private readonly ApiEndPoints _apiEndPoints;
        private readonly IMediator _mediator;
        private readonly ILogger<InitiateOnlineOfferCommand> _logger;

        public PullPartyDataCommandHandler(IMediator mediator, IApplicationRepository applicationRepository,
            ILogger<InitiateOnlineOfferCommand> logger, ApiEndPoints apiEndPoints, IInvolvedPartyRepository partyRepository, 
            IMasterPartyDataService partyDataService, IAuditClient auditClient)
        {
            this._applicationRepository = applicationRepository;
            this._partyRepository = partyRepository;
            this._partyDataService = partyDataService;
            _auditClient = auditClient ?? throw new ArgumentNullException(nameof(auditClient));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _apiEndPoints = apiEndPoints ?? throw new ArgumentNullException(nameof(apiEndPoints));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        

        public async Task<ApplicationView> Handle(PullPartyDataCommand message, CancellationToken cancellationToken)
        {
            var application = await _applicationRepository.GetAsync(message.ApplicationNumber, "involved-parties");
            if (application == null)
            {
                _logger.LogError("Application {applicationNumber} not found for updating data of involved parties.", message.ApplicationNumber);
                return null;
            }

            for(int i = 0; i < application.InvolvedParties.Count; i++)
            {
                var party = application.InvolvedParties[i];
                if (party.CustomerNumber != null)
                {
                    var partyData = await _partyDataService.GetPartyData(party);
                    if (party.PartyRole == PartyRole.Customer)
                    {
                        application.CustomerName = partyData.CustomerName;
                        application.CustomerNumber = partyData.CustomerNumber;
                        
                        if (string.IsNullOrEmpty(application.CountryCode) && party != null)
                        {
                            application.CountryCode = party.CountryOfResidence;
                        }
                        if (string.IsNullOrEmpty(application.PrefferedCulture) && party != null)
                        {
                            application.PrefferedCulture = party.PreferredCulture;
                        }
                    }
                    _partyRepository.Update(partyData);
                }
            }

            _applicationRepository.Update(application);
            if (message.AuditLog)
            {
                await _auditClient.WriteLogEntry(AuditLogEntryAction.Update, AuditLogEntryStatus.Success, "involved-parties", application.ApplicationNumber, "Updating data for involved parties in application", new { });
            }
            ApplicationView result = (ApplicationView) AutoMapper.Mapper.Map(application, typeof(Domain.AggregatesModel.ApplicationAggregate.Application), typeof(ApplicationView));
            bool resultOk = await _applicationRepository.UnitOfWork.SaveEntitiesAsync();
            if (resultOk)
            {
                return result;
            }
            else
            {
                _logger.LogError("Updating data of involved parties for application {applicationNumber} has failed.", message.ApplicationNumber);
                return null;
            }
        }

    }

    public class PullPartyDataIdentifiedCommandHandler : IdentifiedCommandHandler<PullPartyDataCommand, ApplicationView>
    {
        public PullPartyDataIdentifiedCommandHandler(IMediator mediator, IRequestManager requestManager) : base(mediator, requestManager)
        {
        }

        protected override ApplicationView CreateResultForDuplicateRequest()
        {
            return null;
        }
    }
}
