using AuditClient;
using AuditClient.Model;
using MediatR;
using MicroserviceCommon.API.ApiUtils;
using MicroserviceCommon.Application.Commands;
using MicroserviceCommon.Infrastructure.Idempotency;
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

namespace Offer.API.Application.Commands
{

    public class UpdateCustomerNumberCommandHandler : IRequestHandler<UpdateCustomerNumberCommand, bool>
    {
        private readonly IApplicationRepository _applicationRepository;
        private readonly IInvolvedPartyRepository _partyRepository;
        private readonly IMediator _mediator;
        private readonly ILogger<UpdateCustomerNumberCommand> _logger;
        private readonly IConfigurationService _configurationService;
        private readonly IAuditClient _auditClient;

        public UpdateCustomerNumberCommandHandler(IMediator mediator,
            IApplicationRepository applicationRepository,
            IInvolvedPartyRepository partyRepository,
            ILogger<UpdateCustomerNumberCommand> logger,
            IConfigurationService configurationService,
            IAuditClient auditClient)
        {
            this._applicationRepository = applicationRepository;
            this._partyRepository = partyRepository;
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _configurationService = configurationService ?? throw new ArgumentNullException(nameof(configurationService));
            _auditClient = auditClient ?? throw new ArgumentNullException(nameof(auditClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<bool> Handle(UpdateCustomerNumberCommand message, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Updating customer number for application {applicationNumber}, new customer number is: {customerNumber}", message.ApplicationNumber, message.CustomerNumber);
            //var application = _applicationRepository.GetAsync(message.ApplicationNumber, "involved-parties", null).Result;
            //application.CustomerNumber = message.CustomerNumber;
            //_applicationRepository.Update(application);

            //var party = application.InvolvedParties?.Where(x => x.PartyRole == PartyRole.Customer).FirstOrDefault();
            //if (party != null)
            //{
            //    Individual customer = (Individual)party;
            //    customer.CustomerNumber = message.CustomerNumber;
            //    _partyRepository.Update(customer);
            //}

            //return await _applicationRepository.UnitOfWork.SaveEntitiesAsync();
            return await UpdateProspectOffers(message);
        }

        private async Task<bool> UpdateProspectOffers(UpdateCustomerNumberCommand message)
        {
            _logger.LogInformation("Updating active prospect offers"); // with customer number {customerNumber}", message.CustomerNumber);
            string activeStatuses = await _configurationService.GetEffective("offer/active-statuses", "draft");
            List<ApplicationStatus> activeStatusList = EnumUtils.GetEnumPropertiesForListString<ApplicationStatus>(activeStatuses);
            var applications = _applicationRepository.GetProspectOffers(message.Username, activeStatusList);
            foreach (Domain.AggregatesModel.ApplicationAggregate.Application app in applications)
            {
                if (string.IsNullOrEmpty(app.CustomerNumber))
                {
                    app.CustomerNumber = message.CustomerNumber;
                    _applicationRepository.Update(app);
                    _logger.LogInformation("Customer number in application {applicationNumber} updated to {customerNumber}.", app.ApplicationNumber, message.CustomerNumber);
                }
                else
                {
                    _logger.LogWarning("Customer number in aplication {aplicationNumber} already exist! Found: {oldCustomerNumber}, should be: {customerNumber}",
                                        app.ApplicationNumber, app.CustomerNumber, message.CustomerNumber);
                }

                var party = app.InvolvedParties?.Where(x => x.PartyRole == PartyRole.Customer).FirstOrDefault();
                if (party != null)
                {
                    IndividualParty customer = (IndividualParty)party;
                    if (string.IsNullOrEmpty(customer.CustomerNumber))
                    {
                        customer.CustomerNumber = message.CustomerNumber;
                        _partyRepository.Update(customer);
                    }
                    else
                    {
                        _logger.LogWarning("Customer number for involved party in aplication {aplicationNumber} already exist! Found: {oldCustomerNumber}, should be: {customerNumber}",
                                        app.ApplicationNumber, customer.CustomerNumber, message.CustomerNumber);
                    }
                }
                if (message.AuditLog)
                {
                    await _auditClient.WriteLogEntry(AuditLogEntryAction.Update, AuditLogEntryStatus.Success, "customer-number", app.ApplicationNumber, "Updated customer number " + party.CustomerNumber, new { });
                }
            }
            
            return await _applicationRepository.UnitOfWork.SaveEntitiesAsync();
        }
    }
    public class UpdateCustomerNumberIdentifiedCommandHandler : IdentifiedCommandHandler<UpdateCustomerNumberCommand, bool>
    {
        public UpdateCustomerNumberIdentifiedCommandHandler(IMediator mediator, IRequestManager requestManager) : base(mediator, requestManager)
        {
        }

        protected override bool CreateResultForDuplicateRequest()
        {
            return true;
        }
    }

}
