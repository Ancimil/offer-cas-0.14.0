using MediatR;
using MicroserviceCommon.Application.Commands;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Offer.API.Application.Commands;
using Offer.Domain.AggregatesModel.ApplicationAggregate;
using PriceCalculation.Models.Pricing;
using System;
using System.Threading.Tasks;
using CalculationService.Models;
using CalculationService.Calculations;
using MicroserviceCommon.Filters;

namespace Offer.API.Controllers
{
    [ValidateModel]
    [Route("v1/offer")]
    public class CalculationController : Controller
    {
        private readonly IMediator _mediator;
        private readonly IApplicationRepository _applicationRepository;
        private readonly ILogger<InvolvedPartyController> _logger;
        private readonly Calculator _calculator;

        public CalculationController(
            IApplicationRepository applicationRepository,
            IMediator mediator,
            ILogger<InvolvedPartyController> logger,
            Calculator calculator)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _applicationRepository = applicationRepository ?? throw new ArgumentNullException(nameof(applicationRepository));
            _applicationRepository = applicationRepository ?? throw new ArgumentNullException(nameof(applicationRepository));
            _calculator = calculator ?? throw new ArgumentNullException(nameof(calculator));
        }

        [HttpPost]
        [Route("calculations/calculate-proposal")]
        public async Task<IActionResult> CalculateOfferAsync([FromBody]InitiateCalculateOfferCommand command)
        {
            try
            {
                ArrangementRequest commandResult;
                if ((command.DownpaymentAmount > 0 && Math.Round(command.InvoiceAmount - command.DownpaymentAmount - command.Amount, 2) != 0) ||
                   (command.DownpaymentAmount == 0 && command.InvoiceAmount > 0 && Math.Round(command.InvoiceAmount - command.Amount, 2) != 0))
                {
                    _logger.LogError("Downpayment amount and requested amount doesn't match invoice amount!");
                    return (IActionResult)BadRequest("Downpayment amount and requested amount doesn't match invoice amount!");
                }
                var calculateOfferCommand = new IdentifiedCommand<InitiateCalculateOfferCommand, ArrangementRequest>(command, new Guid());
                commandResult = await _mediator.Send(calculateOfferCommand);
                return commandResult != null ? (IActionResult)Json(commandResult) : (IActionResult)BadRequest();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error in calculate proposal");
                return (IActionResult)BadRequest(new { error = "Error in calculate proposal: " + e.Message });
            }
        }

        [HttpPost]
        [Route("calculations/calculate-price")]
        public async Task<IActionResult> PriceCalculation([FromBody]InitiatePriceCalculationCommand command)
        {
            PriceCalculationResponse commandResult;
            var calculateOfferCommand = new IdentifiedCommand<InitiatePriceCalculationCommand, PriceCalculationResponse>(command, new Guid());
            commandResult = await _mediator.Send(calculateOfferCommand);
            return commandResult != null ? (IActionResult)Json(commandResult) : (IActionResult)BadRequest();
        }

        [HttpPost]
        [Route("calculation-service/calculate-installment-plan")]
        public ActionResult CalculateInstallmentPlan([FromBody] CalculateInstallmentPlanRequest request)
        {
            var plan = _calculator.CalculateInstallmentPlan(request);
            return Ok(plan);
        }
    }
}
