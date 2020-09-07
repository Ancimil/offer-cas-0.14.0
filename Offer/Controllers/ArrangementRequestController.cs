using MediatR;
using MicroserviceCommon.Application.Commands;
using MicroserviceCommon.Filters;
using MicroserviceCommon.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Offer.API.Application.Commands;
using Offer.API.Application.Commands.ArrangementRequests;
using Offer.API.Application.Filter;
using Offer.Domain.AggregatesModel.ApplicationAggregate;
using Offer.Domain.AggregatesModel.ApplicationAggregate.AlternativeOfferModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Offer.API.Controllers
{
    [ApplicationPhaseLockFilter]
    [ValidateModel]
    [Route("v1/offer")]
    public class ArrangementRequestController : Controller
    {
        private readonly IMediator _mediator;
        private readonly IApplicationRepository _applicationRepository;
        private readonly IArrangementRequestRepository _arrangementRequestRepository;
        private readonly ILogger<ArrangementRequestController> _logger;

        public ArrangementRequestController(IApplicationRepository applicationRepository,
            IArrangementRequestRepository arrangementRequestRepository,
            IMediator mediator,
            ILogger<ArrangementRequestController> logger)
        {
            _logger = logger;
            _applicationRepository = applicationRepository;
            _arrangementRequestRepository = arrangementRequestRepository;
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        [HttpGet]
        [Route("applications/{application-number:long}/arrangement-requests")]
        public IActionResult GetArrangementRequestsForApplication([FromRoute] long applicationNumber, [FromQuery]string include,
            [FromQuery]string trim, [FromQuery] bool includePotential = false)
        {
            List<ArrangementRequest> arrangementRequests = _arrangementRequestRepository.GetArrangementRequests(applicationNumber,
                include, trim, includePotential);
            if (arrangementRequests != null)
            {
                return new ObjectResult(new { ArrangementRequests = arrangementRequests });
            }
            return NotFound();
        }


        [HttpPost]
        [Route("applications/{application-number:long}/arrangement-requests")]
        public async Task<IActionResult> AddArrangementRequestsForApplication([FromRoute] long applicationNumber, [FromBody] ArrangementRequest command)
        {
            var addArrangementRequest = new AddArrangementRequestCommand
            {
                ArrangementRequest = command,
                ApplicationNumber = applicationNumber,
            };
            bool? commandResult;
            commandResult = await _mediator.Send(addArrangementRequest);
            return commandResult.HasValue ? commandResult.Value ? (IActionResult)Ok() : (IActionResult)BadRequest() : NotFound();
        }

        [HttpGet]
        [Route("applications/{application-number:long}/arrangement-requests/{arrangement-request-id:int}")]
        public IActionResult GetArrangementRequestForApplication([FromRoute] long applicationNumber, [FromRoute] int arrangementRequestId, [FromQuery]string include, [FromQuery]string trim)
        {
            var result = _arrangementRequestRepository.GetArrangementRequest(applicationNumber, arrangementRequestId, include, trim);
            string[] inclusions = string.IsNullOrEmpty(include) ? new string[] { } : include.Split(',');
            if (result != null)
            {
                if (!inclusions.Contains("installment-plan"))
                {
                    result.InstallmentPlan = null;
                }
                if (!inclusions.Contains("product-snapshot"))
                {
                    result.ProductSnapshot = null;
                }
            }
            if (result == null)
            {
                return NotFound();
            }
            return Ok(result);
        }

        [HttpPut]
        [Route("applications/{application-number:long}/arrangement-requests/{arrangement-request-id:int}")]
        public async Task<IActionResult> UpdateArrangementRequestsForApplication([FromRoute] long applicationNumber, [FromRoute] int arrangementRequestId, [FromBody] ArrangementRequest command)
        {
            var updateArrangementRequest = new UpdateArrangementRequestCommand
            {
                ArrangementRequest = command,
                ApplicationNumber = applicationNumber,
                ArrangementRequestId = arrangementRequestId
            };
            var updateCustomerCommand = new IdentifiedCommand<UpdateArrangementRequestCommand, CommandStatus<bool>>(updateArrangementRequest, new Guid());

            var commandResult = await _mediator.Send(updateCustomerCommand);
            if (commandResult.CommandResult == StandardCommandResult.OK)
            {
                return Ok();
            }
            else if (commandResult.CommandResult == StandardCommandResult.NOT_FOUND)
            {
                return NotFound();
            }
            else if (commandResult.CommandResult == StandardCommandResult.BAD_REQUEST)
            {
                return BadRequest(commandResult.Exception.Message);
            }
            else
            {
                return StatusCode(500);
            }
        }

        [HttpDelete]
        [Route("applications/{application-number:long}/arrangement-requests/{arrangement-request-id:int}")]
        public async Task<IActionResult> GetArrangementRequestForApplication([FromRoute] long applicationNumber, [FromRoute] int arrangementRequestId)
        {
            if (_applicationRepository.IsMainProduct(applicationNumber, arrangementRequestId))
            {
                return BadRequest(new { error = "You cannot delete main product of application" });
            }

            bool? commandResult;
            var deleteArrangementRequest = new DeleteArrangementRequestCommand
            {
                ApplicationNumber = applicationNumber,
                ArrangementRequestId = arrangementRequestId
            };
            var deleteCustomerCommand = new IdentifiedCommand<DeleteArrangementRequestCommand, bool?>(deleteArrangementRequest, new Guid());
            commandResult = await _mediator.Send(deleteCustomerCommand);
            return commandResult.HasValue ? commandResult.Value ? (IActionResult)Ok() : (IActionResult)BadRequest() : NotFound();
        }

        [HttpPost]
        [Route("applications/{application-number:long}/arrangement-requests-availability")]
        public async Task<IActionResult> UpsertArrangementRequestsToApplication([FromRoute] long applicationNumber,
            [FromBody] ArrangementRequestsAvailabilityCommand command)
        {
            command.ApplicationId = applicationNumber;
            var updatRequestsAvailability = new IdentifiedCommand<ArrangementRequestsAvailabilityCommand, bool?>(command, new Guid());
            var commandResult = await _mediator.Send(updatRequestsAvailability);
            return commandResult.HasValue ? commandResult.Value ? (IActionResult)Ok() : (IActionResult)BadRequest() : NotFound();
        }

        [HttpPut]
        [Route("applications/{application-number:long}/arrangement-requests/{arrangement-request-id:int}/extended")]
        public async Task<IActionResult> PostExtendedArrangementData([FromRoute] long applicationNumber, [FromRoute] int arrangementRequestId,
            [FromBody] Offer.API.Application.Commands.ArrangementRequests.PutExtendedPartyCommand command)
        {
            command.ApplicationNumber = applicationNumber;
            command.ArrangementRequestId = arrangementRequestId;
            var postExtendedDataCommand = new IdentifiedCommand<Offer.API.Application.Commands.ArrangementRequests.PutExtendedPartyCommand, CommandStatus>(command, new Guid());
            CommandStatus commandResult = await _mediator.Send(postExtendedDataCommand);

            if (commandResult.CommandResult.Equals(StandardCommandResult.OK))
            {
                return Ok();
            }
            else if (commandResult.CommandResult.Equals(StandardCommandResult.NOT_FOUND))
            {
                return BadRequest();
            }
            else if (commandResult.CommandResult.Equals(StandardCommandResult.BAD_REQUEST))
            {
                return BadRequest();
            }
            else if (commandResult.Equals(StandardCommandResult.INTERNAL_ERROR))
            {
                _logger.LogError(commandResult.Exception, "An error occurred while putting extended sections to arrangement request");
                return StatusCode(500);
            }
            else
            {
                _logger.LogError("An unknown error occurred while putting extended sections to arrangement request");
                return StatusCode(500);
            }
        }

        [HttpGet]
        [Route("applications/{application-number:long}/arrangement-requests/{arrangement-request-id:int}/extended")]
        public IActionResult GetExtendedPartyData([FromRoute] long applicationNumber, [FromRoute] int arrangementRequestId)
        {
            try
            {
                var data = _arrangementRequestRepository.GetExtendedArrangementData(applicationNumber, arrangementRequestId);
                if (data == null)
                {
                    return NotFound();
                }

                return new ObjectResult(data);
            }
            catch (KeyNotFoundException e)
            {
                return BadRequest(new { message = e.Message });
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An error occurred while getting extended data for arrangement request");
                return StatusCode(500);
            }
        }

        [HttpGet]
        [Route("applications/{application-number:long}/arrangement-requests/{arrangement-request-id:int}/extended/{section-name}")]
        public IActionResult GetExtendedSection([FromRoute] long applicationNumber, [FromRoute] int arrangementRequestId,
            [FromRoute] string sectionName)
        {
            var data = _arrangementRequestRepository.GetExtendedArrangementDataSection(applicationNumber, arrangementRequestId, sectionName);

            if (data == null)
            {
                return NotFound();
            }
            return new ObjectResult(data);
        }

        [HttpDelete]
        [Route("applications/{application-number:long}/arrangement-requests/{arrangement-request-id:int}/extended/{section-name}")]
        public async Task<IActionResult> DeleteExtendedSection([FromRoute] long applicationNumber, [FromRoute] int arrangementRequestId,
            [FromRoute] string sectionName)
        {
            try
            {
                var isDeleted = await _arrangementRequestRepository.DeleteExtendedDataSection(applicationNumber, arrangementRequestId, sectionName);
                if (isDeleted.HasValue)
                {
                    return NoContent();
                }
                else
                {
                    return NotFound();
                }
            }
            catch (KeyNotFoundException e)
            {
                return BadRequest(e.Message);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An error occurred while deleting extended section");
                return StatusCode(500);
            }
        }

        [HttpPatch]
        [Route("applications/{application-number:long}/arrangement-requests/{arrangement-request-id:int}/accounts")]
        public async Task<IActionResult> AddAccountNumbersAsync([FromRoute]long applicationNumber, [FromRoute] int arrangementRequestId, [FromBody]AddAccountNumbersCommand command)
        {
            bool? commandResult;
            if (command == null)
            {
                return (IActionResult)BadRequest();
            }
            command.ApplicationNumber = applicationNumber;
            command.ArrangementRequestId = arrangementRequestId;
            var AddAccountNumbersCommand = new IdentifiedCommand<AddAccountNumbersCommand, bool?>(command, new Guid());
            commandResult = await _mediator.Send(AddAccountNumbersCommand);
            return commandResult.HasValue ? commandResult.Value ? (IActionResult)Ok() : (IActionResult)BadRequest() : NotFound();
        }

        [HttpPost]
        [Route("applications/{application-number:long}/arrangement-requests/{arrangement-request-id:int}/recalculate")]
        public async Task<IActionResult> RecalculateArrangementRequest([FromRoute]long applicationNumber, [FromRoute] int arrangementRequestId)
        {
            var command = new RecalculateArrangementRequestCommand
            {
                ApplicationNumber = applicationNumber,
                ArrangementRequestId = arrangementRequestId

            };
            var recalculateArrangementRequestCommand = new IdentifiedCommand<RecalculateArrangementRequestCommand, bool?>(command, new Guid());
            var commandResult = await _mediator.Send(recalculateArrangementRequestCommand);
            return commandResult.HasValue ? commandResult.Value ? (IActionResult)Ok() : (IActionResult)BadRequest() : NotFound();
        }

        [HttpPost]
        [Route("applications/{application-number:long}/arrangement-requests/{arrangement-request-id:int}/availability")]
        public async Task<IActionResult> SetArrangementRequestAvailability([FromRoute]long applicationNumber,
            [FromRoute] int arrangementRequestId, [FromBody] SetArrangementRequestAvailabilityCommand command)
        {
            command.ApplicationId = applicationNumber;
            command.ArrangementRequestId = arrangementRequestId;
            var setArrangementRequestAvailabilityCommand = new IdentifiedCommand<SetArrangementRequestAvailabilityCommand, bool?>
                (command, new Guid());
            var commandResult = await _mediator.Send(setArrangementRequestAvailabilityCommand);
            return commandResult.HasValue ? commandResult.Value ? (IActionResult)Ok() : (IActionResult)BadRequest() : NotFound();
        }

        [HttpPost]
        [Route("applications/{application-number:long}/arrangement-requests/{arrangement-request-id:int}/approved-limits")]
        public async Task<IActionResult> SetApprovedLimits([FromRoute]long applicationNumber, [FromRoute] int arrangementRequestId, [FromBody] ApprovedLimits command)
        {
            try
            {
                var application = await _applicationRepository.GetAsync(applicationNumber);
                if (application == null)
                {
                    return NotFound();
                }
                var setApprovedLimits = await _arrangementRequestRepository
                    .SetApprovedLimits(applicationNumber, arrangementRequestId, command, application);

                if (setApprovedLimits == null)
                {
                    return NotFound();
                }
                return Ok();
            }
            catch (KeyNotFoundException e)
            {
                this._logger.LogError(e, "An error occurred while updating Arrangement Request for arrangementRequestId {arrangementRequestId} on application {ApplicationNumber}", arrangementRequestId, applicationNumber);
                return BadRequest(new { message = e.Message });
            }
            catch (Exception e)
            {
                this._logger.LogError(e, "An error occurred while updating Arrangement Request for arrangementRequestId {arrangementRequestId} on application {ApplicationNumber}", arrangementRequestId, applicationNumber);
                return StatusCode(500);
            }
        }

        [HttpPost]
        [Route("applications/{application-number:long}/arrangement-requests/{arrangement-request-id:int}/accepted-values")]
        public async Task<IActionResult> SetAcceptedValues([FromRoute]long applicationNumber,
            [FromRoute] int arrangementRequestId, [FromBody] AcceptedValues command)
        {
            try
            {
                var application = await _applicationRepository.GetAsync(applicationNumber);
                if (application != null)
                {
                    var setAcceptedValues = await _arrangementRequestRepository
                        .SetAcceptedValues(applicationNumber, arrangementRequestId, command, application);
                    if (setAcceptedValues == null)
                    {
                        return NotFound();
                    }
                    return Ok();
                }
                else
                {
                    return NotFound();
                }
            }
            catch (KeyNotFoundException e)
            {
                this._logger.LogError(e, "An error occurred while updating Arrangement Request for arrangementRequestId {arrangementRequestId} on application {ApplicationNumber}", arrangementRequestId, applicationNumber);
                return BadRequest(new { message = e.Message });
            }
            catch (Exception e)
            {
                this._logger.LogError(e, "An error occurred while updating Arrangement Request for arrangementRequestId {arrangementRequestId} on application {ApplicationNumber}", arrangementRequestId, applicationNumber);
                return StatusCode(500);
            }
        }

        [HttpPost]
        [Route("applications/{application-number:long}/arrangement-requests/validate")]
        public async Task<IActionResult> ValidateArrangementRequests([FromRoute]long applicationNumber)
        {
            try
            {
                var cmd = new ValidateArrangementRequestsCommand
                {
                    ApplicationId = applicationNumber
                };
                var setArrangementRequestAvailabilityCommand = new IdentifiedCommand
                    <ValidateArrangementRequestsCommand, ValidateArrangementRequestResponse>(cmd, new Guid());
                var commandResult = await _mediator.Send(setArrangementRequestAvailabilityCommand);
                return Ok(commandResult);
            }
            catch (Exception e)
            {
                this._logger.LogError(e, "An error occurred while valdiating Arrangement Requests for application {ApplicationNumber}", applicationNumber);
                return StatusCode(500);
            }
        }

        [HttpPost]
        [Route("applications/{application-number:long}/arrangement-requests/{arrangement-request-id:int}/credit-line-users")]
        public async Task<IActionResult> SetCreditLineUsers([FromRoute]long applicationNumber, [FromRoute] int arrangementRequestId, [FromBody] AllowedRevolvingLineUsersCommand command)
        {
            try
            {
                //var application = await _applicationRepository.GetAsync(applicationNumber);
                //var setCreditLineUsers = await _arrangementRequestRepository.SetCreditLineUsers
                //    (applicationNumber, arrangementRequestId, command, application);
                //return Ok();
                var comm = new AllowedRevolvingLineUsersCommand
                {
                    ApplicationId = applicationNumber,
                    ArrangementRequestId = arrangementRequestId
                };
                command.ApplicationId = comm.ApplicationId;
                command.ArrangementRequestId = arrangementRequestId;
                var setCreditLineUsers = new IdentifiedCommand<AllowedRevolvingLineUsersCommand, bool?>(command, new Guid());
                var commandResult = await _mediator.Send(setCreditLineUsers);
                return commandResult.HasValue ? commandResult.Value ? (IActionResult)Ok() : (IActionResult)BadRequest() : NotFound();
            }
            catch (KeyNotFoundException e)
            {
                this._logger.LogError(e, "An error occurred while updating Arrangement Request for arrangementRequestId {arrangementRequestId} on application {ApplicationNumber}", arrangementRequestId, applicationNumber);
                return BadRequest(new { message = e.Message });
            }
            catch (Exception e)
            {
                this._logger.LogError(e, "An error occurred while updating Arrangement Request for arrangementRequestId {arrangementRequestId} on application {ApplicationNumber}", arrangementRequestId, applicationNumber);
                return StatusCode(500);
            }
        }

        [HttpPost]
        [Route("applications/{application-number:long}/arrangement-requests/{arrangement-request-id:int}/credit-line-product-codes")]
        public async Task<IActionResult> SetCreditLineProductCodes([FromRoute]long applicationNumber, [FromRoute] int arrangementRequestId, [FromBody] ProductCodesCommand command)
        {
            try
            {
                // var application = await _applicationRepository.GetAsync(applicationNumber);
                var comm = new ProductCodesCommand
                {
                    ApplicationId = applicationNumber,
                    ArrangementRequestId = arrangementRequestId
                };
                command.ApplicationId = comm.ApplicationId;
                command.ArrangementRequestId = arrangementRequestId;
                var setCreditLineProductCodes = new IdentifiedCommand<ProductCodesCommand, bool?>(command, new Guid());
                var commandResult = await _mediator.Send(setCreditLineProductCodes);
                return commandResult.HasValue ? commandResult.Value ? (IActionResult)Ok() : (IActionResult)BadRequest() : NotFound();
            }
            catch (KeyNotFoundException e)
            {
                this._logger.LogError(e, "An error occurred while updating Arrangement Request for arrangementRequestId {arrangementRequestId} on application {ApplicationNumber}", arrangementRequestId, applicationNumber);
                return BadRequest(new { message = e.Message });
            }
            catch (Exception e)
            {
                this._logger.LogError(e, "An error occurred while updating Arrangement Request for arrangementRequestId {arrangementRequestId} on application {ApplicationNumber}", arrangementRequestId, applicationNumber);
                return StatusCode(500);
            }
        }

        [HttpPost]
        [Route("applications/{application-number:long}/arrangement-requests/{arrangement-request-id:int}/credit-line-product-kinds")]
        public async Task<IActionResult> SetCreditLineProductKinds([FromRoute]long applicationNumber, [FromRoute] int arrangementRequestId, [FromBody] ProductKindsCommand command)
        {
            try
            {
                //var application = await _applicationRepository.GetAsync(applicationNumber);
                //var setCreditLineProductKinds = await _arrangementRequestRepository.SetCreditLineProductKinds
                //    (applicationNumber, arrangementRequestId, command, application);
                //return Ok();
                var comm = new ProductKindsCommand
                {
                    ApplicationId = applicationNumber,
                    ArrangementRequestId = arrangementRequestId
                };
                command.ApplicationId = comm.ApplicationId;
                command.ArrangementRequestId = arrangementRequestId;
                var setCreditLineProductKinds = new IdentifiedCommand<ProductKindsCommand, bool?>(command, new Guid());
                var commandResult = await _mediator.Send(setCreditLineProductKinds);
                return commandResult.HasValue ? commandResult.Value ? (IActionResult)Ok() : (IActionResult)BadRequest() : NotFound();
            }
            catch (KeyNotFoundException e)
            {
                this._logger.LogError(e, "An error occurred while updating Arrangement Request for arrangementRequestId {arrangementRequestId} on application {ApplicationNumber}", arrangementRequestId, applicationNumber);
                return BadRequest(new { message = e.Message });
            }
            catch (Exception e)
            {
                this._logger.LogError(e, "An error occurred while updating Arrangement Request for arrangementRequestId {arrangementRequestId} on application {ApplicationNumber}", arrangementRequestId, applicationNumber);
                return StatusCode(500);
            }
        }


    }
}
