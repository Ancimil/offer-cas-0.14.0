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
    public class AddAvailableProductsCommandHandler : IRequestHandler<AddAvailableProductsCommand, CommandStatus>
    {
        private readonly IMediator _mediator;
        private readonly IApplicationRepository _applicationRepository;
        private readonly ILogger<AddAvailableProductsCommand> _logger;

        public AddAvailableProductsCommandHandler(IApplicationRepository applicationRepository, ILogger<AddAvailableProductsCommand> logger,
            IMediator mediator)
        {
            _applicationRepository = applicationRepository ?? throw new ArgumentNullException(nameof(applicationRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        public async Task<CommandStatus> Handle(AddAvailableProductsCommand request, CancellationToken cancellationToken)
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
                    if (!list.Contains(code.Trim()) && !string.IsNullOrEmpty(code.Trim()))
                    {
                        var getProduct = new IdentifiedCommand<GetProductDataCommand, ProductData>(new GetProductDataCommand
                        {
                            ProductCode = code

                        }, new Guid());
                        ProductData product = await _mediator.Send(getProduct);
                        if (product == null)
                        {
                            return new CommandStatus { CommandResult = StandardCommandResult.BAD_REQUEST, 
                                CustomError = "Not found product with product code: " + code };
                        }
                        list.Add(code.Trim());
                    }
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

        public class AddAvailableProductsIdentifiedCommandHandler : IdentifiedCommandHandler<AddAvailableProductsCommand, CommandStatus>
        {
            public AddAvailableProductsIdentifiedCommandHandler(IMediator mediator, IRequestManager requestManager) : base(mediator, requestManager)
            {
            }
            protected override CommandStatus CreateResultForDuplicateRequest()
            {
                return new CommandStatus { CommandResult = StandardCommandResult.OK };
            }
        }
    }
}
