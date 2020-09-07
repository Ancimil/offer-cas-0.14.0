using MediatR;
using MicroserviceCommon.Application.Commands;
using MicroserviceCommon.Filters;
using MicroserviceCommon.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Offer.API.Application.Commands;
using Offer.API.Application.Filter;
using Offer.Domain.AggregatesModel.ApplicationAggregate;
using System;
using System.Threading.Tasks;

namespace Offer.API.Controllers
{
    [ValidateModel]
    [ApplicationPhaseLockFilter]
    [Route("v1/offer")]
    public class StatusController : Controller
    {
        private readonly IMediator _mediator;
        private readonly ILogger<InvolvedPartyController> _logger;

        public StatusController(
            IMediator mediator,
            ILogger<InvolvedPartyController> logger)
        {
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this._mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        [Authorize]
        [HttpPut]
        [Route("applications/{application-number:long}/status")]
        public async Task<IActionResult> UpdateApplicationStatus([FromRoute]long applicationNumber, [FromBody]UpdateApplicationStatusCommand updateApplicationStatus)
        {
            bool commandResult = false;
            updateApplicationStatus.ApplicationId = applicationNumber;
            var updateApplicationStatusCommand = new IdentifiedCommand<UpdateApplicationStatusCommand, bool>(updateApplicationStatus, new Guid());
            commandResult = await _mediator.Send(updateApplicationStatusCommand);
            return commandResult ? Ok() : StatusCode(500);
        }

        [Authorize]
        [HttpPost]
        [Route("applications/{application-number:long}/cancel")]
        public async Task<IActionResult> CancelApplication([FromRoute]long applicationNumber, [FromBody]CancelApplicationCommand command)
        {
            _logger.LogInformation("Updating application status for application {applicationNumber} to {status}", applicationNumber, "Canceled");
            command.ApplicationNumber = applicationNumber;
            var cancelApplicationCommand = new IdentifiedCommand<CancelApplicationCommand, CommandStatus>(command, new Guid());
            CommandStatus commandResult = await _mediator.Send(cancelApplicationCommand);
            if (commandResult.CommandResult.Equals(StandardCommandResult.OK))
            {
                return Ok();
            }
            else if (commandResult.CommandResult.Equals(StandardCommandResult.BAD_REQUEST))
            {
                return BadRequest();
            }
            else
            {
                return StatusCode(500);
            }
        }

        [Authorize]
        [HttpPost]
        [Route("applications/{application-number:long}/accept")]
        public async Task<IActionResult> AcceptApplication([FromRoute]long applicationNumber)
        {
            _logger.LogInformation("Updating application status for application {applicationNumber} to {status}", applicationNumber, "Accepted");
            var statusInfo = new StatusInformation
            {
                Title = "Accepted",
                Description = "Your request is accepted.",
                Html = ""
            };

            UpdateApplicationStatusCommand updateStatusCommand = new UpdateApplicationStatusCommand { Status = ApplicationStatus.Accepted, StatusInformation = statusInfo };
            updateStatusCommand.ApplicationId = applicationNumber;
            var result = await UpdateApplicationStatus(applicationNumber, updateStatusCommand);
            return result;
        }

        [Authorize]
        [HttpPost]
        [Route("applications/{application-number:long}/reject")]
        public async Task<IActionResult> RejectApplication([FromRoute]long applicationNumber)
        {
            _logger.LogInformation("Updating application status for application {applicationNumber} to {status}", applicationNumber, "Rejected");
            var statusInfo = new StatusInformation
            {
                Title = "Reject",
                Description = "Your request is rejected.",
                Html = ""
            };
            UpdateApplicationStatusCommand updateStatusCommand = new UpdateApplicationStatusCommand { Status = ApplicationStatus.Rejected, StatusInformation = statusInfo };
            var result = await UpdateApplicationStatus(applicationNumber, updateStatusCommand);
            return result;
        }
    }
}
