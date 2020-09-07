using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Offer.Domain.AggregatesModel.ApplicationAggregate;
using Offer.API.Application.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using MicroserviceCommon.API.ApiUtils;
using MicroserviceCommon.Application.Commands;
using System.Globalization;
using AssecoCurrencyConvertion;
using Microsoft.Extensions.Logging;
using MicroserviceCommon.ApiUtil;
using Offer.Domain.Exceptions;
using MicroserviceCommon.Controllers;
using MicroserviceCommon.Infrastructure.Repository;
using MicroserviceCommon.Filters;
using MicroserviceCommon.Models;
using Offer.Domain.View;
using Offer.API.Application.Filter;
using Offer.API.Application.Commands.Application;
using Offer.Domain.AggregatesModel.ApplicationAggregate.Interfaces;
using MicroserviceCommon.Services;
using Offer.Domain.View.AllDataViews;
using Offer.API.Application.Commands.Product;

namespace Offer.Controllers
{
    [ApplicationPhaseLockFilter]
    [ValidateModel]
    [Route("v1/offer")]
    public class OfferController : Controller
    {

        private readonly IMediator _mediator;
        private readonly IApplicationRepository _applicationRepository;
        private readonly IChangePortfolioRepository _changePortfolioRepository;
        private readonly ILogger<OfferController> _logger;
        private readonly IConfigurationService _configurationService;

        public OfferController(IApplicationRepository applicationRepository,
            IChangePortfolioRepository changePortfolioRepository,
            IMediator mediator,
            ILogger<OfferController> logger,
            IConfigurationService configurationService)
        {
            this._logger = logger;
            this._applicationRepository = applicationRepository;
            _changePortfolioRepository = changePortfolioRepository;
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _configurationService = configurationService ?? throw new ArgumentNullException(nameof(configurationService));
        }

        #region Reporting
        [HttpGet]
        [Authorize]
        [Route("reporting/applications-by-status")]
        public IActionResult GetApplicationsByStatus()
        {
            return Ok(_applicationRepository.GetApplicationsByStatus());
        }


        [HttpGet]
        [Authorize]
        [Route("applications")]
        public async Task<IActionResult> GetApplications([FromQuery]string statuses, [FromQuery]string kinds, [FromQuery] string productCode,
            [FromQuery] string customerData, [FromQuery] string statusFromDate, [FromQuery] string dateFrom, [FromQuery] string dateTo,
            [FromQuery]string include, [FromQuery]List<string> trim, [FromQuery]int? page, [FromQuery]int? pageSize, [FromQuery]string sortBy,
            [FromQuery]string sortOrder, [FromQuery] string applicationNumber, [FromQuery] string customerNumber,
            [FromQuery] string partyRoles, [FromQuery] string channels, [FromQuery] string expirationDateFrom, [FromQuery] string expirationDateTo, [FromQuery] string initiator)
        {
            Console.WriteLine("Offer - EndPoint GET application");
            var dateBeforeCall = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff",
                                            CultureInfo.InvariantCulture);
            Console.WriteLine("Before Call {0}", dateBeforeCall);
            List<ApplicationStatus> statusList = null;
            if (!string.IsNullOrEmpty(statuses))
                statusList = EnumUtils.GetEnumPropertiesForListString<ApplicationStatus>(statuses);

            List<ArrangementKind?> kindList = null;
            if (kinds != null && kinds != "")
            {
                kindList = EnumUtils.GetEnumPropertiesForListString<ArrangementKind>(kinds).ConvertAll<ArrangementKind?>(i => i);

            }
            List<string> partyList = null;
            if (!string.IsNullOrEmpty(partyRoles))
            {
                partyList = partyRoles.Split(",").Select(p => p.Trim()).ToList();
            }
            List<string> channelList = null;
            if (!string.IsNullOrEmpty(channels))
            {
                channelList = channels.Split(",").Select(c => c.Trim()).ToList();
            }

            PagedApplicationList applications = await _applicationRepository.GetApplications(statusList, kindList, productCode, customerData,
                statusFromDate, dateFrom, dateTo, include, trim, page, pageSize, sortBy, sortOrder, applicationNumber, customerNumber, partyList, channelList, expirationDateFrom, expirationDateTo, initiator);
            if (applications != null)
            {
                string[] inclusions = string.IsNullOrEmpty(include) ? new string[] { } : include.Split(',');
                var dateAfterCall = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff",
                                            CultureInfo.InvariantCulture); ;
                Console.WriteLine("ID {0} - After Call: {1} ", applicationNumber, dateAfterCall);

                double milDiff = DateTime.Parse(dateAfterCall).Subtract(DateTime.Parse(dateBeforeCall)).TotalMilliseconds;
                Console.WriteLine("ID {0} - COUNT: {1}", applicationNumber, milDiff);
                return new ObjectResult(applications);
            }
            return BadRequest(new { message = "Bad request" });
        }

        [Authorize]
        [HttpPost]
        [Route("applications")]
        public async Task<IActionResult> PostApplicationOnline([FromBody]InitiateOnlineOfferCommand command)
        {
            Console.WriteLine("Offer - EndPoint POST application !!!");
            var dateBeforeCall = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff",
                                            CultureInfo.InvariantCulture);
            Console.WriteLine("Before Call {0}", dateBeforeCall);
            var userType = this.User.Claims.Where(x => x.Type == "user_type").FirstOrDefault()?.Value;
            var usernameClaim = this.User.Claims.Where(x => x.Type == "preferred_username").FirstOrDefault();
            command.IsCustomer = IsCustomer(userType);
            var isAgent = IsAgent(userType);
            if (isAgent)
            {
                command.AgentOrganizationUnit = this.User.Claims.Where(x => x.Type == "main_organization_unit").FirstOrDefault()?.Value;
            }
            if (IsRobot())
            {
                command.Username = GetRobotUsername();
            }
            else
            {
                string userId = usernameClaim?.Value;
                if (userId == null)
                {
                    return ErrorHelper.GetRequiredValidationErrorResult("user");
                }
                command.Username = userId;
            }

            _logger.LogInformation("User {Username} is posting application.", command.Username);

            if (command.CustomerNumber == null)
            {
                usernameClaim = this.User.Claims.Where(x => x.Type == "customer_number").FirstOrDefault();
                command.CustomerNumber = usernameClaim?.Value;
            }

            if (!isAgent && string.IsNullOrEmpty(command.CustomerNumber) && string.IsNullOrEmpty(command.Surname))
            {
                _logger.LogError("No customer number and no customer data provided when applying online!");
                return (IActionResult)BadRequest(new { error = "No customer number and no customer data provided when applying online" });
            }

            if ((command.DownpaymentAmount > 0 && Math.Round(command.InvoiceAmount - command.DownpaymentAmount - command.Amount, 2) != 0) ||
                (command.DownpaymentAmount == 0 && command.InvoiceAmount > 0 && Math.Round(command.InvoiceAmount - command.Amount, 2) != 0))
            {
                _logger.LogError("Downpayment amount and requested amount doesn't match invoice amount!");
                return (IActionResult)BadRequest(new { error = "Downpayment amount and requested amount doesn't match invoice amount" });
            }

            var requestApplicationNumber = new IdentifiedCommand<InitiateOnlineOfferCommand, IntialOnlineOfferCommandResult>
                (command, new Guid());
            try
            {
                IntialOnlineOfferCommandResult onlineOfferCommandResult = await _mediator.Send(requestApplicationNumber);
                if (onlineOfferCommandResult.Result == CommandResult.OK && onlineOfferCommandResult.ApplicationNumber != null)
                {
                    var dateAfterCall = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff",
                                            CultureInfo.InvariantCulture); ;
                    Console.WriteLine("Offer - After Call: {0} ", dateAfterCall);

                    double milDiff = DateTime.Parse(dateAfterCall).Subtract(DateTime.Parse(dateBeforeCall)).TotalMilliseconds;
                    Console.WriteLine("Offer - END - COUNT: {0} !!!", milDiff);
                    return (IActionResult)Ok(new { onlineOfferCommandResult.ApplicationNumber });
                }
                else if (onlineOfferCommandResult.Result == CommandResult.LEAD_ALREADY_EXIST)
                    return (IActionResult)BadRequest(new { error = "Lead Already Exist. Application hasn't been created" });
                else
                    return (IActionResult)BadRequest(new { error = "Something went wrong. Application hasn't been created" });
            }
            catch (InvalidCastException e)
            {
                _logger.LogWarning(e, "Cast Exception occurred while creating application");
                return BadRequest(new { error = e.Message });
            }
            catch (InvalidCalculationException e)
            {
                return (IActionResult)BadRequest(new { error = e.Message });
            }
        }


        [HttpGet]
        [Authorize]
        [Route("application-list")]
        public async Task<IActionResult> GetApplicationsList([FromQuery]string statuses, [FromQuery]string kinds, [FromQuery] string productCode,
            [FromQuery] string customerData, [FromQuery] string statusFromDate, [FromQuery] string dateFrom, [FromQuery] string dateTo,
            [FromQuery]string include, [FromQuery]List<string> trim, [FromQuery]int? page, [FromQuery]int? pageSize, [FromQuery]string sortBy,
            [FromQuery]string sortOrder, [FromQuery] string applicationNumber, string customerNumber, string partyRoles, string channels, [FromQuery] string expirationDateFrom, [FromQuery] string expirationDateTo, [FromQuery] string initiator)
        {
            List<ApplicationStatus> statusList = null;
            if (!string.IsNullOrEmpty(statuses))
                statusList = EnumUtils.GetEnumPropertiesForListString<ApplicationStatus>(statuses);

            List<ArrangementKind?> kindList = null;
            if (kinds != null && kinds != "")
            {
                kindList = EnumUtils.GetEnumPropertiesForListString<ArrangementKind>(kinds).ConvertAll<ArrangementKind?>(i => i);

            }
            List<string> partyList = null;
            if (!string.IsNullOrEmpty(partyRoles))
            {
                partyList = partyRoles.Split(",").Select(p => p.Trim()).ToList();
            }
            List<string> channelList = null;
            if (!string.IsNullOrEmpty(channels))
            {
                channelList = channels.Split(",").Select(c => c.Trim()).ToList();
            }

            ApplicationList applications = await _applicationRepository.GetApplicationsList(statusList, kindList, productCode, customerData,
                statusFromDate, dateFrom, dateTo, include, trim, page, pageSize, sortBy, sortOrder, applicationNumber, customerNumber, partyList, channelList, expirationDateFrom, expirationDateTo, initiator);

            if (applications != null)
            {
                return new ObjectResult(applications);
            }

            return BadRequest(new { message = "Bad request" });
        }


        #endregion
        [HttpGet]
        [Authorize]
        [Route("applications/{application-number:long}")]
        public IActionResult GetApplication([FromRoute]long applicationNumber, [FromQuery]string include, [FromQuery]string trim)
        {
            ApplicationDetailsView application = _applicationRepository.GetAsyncDetailsView(applicationNumber, include, trim).Result;
            if (application != null)
            {
                string[] inclusions = string.IsNullOrEmpty(include) ? new string[] { } : include.Split(',');
                if (!inclusions.Contains("exposure-info"))
                {
                    application.ExposureInfo = null;
                }
                return new ObjectResult(application);
            }
            else
            {
                return NotFound();
            }
        }

        [HttpPut]
        [Authorize]
        [Route("applications/{application-number:long}")]
        public async Task<IActionResult> UpdateApplication([FromRoute]long applicationNumber, [FromBody] UpdateApplicationCommand command)
        {
            command.ApplicationNumber = applicationNumber;
            command.AuditLog = AuditLog();
            var updateCustomerCommand = new IdentifiedCommand<UpdateApplicationCommand, CommandStatus>(command, new Guid());
            CommandStatus commandResult = await _mediator.Send(updateCustomerCommand);
            if (commandResult.CommandResult.Equals(StandardCommandResult.OK))
            {
                return Ok();
            }
            else if (commandResult.CommandResult.Equals(StandardCommandResult.NOT_FOUND))
            {
                return NotFound();
            }
            else
            {
                return StatusCode(500);
            }
        }

        [HttpPatch]
        [Authorize]
        [Route("applications/{application-number:long}")]
        public async Task<IActionResult> PatchApplication([FromRoute] long applicationNumber, [FromBody] PatchApplicationCommand command)
        {
            command.ApplicationNumber = applicationNumber;
            command.AuditLog = AuditLog();
            var patchApplicationCommand = new IdentifiedCommand<PatchApplicationCommand, CommandStatus>(command, new Guid());
            CommandStatus commandResult = await _mediator.Send(patchApplicationCommand);
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

        [HttpGet]
        [Authorize]
        [Route("applications/{application-number:long}/all-data")]
        public async Task<IActionResult> GetApplicationWithAllData([FromRoute]long applicationNumber)
        {
            try
            {
                var username = GetUsernameFromToken();
                var auditLog = AuditLog();
                string conversionMethod = await _configurationService.GetEffective("offer/fee-currency-conversion", "Buy to middle");
                string domesticCurrency = await _configurationService.GetEffective("domestic-currency", "RSD");
                var application = await _applicationRepository.GetApplicationWithAllData(applicationNumber, username, auditLog);

                var interestingRequests = application.ArrangementRequests
                    .Where(ar => ar.Calculation != null);
                foreach (var req in interestingRequests)
                {
                    if (req is FinanceServiceRequestAllDataView financeServiceReq)
                    {
                        if (domesticCurrency == financeServiceReq.Currency)
                        {
                            req.Calculation.TotalExpensesInDomesticCurrency = req.Calculation.TotalExpenses;
                        }
                        else
                        {
                            req.Calculation.TotalExpensesInDomesticCurrency = new CurrencyConverter().CurrencyConvert(
                                financeServiceReq.Calculation.TotalExpenses ?? 0, financeServiceReq.Currency, domesticCurrency,
                                DateTime.Today.ToString("o", CultureInfo.InvariantCulture), conversionMethod);
                        }
                    }
                }

                return Ok(application);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        [HttpPost]
        [Authorize]
        [Route("applications/{application-number:long}/portfolio-change-request")]
        public async Task<IActionResult> ChangePortfolioPost([FromRoute] long applicationNumber, [FromBody] ChangePortfolioCommand command)
        {
            command.ApplicationNumber = applicationNumber;
            command.AuditLog = AuditLog();
            var changePortfolioCommand = new IdentifiedCommand<ChangePortfolioCommand, CommandStatus>(command, new Guid());
            CommandStatus commandResult = await _mediator.Send(changePortfolioCommand);
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

        [HttpPut]
        [Authorize]
        [Route("applications/{application-number:long}/portfolio-change-request")]
        public async Task<IActionResult> ChangePortfolioPut([FromRoute] long applicationNumber, [FromBody] ChangePortfolioAcceptCommand command)
        {
            command.ApplicationNumber = applicationNumber;
            command.AuditLog = AuditLog();
            var changePortfolioAcceptCommand = new IdentifiedCommand<ChangePortfolioAcceptCommand, CommandStatus>(command, new Guid());
            CommandStatus commandResult = await _mediator.Send(changePortfolioAcceptCommand);
            if (commandResult.CommandResult.Equals(StandardCommandResult.OK))
            {
                PatchApplicationCommand patchCommand = new PatchApplicationCommand(applicationNumber, command.FinalValue);
                await PatchApplication(applicationNumber, patchCommand);
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

        [HttpGet]
        [Route("applications/{application-number:long}/portfolio-change-request/{portfolio-change-request-id:long}")]
        public async Task<IActionResult> GetPortfolioChangeRequest([FromRoute] long applicationNumber, long portfolioChangeRequestId)
        {
            var result = await _changePortfolioRepository.GetRequest(applicationNumber, portfolioChangeRequestId);
            if (result != null)
            {
                return new ObjectResult(result);
            }
            return NotFound();
        }

        [Authorize]
        [HttpPost]
        [Route("applications/bind-to-customer")]
        public async Task<IActionResult> UpdateCustomerNumberAsync([FromBody]UpdateCustomerNumberCommand command)
        {
            command.AuditLog = AuditLog();
            bool commandResult;
            if (string.IsNullOrEmpty(command.CustomerNumber) || string.IsNullOrEmpty(command.Username))
            {
                return BadRequest("Missing parameter!");
            }
            var updateCustomerNumberCommand = new IdentifiedCommand<UpdateCustomerNumberCommand, bool>(command, new Guid());
            commandResult = await _mediator.Send(updateCustomerNumberCommand);
            return commandResult ? Ok() : (IActionResult)BadRequest();
        }


        [Authorize]
        [HttpPost]
        [Route("applications/{application-number:long}/continue")]
        public async Task<IActionResult> ContinueApplication([FromRoute] long applicationNumber, [FromBody] ContinueApplicationCommand command)
        {
            command.ApplicationNumber = applicationNumber;
            command.Username = GetUsernameFromToken();
            command.AuditLog = AuditLog();

            var continueApplicationCommand = new IdentifiedCommand<ContinueApplicationCommand, CommandStatus>(command, new Guid());
            CommandStatus commandStatus = await _mediator.Send(continueApplicationCommand);

            if (commandStatus.CommandResult.Equals(StandardCommandResult.OK))
            {
                return Ok();
            }
            else if (commandStatus.CommandResult.Equals(StandardCommandResult.BAD_REQUEST))
            {
                if (commandStatus.CustomError != null)
                {
                    return BadRequest(commandStatus.CustomError);
                }
                return BadRequest();
            }
            else if (commandStatus.CommandResult.Equals(StandardCommandResult.NOT_FOUND))
            {
                return NotFound();
            }
            else
            {
                return StatusCode(500);
            }
        }

        [HttpPut]
        [Route("applications/{application-number:long}/extended")]
        public async Task<IActionResult> PostExtendedData([FromRoute] long applicationNumber,
            [FromBody] API.Application.Commands.Application.PutExtendedPartyCommand command)
        {
            command.ApplicationNumber = applicationNumber;
            var postExtendedDataCommand = new IdentifiedCommand<API.Application.Commands.Application.PutExtendedPartyCommand, CommandStatus>(command, new Guid());
            CommandStatus commandResult = await _mediator.Send(postExtendedDataCommand);

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
                return NotFound();
            }
            else if (commandResult.Equals(StandardCommandResult.INTERNAL_ERROR))
            {
                _logger.LogError(commandResult.Exception, "An error occurred while putting extended sections to application");
                return StatusCode(500);
            }
            else
            {
                _logger.LogError("An unknown error occurred while putting extended sections to application");
                return StatusCode(500);
            }
        }

        [HttpGet]
        [Route("applications/{application-number:long}/extended")]
        public async Task<IActionResult> GetExtended([FromRoute] long applicationNumber)
        {
            try
            {
                var data = await _applicationRepository.GetExtendedData(applicationNumber);
                if (data != null)
                {
                    return new ObjectResult(data);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An exception occurred while getting application extended data");
                return StatusCode(500);
            }
        }

        [HttpGet]
        [Route("applications/{application-number:long}/extended/{section-name}")]
        public async Task<IActionResult> GetExtendedSection([FromRoute] long applicationNumber, [FromRoute] string sectionName)
        {
            var data = await _applicationRepository.GetExtendedDataSection(applicationNumber, sectionName);

            if (data == null)
            {
                return NotFound();
            }
            return new ObjectResult(data);
        }

        [HttpDelete]
        [Route("applications/{application-number:long}/extended/{section-name}")]
        public async Task<IActionResult> DeleteExtendedSection([FromRoute] long applicationNumber, [FromRoute] string sectionName)
        {
            try
            {
                var isDeleted = await _applicationRepository.DeleteExtendedDataSection(applicationNumber, sectionName);
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

        [HttpPost]
        [Route("applications/{application-number:long}/refresh-current-exposure")]
        public async Task<IActionResult> RetrieveCurrentExposure([FromRoute]long applicationNumber)
        {
            Console.WriteLine("Offer - EndPoint POST RetrieveCurrentExposure !!!");
            var dateBeforeCall = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff",
                                            CultureInfo.InvariantCulture);
            Console.WriteLine("Before Call RetrieveCurrentExposure {0}", dateBeforeCall);

            var RetrieveCurrentExposureCommand = new IdentifiedCommand<RetrieveCurrentExposureCommand, CommandStatus>(new RetrieveCurrentExposureCommand(applicationNumber, GetUsernameFromToken()), new Guid());
            CommandStatus commandResult = await _mediator.Send(RetrieveCurrentExposureCommand);
            if (commandResult.CommandResult.Equals(StandardCommandResult.OK))
            {
                var dateAfterCall = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff",
                                            CultureInfo.InvariantCulture); ;
                Console.WriteLine("Offer RetrieveCurrentExposure - After Call: {0} ", dateAfterCall);

                double milDiff = DateTime.Parse(dateAfterCall).Subtract(DateTime.Parse(dateBeforeCall)).TotalMilliseconds;
                Console.WriteLine("Offer RetrieveCurrentExposure - END - COUNT: {0} !!!", milDiff);
                return Ok();
            }
            else if (commandResult.CommandResult.Equals(StandardCommandResult.NOT_FOUND))
            {
                return NotFound();
            }
            else
            {
                return StatusCode(500);
            }
        }

        [HttpPost]
        [Route("applications/{application-number:long}/calculate-credit-bureau-exposure")]
        public async Task<IActionResult> RetrieveCreditBureauExposure([FromRoute] long applicationNumber, [FromBody] RetrieveCreditBureauExposureCommand command)
        {
            command.ApplicationNumber = applicationNumber;
            var creditBureauExposureCommand = new IdentifiedCommand<RetrieveCreditBureauExposureCommand, CommandStatus>(command, new Guid());
            var commandResult = await _mediator.Send(creditBureauExposureCommand);
            if (commandResult.CommandResult.Equals(StandardCommandResult.OK))
            {
                return Ok();
            }
            else if (commandResult.CommandResult.Equals(StandardCommandResult.NOT_FOUND))
            {
                return NotFound();
            }
            else
            {
                return StatusCode(500);
            }
        }

        [HttpPost]
        [Route("applications/{application-number:long}/calculate-total-exposure")]
        public async Task<IActionResult> CalculateNewExposure([FromRoute]long applicationNumber, [FromBody]CalculateNewExposureCommand command)
        {

            Console.WriteLine("Offer - EndPoint POST calculate-total-exposure !!!");
            var dateBeforeCall = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff",
                                            CultureInfo.InvariantCulture);
            Console.WriteLine("Before Call calculate-total-exposure {0}", dateBeforeCall);

            if (command == null)
            {
                return BadRequest(new { message = "Command not present!" });
            }
            command.ApplicationNumber = applicationNumber;

            if (command.RetrieveCurrentExposure == true)
            {
                await RetrieveCurrentExposure(applicationNumber);
            }
            var CalculateNewExposureCommand = new IdentifiedCommand<CalculateNewExposureCommand, CommandStatus<Currency>>(command, new Guid());
            var totalExposureCommandResult = await _mediator.Send(CalculateNewExposureCommand);

            if (totalExposureCommandResult.CommandResult == StandardCommandResult.OK)
            {
                var dateAfterCall = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff",
                                           CultureInfo.InvariantCulture); ;
                Console.WriteLine("Offer calculate-total-exposure - After Call: {0} ", dateAfterCall);

                double milDiff = DateTime.Parse(dateAfterCall).Subtract(DateTime.Parse(dateBeforeCall)).TotalMilliseconds;
                Console.WriteLine("Offer calculate-total-exposure - END - COUNT: {0} !!!", milDiff);
                return new ObjectResult(totalExposureCommandResult.Result);
            }
            else if (totalExposureCommandResult.CommandResult == StandardCommandResult.NOT_FOUND)
            {
                return NotFound();
            }
            else
            {
                return StatusCode(500);
            }
        }

        // TODO GetAvailableProductsForApplication
        [HttpGet]
        [Authorize]
        [Route("applications/{application-number}/available-products")]
        public async Task<IActionResult> GetAvailableProductsForApplication([FromRoute] long applicationNumber, [FromQuery] string channelCode,
            [FromQuery] string customerId)
        {
            var msg = new GetAvailableProductsCommand
            {
                ApplicationId = applicationNumber,
                ChannelCode = channelCode,
                CustomerId = customerId
            };
            var getAvailableProductsCmd = new IdentifiedCommand<GetAvailableProductsCommand, AvailableProductsResponse>(msg, new Guid());
            var availableProducts = await _mediator.Send(getAvailableProductsCmd);
            return Ok(availableProducts);
        }

        [HttpPost]
        [Authorize]
        [Route("applications/{application-number}/available-products")]
        public async Task<IActionResult> PostAvailableProducts([FromRoute] long applicationNumber, [FromBody] AddAvailableProductsCommand command)
        {
            command.ApplicationNumber = applicationNumber;
            var addAvailableProducts = new IdentifiedCommand<AddAvailableProductsCommand, CommandStatus>(command, new Guid());
            CommandStatus commandStatus = await _mediator.Send(addAvailableProducts);

            if (commandStatus.CommandResult.Equals(StandardCommandResult.OK))
            {
                return Ok();
            }
            else if (commandStatus.CommandResult.Equals(StandardCommandResult.BAD_REQUEST))
            {
                if (commandStatus.CustomError != null)
                {
                    return BadRequest(commandStatus.CustomError);
                }
                return BadRequest();
            }
            else if (commandStatus.CommandResult.Equals(StandardCommandResult.NOT_FOUND))
            {
                return NotFound();
            }
            else
            {
                return StatusCode(500);
            }
        }

        [HttpDelete]
        [Authorize]
        [Route("applications/{application-number}/available-products/{product-code}")]
        public async Task<IActionResult> DeleteAvailableProduct([FromRoute] long applicationNumber, [FromRoute] string productCode)
        {
            DeleteAvailableProductsCommand command = new DeleteAvailableProductsCommand 
            { 
                Products = new List<string>(new string[] { productCode })
            };
            command.ApplicationNumber = applicationNumber;
            var deleteAvailableProducts = new IdentifiedCommand<DeleteAvailableProductsCommand, CommandStatus>(command, new Guid());
            CommandStatus commandStatus = await _mediator.Send(deleteAvailableProducts);

            if (commandStatus.CommandResult.Equals(StandardCommandResult.OK))
            {
                return Ok();
            }
            else if (commandStatus.CommandResult.Equals(StandardCommandResult.BAD_REQUEST))
            {
                if (commandStatus.CustomError != null)
                {
                    return BadRequest(commandStatus.CustomError);
                }
                return BadRequest();
            }
            else if (commandStatus.CommandResult.Equals(StandardCommandResult.NOT_FOUND))
            {
                return NotFound();
            }
            else
            {
                return StatusCode(500);
            }
        }

        private string GetUsernameFromToken()
        {
            return this.User?.Claims?.Where(x => x.Type == "preferred_username").FirstOrDefault()?.Value;
        }

        private bool IsCustomer(string userType)
        {
            userType = userType ?? "";
            return userType.Equals("Customer") || userType.Equals("Prospect");
        }
        private bool IsAgent(string userType)
        {
            userType = userType ?? "";
            return userType.ToLower().Contains("agent");
        }

        private bool IsRobot()
        {
            var xAgent = HttpContext.Request.Headers["X-Agent"].ToString();
            return !string.IsNullOrEmpty(xAgent) && xAgent.Contains("robot");
        }

        private string GetRobotUsername()
        {
            return HttpContext.Request.Headers["X-Agent"];
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

    [Route("v1/offer")]
    public class OfferClassController : ClassificationController
    {
        public OfferClassController(ClassificationRepository classificationRepository) : base(classificationRepository)
        {

        }
    }
}
