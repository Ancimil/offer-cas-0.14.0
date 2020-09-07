using System;
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
using MicroserviceCommon.Authentication;

namespace Offer.API.Controllers
{
    [Route("v1/offer")]
    [Authorize]
    public class GdprController : Controller
    {
        private readonly IMediator _mediator;
        private readonly IApplicationRepository _applicationRepository;
        private readonly ILogger<GdprController> _logger;
        private readonly UserInfoRetrieval _userInfoRetrieval;

        public GdprController(IApplicationRepository applicationRepository,
            IMediator mediator,
            ILogger<GdprController> logger,
            UserInfoRetrieval userInfoRetrieval)
        {
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this._applicationRepository = applicationRepository ?? throw new ArgumentNullException(nameof(applicationRepository));
            this._userInfoRetrieval = userInfoRetrieval ?? throw new ArgumentNullException(nameof(userInfoRetrieval));
            this._mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        [HttpGet]
        [Route("applications/export")]
        public IActionResult ExportGdprData([FromQuery] string username, [FromQuery] string customerNumber)
        {
            if (string.IsNullOrEmpty(username) && string.IsNullOrEmpty(customerNumber))
            {
                return BadRequest(new { message = "Neither username nor customer number are not specified or they are empty strings" });
            }
            try
            {
                var matcher = new PartyMatcher(username, customerNumber);
                var gdprData = _applicationRepository.ExportGdprData(matcher);
                return Content(gdprData, "application/json");
            }
            catch (DuplicateObjectException e)
            {
                return BadRequest(new { message = e.Message });
            }
        }

        [HttpPost]
        [Route("applications/anonymize")]
        public async Task<IActionResult> AnonymizeGdprData([FromBody] AnonymizeGdprDataCommand message)
        {
            try
            {
                var anonymizeCommand = new IdentifiedCommand<AnonymizeGdprDataCommand, string>(message, new Guid());
                var commandResult = await _mediator.Send(anonymizeCommand);
                return Content(commandResult, "application/json");
            }
            catch (DuplicateObjectException e)
            {
                Console.WriteLine(e);
                return BadRequest(new { message = e.Message });
            }
            catch (ArgumentException e)
            {
                Console.WriteLine(e);
                return BadRequest(new { message = e.Message });
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return BadRequest(new { message = "An unknown error occurred." });
            }
        }

        [HttpPost]
        [Route("gdpr/initiate/export")]
        public async Task<IActionResult> InitiateGdprExportProcess([FromBody] InitiateGdprProcessCommand cmd)
        {
            try
            {
                cmd = await AppendUserDataToCommand(cmd);
                cmd.ProcessKey = GdprProcessEnum.Export;
            }
            catch (ArgumentNullException e)
            {
                _logger.LogWarning(e, "Argument for GDPR process initialization is not defined.");
                return (IActionResult)BadRequest(new { message = e.Message });
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An error occurred while initializing GDPR process.");
                return StatusCode(500);
            }

            var initiateProcessCommand = new IdentifiedCommand<InitiateGdprProcessCommand, string>(cmd, new Guid());
            var commandResult = await _mediator.Send(initiateProcessCommand);
            return commandResult == null ? (IActionResult)Ok() : (IActionResult)BadRequest(new { message = commandResult });
        }

        [HttpPost]
        [Route("gdpr/initiate/anonymize")]
        public async Task<IActionResult> InitiateGdprAnonymizeProcess([FromBody] InitiateGdprProcessCommand cmd)
        {
            try
            {
                cmd = await AppendUserDataToCommand(cmd);
                cmd.ProcessKey = GdprProcessEnum.Anonymize;
            }
            catch (ArgumentNullException e)
            {
                _logger.LogWarning(e, "Argument for GDPR process initialization is not defined.");
                return (IActionResult)BadRequest(new { message = e.Message });
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An error occurred while initializing GDPR process.");
                return StatusCode(500);
            }

            var initiateProcessCommand = new IdentifiedCommand<InitiateGdprProcessCommand, string>(cmd, new Guid());
            var commandResult = await _mediator.Send(initiateProcessCommand);
            return commandResult == null ? (IActionResult)Ok() : (IActionResult)BadRequest(new { message = commandResult });
        }

        [HttpPost]
        [Route("gdpr/initiate/correction")]
        public async Task<IActionResult> InitiateGdprCorrectionProcess([FromBody] InitiateGdprProcessCommand cmd)
        {
            try
            {
                cmd = await AppendUserDataToCommand(cmd);
                cmd.ProcessKey = GdprProcessEnum.Correction;
            }
            catch (ArgumentNullException e)
            {
                _logger.LogWarning(e, "Argument for GDPR process initialization is not defined.");
                return (IActionResult)BadRequest(new { message = e.Message });
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An error occurred while initializing GDPR process.");
                return StatusCode(500);
            }

            var initiateProcessCommand = new IdentifiedCommand<InitiateGdprProcessCommand, string>(cmd, new Guid());
            var commandResult = await _mediator.Send(initiateProcessCommand);
            return commandResult == null ? (IActionResult)Ok() : (IActionResult)BadRequest(new { message = commandResult });
        }

        private async Task<InitiateGdprProcessCommand> AppendUserDataToCommand(InitiateGdprProcessCommand cmd)
        {
            cmd.UserType = this.User.Claims.Where(x => x.Type == "user_type").FirstOrDefault()?.Value;
            var username = IsCustomer(cmd.UserType) ? this.User.Claims.Where(x => x.Type == "preferred_username").FirstOrDefault()?.Value : cmd.Username;
            if (string.IsNullOrEmpty(username))
            {
                throw new ArgumentNullException("Username is not available");
            }
            var userData = await _userInfoRetrieval.GetUserData(username);
            if (string.IsNullOrEmpty(userData.Email))
            {
                throw new ArgumentNullException("Email is not available");
            }
            cmd.Username = userData.Username;
            cmd.CustomerNumber = userData.CustomerNumber;
            cmd.Email = userData.Email;
            cmd.PhoneNumber = userData.PhoneNumber;
            cmd.FirstName = userData.FirstName;
            cmd.LastName = userData.LastName;
            cmd.Initiator = this.User.Claims.Where(x => x.Type == "preferred_username").FirstOrDefault()?.Value;
            return cmd;
        }

        private bool IsCustomer(string userType)
        {
            return string.IsNullOrEmpty(userType) || userType.Equals("Customer") || userType.Equals("Prospect");
        }
    }
}
