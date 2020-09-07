using System;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Offer.Domain.AggregatesModel.ApplicationAggregate;
using MicroserviceCommon.Application.Commands;
using MicroserviceCommon.Filters;
using Offer.API.Application.Commands.ArrangementRequests.Collateral;
using Offer.API.Application.Commands.ArrangementRequests;

namespace Offer.API.Controllers
{
    [ValidateModel]
    [Authorize]
    [Route("v1/offer")]
    public class CollateralController : Controller
    {
        private readonly IMediator _mediator;
        private readonly IArrangementRequestRepository _arrangementRequestRepository;

        public CollateralController(
            IArrangementRequestRepository arrangementRequestRepository,
            IMediator mediator)
        {
            this._mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            this._arrangementRequestRepository = arrangementRequestRepository ?? throw new ArgumentNullException(nameof(arrangementRequestRepository));
        }


        [HttpGet]
        [Route("applications/{application-number:long}/arrangement-requests/{arrangement-request-id:int}/collateral-requirements")]
        public IActionResult GetCollateralRequrements([FromRoute]long applicationNumber, [FromRoute]int arrangementRequestId)
        {
            var arrRequest = _arrangementRequestRepository.GetArrangementRequest(applicationNumber, arrangementRequestId, "collateral-requirements", null);
            if (arrRequest == null || !(arrRequest is FinanceServiceArrangementRequest))
            {
                return NotFound();
            }
            return Ok(new { ((FinanceServiceArrangementRequest)arrRequest).CollateralRequirements });
        }

        [HttpPost]
        [Route("applications/{application-number:long}/arrangement-requests/{arrangement-request-id:int}/collateral-requirements")]
        public async Task<IActionResult> AddCollateralRequirement([FromRoute]long applicationNumber, [FromRoute]int arrangementRequestId, [FromBody]AddCollateralRequirementCommand command)
        {
            command.ApplicationNumber = applicationNumber;
            command.ArrangementRequestId = arrangementRequestId;
            bool? commandResult;
            var addCollateralCommand = new IdentifiedCommand<AddCollateralRequirementCommand, bool?>(command, new Guid());
            commandResult = await _mediator.Send(addCollateralCommand);
            return commandResult.HasValue ? commandResult.Value ? (IActionResult)StatusCode(201) : (IActionResult)BadRequest() : NotFound();
        }


        [HttpGet]
        [Route("applications/{application-number:long}/arrangement-requests/{arrangement-request-id:int}/collateral-requirements/{collateral-requirement-id}")]
        public IActionResult GetCollateralRequirement([FromRoute] long applicationNumber, [FromRoute] int arrangementRequestId, [FromRoute] long collateralRequirementId)
        {
            var result = _arrangementRequestRepository.GetCollateralRequirementById(applicationNumber, arrangementRequestId, collateralRequirementId);
            if (result != null)
            {
                return Ok(result);
            }
            else
            {
                return NotFound();
            }
        }

        [Authorize]
        [HttpPut]
        [Route("applications/{application-number:long}/arrangement-requests/{arrangement-request-id:int}/collateral-requirements/{collateral-requirement-id}")]
        public async Task<IActionResult> UpdateCollateralRequirement([FromRoute] long applicationNumber, [FromRoute] int arrangementRequestId, [FromRoute] long collateralRequirementId, [FromBody] UpdateCollateralRequirementCommand collateralRequirement)
        {
            collateralRequirement.ApplicationId = applicationNumber;
            collateralRequirement.ArrangementRequestId = arrangementRequestId;
            collateralRequirement.CollateralRequirementId = collateralRequirementId;
            var commandResult = await _mediator.Send(collateralRequirement);
            return commandResult.HasValue ? commandResult.Value ? (IActionResult)StatusCode(201) : (IActionResult)BadRequest() : NotFound();
        }

        [HttpDelete]
        [Route("applications/{application-number:long}/arrangement-requests/{arrangement-request-id:int}/collateral-requirements/{collateral-requirement-id}")]
        public async Task<IActionResult> DeleteCollateralRequirement([FromRoute] long applicationNumber, [FromRoute] int arrangementRequestId, [FromRoute] long collateralRequirementId)
        {
            var result = await _arrangementRequestRepository.DeleteCollateralRequirement(applicationNumber, arrangementRequestId, collateralRequirementId);
            if (result.HasValue && result.Value)
            {
                return Ok();
            }
            else
            {
                return NotFound();
            }
        }

        [HttpPost]
        [Route("applications/{application-number:long}/arrangement-requests/{arrangement-request-id:int}/change-collateral-model")]
        public async Task<IActionResult> ChangeCollateralModel([FromRoute]long applicationNumber, [FromRoute]int arrangementRequestId, [FromBody] UpdateCollateralModelCommand command)
        {
            command.ApplicationNumber = applicationNumber;
            command.ArrangementRequestId = arrangementRequestId;
            var updateCollateralModel = new IdentifiedCommand<UpdateCollateralModelCommand, bool?>(command, new Guid());
            var commandResult = await _mediator.Send(updateCollateralModel);

            if (commandResult.HasValue)
            {
                if (commandResult.Value)
                {

                    var recalculateCommand = new RecalculateArrangementRequestCommand
                    {
                        ApplicationNumber = applicationNumber,
                        ArrangementRequestId = arrangementRequestId

                    };
                    var recalculateArrangementRequestCommand = new IdentifiedCommand<RecalculateArrangementRequestCommand, bool?>(recalculateCommand, new Guid());
                    commandResult = await _mediator.Send(recalculateArrangementRequestCommand);
                    return commandResult.HasValue && commandResult.Value ? (IActionResult)Ok() : (IActionResult)BadRequest();
                }
                else
                {
                    return BadRequest();
                }
            }
            else
            {
                return NotFound();
            }
        }

        [HttpPost]
        [Route("applications/{application-number:long}/arrangement-requests/{arrangement-request-id:int}/collateral-requirements/validation")]
        public IActionResult ValidateCollateralRequirement([FromRoute]long applicationNumber, [FromRoute]int arrangementRequestId)
        {
            var result = _arrangementRequestRepository.ValidateCollateralRequirement(applicationNumber, arrangementRequestId);
            if (result != null)
            {
                return Ok(result);
            }
            return NotFound();
        }


    }
}
