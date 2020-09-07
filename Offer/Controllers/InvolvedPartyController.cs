using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Offer.Domain.AggregatesModel.ApplicationAggregate;
using Offer.Domain.Exceptions;
using Offer.API.Application.Commands;
using MicroserviceCommon.Application.Commands;
using Offer.Domain.Repository;
using System.IO;
using Offer.Domain.AggregatesModel.ApplicationAggregate.ApplicantModel;
using MicroserviceCommon.Filters;
using Offer.Domain.AggregatesModel.CreditBureauModel;
using Offer.API.Application.Filter;
using MicroserviceCommon.Models;

namespace Offer.API.Controllers
{
    [ApplicationPhaseLockFilter]
    [ValidateModel]
    [Authorize]
    [Route("v1/offer")]
    public class InvolvedPartyController : Controller
    {
        private readonly IMediator _mediator;
        private readonly IInvolvedPartyRepository _involvedPartyRepository;
        private readonly IApplicationRepository _applicationRepository;
        private readonly ILogger<InvolvedPartyController> _logger;

        public InvolvedPartyController(
            IInvolvedPartyRepository involvedPartyRepository,
            IApplicationRepository applicationRepository,
            IMediator mediator,
            ILogger<InvolvedPartyController> logger)
        {
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this._involvedPartyRepository = involvedPartyRepository ?? throw new ArgumentNullException(nameof(involvedPartyRepository));
            this._mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            this._applicationRepository = applicationRepository;
        }

        [HttpGet]
        [Route("applications/{application-number:long}/involved-parties")]
        public IActionResult GetInvolvedParties([FromRoute]long applicationNumber, [FromQuery]string include, [FromQuery]string trim)
        {
            var involvedParties = _applicationRepository.GetInvolvedParties(applicationNumber).Result;
            if (involvedParties != null)
            {
                var partyList = involvedParties.ToList();
                string[] inclusions = string.IsNullOrEmpty(include) ? new string[] { } : include.Split(',');
                partyList.ForEach(x =>
                {
                    if (!inclusions.Contains("employment-data"))
                    {
                        if (x is IndividualParty party)
                        {
                            party.EmploymentData = null;
                        }
                    }

                    if (!inclusions.Contains("household-info"))
                    {
                        if (x is IndividualParty party)
                        {
                            party.HouseholdInfo = null;
                        }
                    }
                    if (!inclusions.Contains("financial-profile"))
                    {
                        if (x is IndividualParty party)
                        {
                            party.FinancialProfile = null;
                        }
                    }
                    if (!inclusions.Contains("product-usage"))
                    {
                        x.ProductUsageInfo = null;
                    }
                    if (!inclusions.Contains("contact-points"))
                    {
                        if (x is IndividualParty party)
                        {
                            party.MobilePhone = null;
                            party.HomePhoneNumber = null;
                        }
                        x.EmailAddress = null;
                        x.ContactAddress = null;
                        x.LegalAddress = null;
                    }
                    if (!inclusions.Contains("credit-bureau"))
                    {
                        if (x is IndividualParty party)
                        {
                            party.CreditBureauData = null;
                        }
                        else
                        {
                            var orgParty = (OrganizationParty)x;
                            orgParty.CreditBureauData = null;
                        }
                    }
                });
                return new ObjectResult(new { involvedParties });
            }
            return NotFound();
        }

        [Authorize]
        [HttpPost]
        [Route("applications/{application-number:long}/involved-parties")]
        public async Task<IActionResult> AddInvolvedParty([FromRoute] long applicationNumber, [FromBody] Party command)
        {
            try
            {
                bool auditLog = AuditLog();
                var newParty = await _involvedPartyRepository.AddParty(applicationNumber, command, auditLog);
                return StatusCode(201);
            }
            catch (KeyNotFoundException e)
            {
                return BadRequest(new { message = e.Message });
            }
            catch (DuplicateObjectException e)
            {
                return BadRequest(new { message = e.Message });
            }
            catch
            {
                return StatusCode(500);
            }
        }


        [HttpGet]
        [Route("applications/{application-number:long}/involved-parties/{party-id:int}")]
        public async Task<IActionResult> GetPartyGeneralInformation([FromRoute] long applicationNumber, [FromRoute] int partyId, [FromQuery]string include, [FromQuery]string trim)
        {

            var result = await _involvedPartyRepository.GetPartyGeneralInformation(applicationNumber, partyId, include, trim);
            if (result != null)
            {
                return Ok(result);
            }
            else
            {
                return NotFound();
            }

        }

        [HttpPut]
        [Route("applications/{application-number:long}/involved-parties/{party-id:int}")]
        public async Task<IActionResult> PostInvolvedParty([FromRoute] long applicationNumber, [FromRoute] int partyId, [FromBody] Party party)
        {
            try
            {
                var updatedParty = await _involvedPartyRepository.UpdatePartyGeneralInformation(applicationNumber, partyId, party);
                if (updatedParty != null)
                {
                    return Ok();
                }
                else
                {
                    return NotFound();
                }
            }
            catch (InvalidDataException e)
            {
                return BadRequest(new { message = e.Message });
            }
            catch (ArgumentNullException e)
            {
                return BadRequest(new { message = e.Message });
            }
        }

        [Authorize]
        [HttpDelete]
        [Route("applications/{application-number:long}/involved-parties/{party-id:int}")]
        public async Task<IActionResult> DeleteInvolvedParty([FromRoute] long applicationNumber, [FromRoute] int partyId)
        {
            bool auditLog = AuditLog();
            try
            {
                var deletedSuccessfully = await _involvedPartyRepository.DeleteParty(applicationNumber, partyId, auditLog);

                return deletedSuccessfully.HasValue ? deletedSuccessfully.Value ?
                    (IActionResult)Ok() : BadRequest(new { message = "Something went wrong. Party could not be deleted." }) : NotFound();
            }
            catch (KeyNotFoundException e)
            {
                return BadRequest(new { message = e.Message });
            }
        }

        [HttpPut]
        [Route("applications/{application-number:long}/involved-parties/{party-id:int}/extended")]
        public async Task<IActionResult> PutExtendedPartyData([FromRoute] long applicationNumber, [FromRoute] int partyId,
            [FromBody] PutExtendedPartyCommand command)
        {
            command.ApplicationNumber = applicationNumber;
            command.PartyId = partyId;
            var putExtendedPartyDataCommand = new IdentifiedCommand<PutExtendedPartyCommand, CommandStatus>(command, new Guid());
            CommandStatus commandResult = await _mediator.Send(putExtendedPartyDataCommand);

            if (commandResult.CommandResult.Equals(StandardCommandResult.OK))
            {
                return Ok();
            }
            else if (commandResult.CommandResult.Equals(StandardCommandResult.BAD_REQUEST))
            {
                return BadRequest();
            }
            else if (commandResult.CommandResult.Equals(StandardCommandResult.NOT_FOUND))
            {
                return BadRequest();
            }
            else if (commandResult.Equals(StandardCommandResult.INTERNAL_ERROR))
            {
                _logger.LogError(commandResult.Exception, "An error occurred while putting extended sections to party");
                return StatusCode(500);
            }
            else
            {
                _logger.LogError("An unknown error occurred while putting extended sections to party");
                return StatusCode(500);
            }
        }

        [HttpGet]
        [Route("applications/{application-number:long}/involved-parties/{party-id:int}/extended")]
        public async Task<ActionResult> GetExtendedPartyData([FromRoute] long applicationNumber, [FromRoute] int partyId)
        {
            var data = await _involvedPartyRepository.GetExtendedPartyData(applicationNumber, partyId);
            if (data == null)
            {
                return NoContent();
            }
            return new ObjectResult(data);
        }

        [HttpGet]
        [Route("applications/{application-number:long}/involved-parties/{party-id:int}/extended/{section-name}")]
        public async Task<ActionResult> GetExtendedSection([FromRoute] long applicationNumber, [FromRoute] int partyId, [FromRoute] string sectionName)
        {
            var data = await _involvedPartyRepository.GetExtendedPartyDataSection(applicationNumber, partyId, sectionName);
            if (data == null)
            {
                return NoContent();
            }
            return new ObjectResult(data);
        }

        [HttpDelete]
        [Route("applications/{application-number:long}/involved-parties/{party-id:int}/extended/{section-name}")]
        public async Task<IActionResult> DeleteExtendedSection([FromRoute] long applicationNumber, [FromRoute] int partyId, [FromRoute] string sectionName)
        {
            try
            {
                var isDeleted = await _involvedPartyRepository.DeleteExtendedPartyDataSection(applicationNumber, partyId, sectionName);
                if (isDeleted.HasValue)
                {
                    return NoContent();
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An error occurred while deleting extended section");
                return StatusCode(500);
            }
        }

        [HttpGet]
        [Route("applications/{application-number:long}/involved-parties/{party-id:int}/employment-info")]
        public async Task<ActionResult> GetPartyEmploymentInfo([FromRoute] long applicationNumber, [FromRoute] int partyId)
        {
            var result = await _involvedPartyRepository.GetPartyEmploymentInfo(applicationNumber, partyId);
            if (result != null)
            {
                return Ok(result);
            }
            return NoContent();
        }

        [HttpPut]
        [Route("applications/{application-number:long}/involved-parties/{party-id:int}/employment-info")]
        public async Task<IActionResult> UpdateEmploymentInfo([FromRoute] long applicationNumber, [FromRoute] int partyId, [FromBody] UpdateEmploymentInfoCommand command)
        {
            command.ApplicationNumber = applicationNumber;
            command.PartyId = partyId;

            var updateInfoCmd = new IdentifiedCommand<UpdateEmploymentInfoCommand, EmploymentData>(command, new Guid());
            try
            {
                EmploymentData commandResult = await _mediator.Send(updateInfoCmd);
                if (command != null)
                {
                    return Ok(commandResult);
                }
                else
                {
                    return NoContent();
                }
            }
            catch (InvalidDataException e)
            {
                return BadRequest(new { message = e.Message });
            }
        }

        [HttpGet]
        [Route("applications/{application-number:long}/involved-parties/{party-id:int}/household-info")]
        public async Task<IActionResult> GetPartyHouseholdInfo([FromRoute] long applicationNumber, [FromRoute] int partyId)
        {
            var result = await _involvedPartyRepository.GetPartyHouseholdInfo(applicationNumber, partyId);
            if (result != null)
            {
                return Ok(result);
            }
            return NoContent();

        }
        [HttpPut]
        [Route("applications/{application-number:long}/involved-parties/{party-id:int}/household-info")]
        public async Task<IActionResult> UpdateHouseHoldInfo([FromRoute] long applicationNumber, [FromRoute] int partyId, [FromBody] UpdateHouseholdInfoCommand command)
        {
            Household commandResult;
            command.ApplicationNumber = applicationNumber;
            command.PartyId = partyId;
            var updateInfoCmd = new IdentifiedCommand<UpdateHouseholdInfoCommand, Household>(command, new Guid());
            try
            {
                commandResult = await _mediator.Send(updateInfoCmd);
                if (commandResult != null)
                {
                    return Ok();
                }
                else
                {
                    return NoContent();
                }
            }
            catch (InvalidDataException e)
            {
                return BadRequest(new { message = e.Message });
            }
        }

        [HttpGet]
        [Route("applications/{application-number:long}/involved-parties/{party-id:int}/relationships")]
        public IActionResult GetPartyRelationshis([FromRoute] long applicationNumber, [FromRoute] int partyId)
        {
            var relationships = _involvedPartyRepository.GetPartyRelationshis(applicationNumber, partyId);
            if (relationships != null)
            {
                return Ok(relationships);
            }
            else
            {
                return NoContent();
            }
        }

        [HttpGet]
        [Route("applications/{application-number:long}/involved-parties/{party-id:int}/bank-accounts")]
        public async Task<IActionResult> GetPartyBankAccounts([FromRoute] long applicationNumber, [FromRoute] int partyId)
        {
            var accounts = await _involvedPartyRepository.GetPartyBankAccounts(applicationNumber, partyId);
            if (accounts != null)
            {
                return Ok(new { BankAccounts = accounts });
            }
            else
            {
                return NoContent();
            }
        }

        [HttpGet]
        [Route("applications/{application-number:long}/involved-parties/{party-id:int}/financial-profile")]
        public async Task<IActionResult> GetPartyFinancialProfile([FromRoute] long applicationNumber, [FromRoute] int partyId)
        {
            var result = await _involvedPartyRepository.GetPartyFinancialProfile(applicationNumber, partyId);
            if (result != null)
            {
                return Ok(result);
            }
            return NoContent();
        }

        [HttpPut]
        [Route("applications/{application-number:long}/involved-parties/{party-id:int}/financial-profile")]
        public async Task<IActionResult> UpdateFinancialProfile([FromRoute] long applicationNumber, [FromRoute] int partyId, [FromBody] UpdateFinancialProfileCommand command)
        {
            FinancialProfile commandResult;
            command.ApplicationNumber = applicationNumber;
            command.PartyId = partyId;

            var updateInfoCmd = new IdentifiedCommand<UpdateFinancialProfileCommand, FinancialProfile>(command, new Guid());

            try
            {
                if (updateInfoCmd != null)
                {
                    commandResult = await _mediator.Send(updateInfoCmd);
                    return Ok();
                }
                else
                {
                    return NoContent();
                }
            }
            catch (InvalidDataException e)
            {
                this._logger.LogError(e, "An error occurred while updating financial profile for party {PartyId} on application {ApplicationNumber}", partyId, applicationNumber);
                return BadRequest(new { message = e.Message });
            }
        }

        [HttpGet]
        [Route("applications/{application-number:long}/involved-parties/{party-id:int}/contact-points")]
        public IActionResult GetPartyContactPoints([FromRoute]long applicationNumber, [FromRoute]int partyId)
        {

            var result = _involvedPartyRepository.GetPartyContactPoints(applicationNumber, partyId);
            if (result != null)
            {
                return Ok(result);
            }
            return NoContent();

        }

        [Authorize]
        [HttpPut]
        [Route("applications/{application-number:long}/involved-parties/{party-id:int}/contact-points")]
        public async Task<IActionResult> UpdatePartyContactPoints([FromRoute] long applicationNumber, [FromRoute] int partyId, [FromBody] ContactPoints contactPoints)
        {
            try
            {
                bool auditLog = AuditLog();
                var updatedParty = await _involvedPartyRepository.UpdatePartyContactPoints(applicationNumber, partyId, contactPoints, auditLog);
                if (updatedParty != null)
                {
                    return Ok();
                }
                return NoContent();
            }
            catch (InvalidDataException e)
            {
                return BadRequest(new { message = e.Message });
            }
        }

        [HttpGet]
        [Route("applications/{application-number:long}/involved-parties/{party-id:int}/credit-bureau")]
        public async Task<IActionResult> GetPartyCreditBureauData([FromRoute]long applicationNumber, [FromRoute]int partyId)
        {
            var report = await _involvedPartyRepository.GetPartyCreditBureauData(applicationNumber, partyId);
            if (report != null)
            {
                return Ok(report);
            }
            return NoContent();
        }

        [Authorize]
        [HttpPut]
        [Route("applications/{application-number:long}/involved-parties/{party-id:int}/credit-bureau")]
        public async Task<IActionResult> UpdatePartyCreditBureauData([FromRoute] long applicationNumber, [FromRoute] int partyId, [FromBody] CreditBureauData creditBureauData)
        {
            try
            {
                bool auditLog = AuditLog();
                var updatedParty = await _involvedPartyRepository.UpdatePartyCreditBureauData(applicationNumber, partyId, creditBureauData, auditLog);
                if (updatedParty != null)
                {
                    return Ok();
                }
                return NoContent();
            }
            catch (InvalidDataException e)
            {
                this._logger.LogError(e, "An error occurred while updating CB Data for party {PartyId} on application {ApplicationNumber}", partyId, applicationNumber);
                return BadRequest(new { message = e.Message });
            }
            catch (Exception e)
            {
                this._logger.LogError(e, "An error occurred while updating CB Data for party {PartyId} on application {ApplicationNumber}", partyId, applicationNumber);
                return StatusCode(500);
            }
        }

        [Authorize]
        [HttpPost]
        [Route("applications/{application-number:long}/pull-party-data")]
        public async Task<IActionResult> PullPartyDataAsync([FromRoute]long applicationNumber)
        {
            bool auditLog = AuditLog();
            ApplicationView commandResult;
            var PullPartyDataCommand = new IdentifiedCommand<PullPartyDataCommand, ApplicationView>(new PullPartyDataCommand(applicationNumber, auditLog), new Guid());
            commandResult = await _mediator.Send(PullPartyDataCommand);
            return commandResult != null ? (IActionResult)Ok() : NotFound();
        }

        [HttpGet]
        [Route("applications/{application-number:long}/involved-parties/{party-id:int}/financial-statements")]
        public async Task<IActionResult> GetPartyFinancialStatements([FromRoute] long applicationNumber, [FromRoute] int partyId)
        {
            var financialStatements = await _involvedPartyRepository.GetPartyFinancialStatements(applicationNumber, partyId);
            if (financialStatements != null)
            {
                return Ok(new { FinancialStatements = financialStatements });
            }
            return NoContent();
        }

        [HttpGet]
        [Route("applications/{application-number:long}/involved-parties/{party-id:int}/data-completeness")]
        public async Task<IActionResult> GetDataCompleteness([FromRoute] long applicationNumber, [FromRoute] int partyId)
        {
            GetDataCompletenessCommand command = new GetDataCompletenessCommand
            {
                ApplicationNumber = applicationNumber,
                PartyId = partyId
            };
            var getDataCompleteness = new IdentifiedCommand<GetDataCompletenessCommand, DataCompletenessResponse>(command, new Guid());
            var result = await _mediator.Send(getDataCompleteness);

            if (result == null)
            {
                return new JsonResult(new { message = "Party with id: " + partyId + ", does not exist for application: " + applicationNumber});
            }
            return Ok(result);
        }

        [HttpPatch]
        [Route("applications/{application-number:long}/involved-parties/{party-id:int}")]
        public async Task<IActionResult> PatchInvolvedParty([FromRoute] long applicationNumber, [FromRoute] int partyId, [FromBody] PatchInvolvedPartyCommand command)
        {
            command.ApplicationNumber = applicationNumber;
            command.PartyId = partyId;
            var patchDataCompleteness = new IdentifiedCommand<PatchInvolvedPartyCommand, CommandStatus>(command, new Guid());
            CommandStatus commandResult = await _mediator.Send(patchDataCompleteness);

            if (commandResult.CommandResult.Equals(StandardCommandResult.OK))
            {
                return Ok();
            }
            else if (commandResult.CommandResult.Equals(StandardCommandResult.BAD_REQUEST))
            {
                return BadRequest();
            }
            else if (commandResult.CommandResult.Equals(StandardCommandResult.NOT_FOUND))
            {
                return BadRequest();
            }
            else if (commandResult.Equals(StandardCommandResult.INTERNAL_ERROR))
            {
                _logger.LogError(commandResult.Exception, "An error occurred while patching data completeness to party");
                return StatusCode(500);
            }
            else
            {
                _logger.LogError("An unknown error occurred while patching data completeness to party");
                return StatusCode(500);
            }
        }


        private bool IsCustomerOrAgent(string userType)
        {
            userType = userType ?? "";
            return userType.Equals("Customer") || userType.Equals("Prospect") || userType.ToLower().Contains("agent");
        }

        private bool AuditLog()
        {
            var userType = this.User.Claims.Where(x => x.Type == "user_type").FirstOrDefault()?.Value;
            return IsCustomerOrAgent(userType);
        }

    }
}
