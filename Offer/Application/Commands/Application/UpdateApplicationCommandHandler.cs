using AuditClient;
using AuditClient.Model;
using MediatR;
using MicroserviceCommon.Application.Commands;
using MicroserviceCommon.Infrastructure.Idempotency;
using MicroserviceCommon.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Offer.Domain.AggregatesModel.ApplicationAggregate;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Offer.API.Application.Commands
{
    public class UpdateApplicationCommandHandler : IRequestHandler<UpdateApplicationCommand, CommandStatus>
    {
        private readonly IApplicationRepository _applicationRepository;
        private readonly ILogger<UpdateApplicationCommand> _logger;
        private readonly IAuditClient _auditClient;

        public UpdateApplicationCommandHandler(
            IApplicationRepository applicationRepository,
            ILogger<UpdateApplicationCommand> logger,
            IAuditClient auditClient
            )
        {
            this._applicationRepository = applicationRepository;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _auditClient = auditClient ?? throw new ArgumentNullException(nameof(auditClient)); ;
        }

        public async Task<CommandStatus> Handle(UpdateApplicationCommand message, CancellationToken cancellationToken)
        {
            try
            {
                var application = await _applicationRepository.GetAsync(message.ApplicationNumber);
                if (application == null)
                {
                    return new CommandStatus { CommandResult = StandardCommandResult.NOT_FOUND };
                }
                application.AmountLimitBreached = message.AmountLimitBreached;
                application.ProductCode = message.ProductCode;
                application.ProductName = message.ProductName;
                application.CustomerNumber = message.CustomerNumber;
                application.CustomerName = message.CustomerName;
                application.CustomerSegment = message.CustomerSegment;
                application.OrganizationUnitCode = message.OrganizationUnitCode;
                application.ChannelCode = message.ChannelCode;
                application.PortfolioId = message.PortfolioId;
                application.CampaignCode = message.CampaignCode;
                application.DecisionNumber = message.DecisionNumber;
                application.Initiator = message.Initiator;
                application.CountryCode = message.CountryCode;
                application.PreferencialPrice = message.PreferencialPrice;
                application.PrefferedCulture = message.PrefferedCulture;
                application.RiskScore = message.RiskScore;
                application.StatusInformation.Description = message.StatusInformation?.Description;
                application.StatusInformation.Title = message.StatusInformation?.Title;
                application.StatusInformation.Html = message.StatusInformation?.Html;
                application.TermLimitBreached = message.TermLimitBreached;
                application.SigningOption = message.SigningOption;
               _applicationRepository.Update(application);
                var result = await _applicationRepository.UnitOfWork.SaveEntitiesAsync();

                if (message.AuditLog)
                {
                    await _auditClient.WriteLogEntry(AuditLogEntryAction.Update, AuditLogEntryStatus.Success, "application", message.ApplicationNumber.ToString(), "Application details updated", new { });
                }

                return new CommandStatus { CommandResult = StandardCommandResult.OK };
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Got exception while updating application {ApplicationNumber}", message.ApplicationNumber);
                return new CommandStatus { CommandResult = StandardCommandResult.INTERNAL_ERROR, Exception = e };
            }
        }
    }
    public class UpdateApplicationIdentifiedCommandHandler : IdentifiedCommandHandler<UpdateApplicationCommand, CommandStatus>
    {
        public UpdateApplicationIdentifiedCommandHandler(IMediator mediator, IRequestManager requestManager) : base(mediator, requestManager)
        {
        }

        protected override CommandStatus CreateResultForDuplicateRequest()
        {
            return new CommandStatus { CommandResult = StandardCommandResult.OK };
        }
    }
}
