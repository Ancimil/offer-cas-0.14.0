using AssecoCurrencyConvertion;
using MediatR;
using MicroserviceCommon.API.ApiUtils;
using MicroserviceCommon.Application.Commands;
using MicroserviceCommon.Filters;
using MicroserviceCommon.Models;
using MicroserviceCommon.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Offer.API.Application.Commands;
using Offer.API.Application.Commands.Application;
using Offer.API.Application.Filter;
using Offer.Domain.AggregatesModel.ApplicationAggregate;
using Offer.Domain.Exceptions;
using PriceCalculation.Models.Pricing;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using InstallmentPlanApp = Offer.Domain.AggregatesModel.ApplicationAggregate.InstallmentPlanRow;

namespace Offer.API.Controllers
{
    [ValidateModel]
    [Authorize]
    [ApplicationPhaseLockFilter]
    [Route("v1/offer")]
    public class DocumentController : Controller
    {
        private readonly IMediator _mediator;
        private readonly IApplicationRepository _applicationRepository;
        private readonly IApplicationDocumentRepository _applicationDocumentRepository;
        private readonly ILogger<DocumentController> _logger;
        private readonly IConfigurationService _configurationService;

        public DocumentController(
            IApplicationRepository applicationRepository,
            IApplicationDocumentRepository applicationDocumentRepository,
            IMediator mediator,
            ILogger<DocumentController> logger,
            IConfigurationService configurationService)
        {
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this._mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            this._configurationService = configurationService ?? throw new ArgumentNullException(nameof(configurationService));
            this._applicationDocumentRepository = applicationDocumentRepository;
            this._applicationRepository = applicationRepository;
        }

        [HttpGet]
        [Route("applications/{application-number:long}/documents")]
        public IActionResult GetApplicationDocumentsAsync([FromRoute]long applicationNumber, [FromQuery]string statuses, [FromQuery] string documentNames, [FromQuery]bool? isForSigning,
        [FromQuery]bool? isForUpload, [FromQuery]bool? isComposedFromTemplate, [FromQuery]string documentKinds, [FromQuery]string documentContext, [FromQuery] string collateralId)

        {
            Console.WriteLine("Offer - EndPoint GET documents !!!");
            var dateBeforeCall = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff",
                                            CultureInfo.InvariantCulture);
            Console.WriteLine("Before Call GET documents {0}", dateBeforeCall);
            List<DocumentStatus> statusList = null;
            if (statuses != null && statuses != "")
                statusList = EnumUtils.GetEnumPropertiesForListString<DocumentStatus>(statuses);
            List<DocumentContextKind> contextList = null;
            if (documentContext != null && documentContext != "")
                contextList = EnumUtils.GetEnumPropertiesForListString<DocumentContextKind>(documentContext);
            var list = _applicationDocumentRepository.GetApplicationDocuments(applicationNumber, statusList, isForSigning, isForUpload, isComposedFromTemplate, documentKinds, documentNames, contextList, collateralId);
            if (list != null)
            {
                var dateAfterCall = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff",
                                           CultureInfo.InvariantCulture); ;
                Console.WriteLine("Offer GET documents - After Call: {0} ", dateAfterCall);

                double milDiff = DateTime.Parse(dateAfterCall).Subtract(DateTime.Parse(dateBeforeCall)).TotalMilliseconds;
                Console.WriteLine("Offer GET documents - END - COUNT: {0} !!!", milDiff);
                return new ObjectResult(list);
            }
            return NotFound();
        }

        [HttpPost]
        [Route("applications/{application-number:long}/documents")]
        public async Task<IActionResult> CreateApplicationDocumentAsync([FromRoute]long applicationNumber, [FromBody]CreateApplicationDocumentCommand command)
        {
            Console.WriteLine("Offer - EndPoint POST documents !!!");
            var dateBeforeCall = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff",
                                            CultureInfo.InvariantCulture);
            Console.WriteLine("Before Call POST documents {0}", dateBeforeCall);
            command.ApplicationNumber = applicationNumber;
            var createApplicationDocumentCommand = new IdentifiedCommand<CreateApplicationDocumentCommand, CommandStatus<ApplicationDocument>>(command, new Guid());
            var commandResult = await _mediator.Send(createApplicationDocumentCommand);
            if (commandResult.CommandResult == StandardCommandResult.OK)
            {
                var dateAfterCall = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff",
                                          CultureInfo.InvariantCulture); ;
                Console.WriteLine("Offer POST documents - After Call: {0} ", dateAfterCall);

                double milDiff = DateTime.Parse(dateAfterCall).Subtract(DateTime.Parse(dateBeforeCall)).TotalMilliseconds;
                Console.WriteLine("Offer POST documents - END - COUNT: {0} !!!", milDiff);
                return new ObjectResult(commandResult.Result);
            }
            else if (commandResult.CommandResult == StandardCommandResult.NOT_FOUND)
            {
                return NotFound();
            }
            else
            {
                return StatusCode(500);
            }
        }

        [HttpPost]
        [Route("applications/{application-number:long}/documents/update-status")]
        public async Task<IActionResult> UpdateDocumentsStatusAsync([FromRoute]long applicationNumber, [FromBody]UpdateApplicationDocumentsStatusCommand command)
        {
            Console.WriteLine("Offer - EndPoint POST update-status documents !!!");
            var dateBeforeCall = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff",
                                            CultureInfo.InvariantCulture);
            Console.WriteLine("Before Call POST update-status documents {0}", dateBeforeCall);
            command.ApplicationId = applicationNumber;
            var updateApplicationDocumentCommand = new IdentifiedCommand<UpdateApplicationDocumentsStatusCommand, bool?>(command, new Guid());
            bool? commandResult = await _mediator.Send(updateApplicationDocumentCommand);
            var dateAfterCall = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff",
                                          CultureInfo.InvariantCulture); ;
            Console.WriteLine("Offer POST update-status documents - After Call: {0} ", dateAfterCall);

            double milDiff = DateTime.Parse(dateAfterCall).Subtract(DateTime.Parse(dateBeforeCall)).TotalMilliseconds;
            Console.WriteLine("Offer POST update-status documents - END - COUNT: {0} !!!", milDiff);
            return commandResult.HasValue ? commandResult.Value ? (IActionResult)Ok() : (IActionResult)BadRequest() : NotFound();
        }

        [HttpDelete]
        [Route("applications/{application-number:long}/documents/{document-id}")]
        public async Task<IActionResult> DeleteApplicationDocumentAsync([FromRoute]long applicationNumber, [FromRoute]string documentId)
        {
            var command = new DeleteApplicationDocumentCommand
            {
                ApplicationNumber = applicationNumber,
                DocumentId = documentId
            };
            try
            {
                var deleteApplicationDocumentCommand = new IdentifiedCommand<DeleteApplicationDocumentCommand, bool?>(command, new Guid());
                var commandResult = await _mediator.Send(deleteApplicationDocumentCommand);
                return commandResult.HasValue ? commandResult.Value ? (IActionResult)Ok()
                    : (IActionResult)BadRequest(new { message = "Something went wrong. Document not deleted." }) : NotFound();
            }
            catch (ApplicationDocumentNotFoundException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException e)
            {
                return BadRequest(new { message = e.Message });
            }
            catch
            {
                return BadRequest(new { message = "Something went wrong. Document not deleted." });
            }
        }

        [HttpGet]
        [Route("applications/{application-number:long}/data-for-document-composition")]
        public async Task<IActionResult> GetOfferDataForCompositionAsync([FromRoute]long applicationNumber)
        {
            Console.WriteLine("Offer - EndPoint POST data-for-document-composition !!!");
            var dateBeforeCall = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff",
                                            CultureInfo.InvariantCulture);
            Console.WriteLine("Before Call POST data-for-document-composition {0}", dateBeforeCall);
            var application = await _applicationRepository.GetAsync(applicationNumber, "arrangement-requests,involved-parties");
            if (application != null)
            {
                string conversionMethod = await _configurationService.GetEffective("offer/fee-currency-conversion", "Buy to middle");
                string domesticCurrency = await _configurationService.GetEffective("domestic-currency", "RSD");
                application.ArrangementRequests = application.ArrangementRequests.Where(a => a.Enabled ?? false).ToList();
                var interestingRequests = application.ArrangementRequests
                    .Where(ar =>
                        ar.ArrangementKind == ArrangementKind.CreditCardFacility ||
                        ar.ArrangementKind == ArrangementKind.OverdraftFacility ||
                        ar.ArrangementKind == ArrangementKind.TermLoan);
                foreach (ArrangementRequest req in interestingRequests)
                {
                    req.SerializeTotals = true;
                    foreach (InstallmentPlanApp row in req.InstallmentPlan)
                    {
                        req.TotalDisbursement += row.Disbursement;
                        req.TotalAnnuity += row.Annuity;
                        req.TotalPrincipal += row.PrincipalRepayment;
                        req.TotalInterest += row.InterestRepayment;
                        req.TotalExpenses += (row.Fee + row.OtherExpenses);
                        req.TotalCashCollateral += row.CashCollateral;
                        req.TotalNetCashFlow += row.NetCashFlow;
                        req.TotalDiscountedNetCashFlow += row.DiscountedNetCashFlow;
                        req.TotalRepaymentAmount += (row.Fee + row.InterestRepayment + row.PrincipalRepayment + row.OtherExpenses);
                    }

                    if (req is FinanceServiceArrangementRequest financeServiceReq)
                    {
                        if (domesticCurrency == financeServiceReq.Currency)
                        {
                            req.TotalExpensesInDomesticCurrency = req.TotalExpenses;
                        }
                        else
                        {
                            req.TotalExpensesInDomesticCurrency = new CurrencyConverter().CurrencyConvert(
                                financeServiceReq.TotalExpenses, financeServiceReq.Currency, domesticCurrency,
                                DateTime.Today.ToString("o", CultureInfo.InvariantCulture), conversionMethod);
                        }
                    }

                    foreach (InterestRateCondition rate in req.Conditions.InterestRates
                        .Where(x => x.Kind == InterestRateKinds.RegularInterest))
                    {
                        if (rate.IsFixed) req.InterestRateVariability = "fixed";
                        else req.InterestRateVariability = "variable";

                        if (rate.IsCompound) req.CalculationMethod = "compound";
                        else req.CalculationMethod = "simple";
                    }
                }
                var dateAfterCall = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff",
                                          CultureInfo.InvariantCulture); ;
                Console.WriteLine("Offer POST data-for-document-composition - After Call: {0} ", dateAfterCall);

                double milDiff = DateTime.Parse(dateAfterCall).Subtract(DateTime.Parse(dateBeforeCall)).TotalMilliseconds;
                Console.WriteLine("Offer POST data-for-document-composition - END - COUNT: {0} !!!", milDiff);
                return new ObjectResult(application);
            }
            else
            {
                return NotFound();
            }
        }

        [HttpPost]
        [Route("applications/{application-number:long}/validate-required-documentation")]
        public async Task<IActionResult> ValidateDocumentationRequirement([FromRoute]long applicationNumber, [FromBody] RequiredDocumentationValidationCommand command)
        {
            if (command == null)
            {
                this._logger.LogInformation("bad request because command is null");
                return BadRequest();
            }
            command.ApplicationNumber = applicationNumber;
            var AddAccountNumbersCommand = new IdentifiedCommand<RequiredDocumentationValidationCommand, DocumentationValidationResponse>(command, new Guid());
            try
            {
                var commandResult = await _mediator.Send(AddAccountNumbersCommand);
                if (command != null)
                {
                    return
                        commandResult.Items != null &&
                           commandResult.Items.Any() ? (IActionResult)Ok(commandResult) : (IActionResult)Ok();
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception e)
            {
                this._logger.LogError("there was an error while validation milestone required documentation: {0},{1},{2} ", e.Message, e.StackTrace, e.InnerException);
                return StatusCode(500);
            }

        }
    }
}
