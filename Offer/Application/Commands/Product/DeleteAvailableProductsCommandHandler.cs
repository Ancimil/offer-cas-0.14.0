using MediatR;
using MicroserviceCommon.Application.Commands;
using MicroserviceCommon.Infrastructure.Idempotency;
using MicroserviceCommon.Models;
using Microsoft.Extensions.Logging;
using Offer.Domain.AggregatesModel.ApplicationAggregate;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Offer.API.Application.Commands.Product
{
    public class DeleteAvailableProductsCommandHandler : IRequestHandler<DeleteAvailableProductsCommand, CommandStatus>
    {
        private readonly IApplicationRepository _applicationRepository;
        private readonly ILogger<DeleteAvailableProductsCommand> _logger;

        public DeleteAvailableProductsCommandHandler(IApplicationRepository applicationRepository, ILogger<DeleteAvailableProductsCommand> logger)
        {
            _applicationRepository = applicationRepository ?? throw new ArgumentNullException(nameof(applicationRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<CommandStatus> Handle(DeleteAvailableProductsCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var application = await _applicationRepository.GetAsync(request.ApplicationNumber);
                if (application == null)
                {
                    return new CommandStatus { CommandResult = StandardCommandResult.NOT_FOUND };
                }
                List<string> list = application?.AvailableProducts ?? new List<string>();

                foreach (var code in request.Products)
                {
                    list.Remove(code.Trim());
                }
                application.AvailableProducts = list;
                _applicationRepository.Update(application);
                await _applicationRepository.UnitOfWork.SaveChangesAsync();
                return new CommandStatus { CommandResult = StandardCommandResult.OK };

            }
            catch (Exception e)
            {
                _logger.LogError(e, "AddAvailableProductsCommandHandler error ");
                return new CommandStatus { CommandResult = StandardCommandResult.INTERNAL_ERROR, Exception = e };
            }
        }

        public class DeleteAvailableProductsIdentifiedCommandHandler : IdentifiedCommandHandler<DeleteAvailableProductsCommand, CommandStatus>
        {
            public DeleteAvailableProductsIdentifiedCommandHandler(IMediator mediator, IRequestManager requestManager) : base(mediator, requestManager)
            {
            }
            protected override CommandStatus CreateResultForDuplicateRequest()
            {
                return new CommandStatus { CommandResult = StandardCommandResult.OK };
            }
        }
    }
}
