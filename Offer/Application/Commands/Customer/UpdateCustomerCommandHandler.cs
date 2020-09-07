using System;
using System.Threading.Tasks;
using MediatR;
using MicroserviceCommon.Application.Commands;
using MicroserviceCommon.Infrastructure.Idempotency;
using Offer.Domain.AggregatesModel.ApplicationAggregate;
using Microsoft.Extensions.Logging;
using System.Threading;

namespace Offer.API.Application.Commands
{
    public class UpdateCustomerCommandHandler : IRequestHandler<UpdateCustomerCommand, bool>
    {
        private readonly IApplicationRepository _applicationRepository;
        private readonly IMediator _mediator;
        private readonly ILogger<UpdateCustomerCommand> _logger;

        public UpdateCustomerCommandHandler(IMediator mediator, IApplicationRepository applicationRepository,
            ILogger<UpdateCustomerCommand> logger)
        {
            this._applicationRepository = applicationRepository;
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<bool> Handle(UpdateCustomerCommand message, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Updating customer data for application: {applicationNumber}", message.ApplicationNumber);
            _applicationRepository.UpdateCustomer(message.ApplicationNumber,
                                            message.IdNumber, message.IdAuthority, message.IdValidFrom, message.IdValidTo, message.ContentUrls,
                                            message.CountryResident, message.CityResident, message.PostalCodeResident, message.StreetNameResident, message.StreetNumberResident,
                                            message.CountryCorrespondent, message.CityCorrespondent, message.PostalCodeCorrespondent, message.StreetNameCorrespondent, message.StreetNumberCorrespondent,
                                            message.AccountOwner, message.RelatedCustomers, message.PoliticallyExposedPerson, message.InfluenceGroup, message.BankAffiliated, message.IsAmericanCitizen,
                                            message.IdentificationNumber, message.Gender, message.DateOfBirth);

            return await _applicationRepository.UnitOfWork.SaveEntitiesAsync();
        }
    }

    public class UpdateCustomerCommandIdentifiedHandler : IdentifiedCommandHandler<UpdateCustomerCommand, bool>
    {
        public UpdateCustomerCommandIdentifiedHandler(IMediator mediator, IRequestManager requestManager) : base(mediator, requestManager)
        {

        }

        protected override bool CreateResultForDuplicateRequest()
        {
            return true;
        }
    }
}
