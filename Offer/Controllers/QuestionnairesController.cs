using MediatR;
using MicroserviceCommon.Application.Commands;
using MicroserviceCommon.Filters;
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
    [Authorize]
    [ApplicationPhaseLockFilter]
    [Route("v1/offer")]
    public class QuestionnairesController : Controller
    {
        private readonly IMediator _mediator;
        private readonly ILogger<InvolvedPartyController> _logger;
        private readonly IQuestionnaireRepository _questionnaireRepository;

        public QuestionnairesController(
            IMediator mediator,
            IQuestionnaireRepository questionnaireRepository,
            ILogger<InvolvedPartyController> logger)
        {
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this._questionnaireRepository = questionnaireRepository;
            this._mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }
        [HttpGet]
        [Route("applications/{application-number:long}/questionnaires")]
        public IActionResult GetQuestionnaires([FromRoute]long applicationNumber)
        {
            QuestionnaireList questionnaires = _questionnaireRepository.GetQuestionnaires(applicationNumber);
            if (questionnaires != null)
            {
                return new ObjectResult(questionnaires);
            }
            return NotFound();
        }

        [HttpPost]
        [Route("applications/{application-number:long}/questionnaires")]
        public async Task<IActionResult> UpsertQuestionnaire([FromRoute] long applicationNumber, [FromBody] UpsertQuestionnaireCommand command)
        {
            bool? commandResult;
            command.ApplicationNumber = applicationNumber;
            _logger.Log(LogLevel.Information, "UpserQuestionaire for applicationNumber: " + applicationNumber);
            var upsertQuestionnaire = new IdentifiedCommand<UpsertQuestionnaireCommand, bool?>(command, new Guid());
            commandResult = await _mediator.Send(upsertQuestionnaire);
            return commandResult.HasValue ? commandResult.Value ? (IActionResult)Ok() : (IActionResult)BadRequest() : NotFound();
        }


        [HttpGet]
        [Route("applications/{application-number:long}/questionnaires/{questionnaire-id}")]
        public IActionResult GetQuestionnaire([FromRoute]long applicationNumber, [FromRoute] string questionnaireId)
        {
            Questionnaire questionnaire = _questionnaireRepository.GetQuestionnaire(applicationNumber, questionnaireId);
            if (questionnaire != null)
            {
                return new ObjectResult(questionnaire);
            }
            return NotFound();
        }

        [HttpPost]
        [Route("applications/{application-number:long}/questionnaires/{questionnaire-id}")]
        public async Task<IActionResult> UpsertQuestionnaire([FromRoute] long applicationNumber, [FromRoute] string questionnaireId, [FromBody] UpsertQuestionnaireCommand command)
        {
            command.ApplicationNumber = applicationNumber;
            command.QuestionnaireId = questionnaireId;
            var upsertQuestionnaire = new IdentifiedCommand<UpsertQuestionnaireCommand, bool?>(command, new Guid());
            var commandResult = await _mediator.Send(upsertQuestionnaire);
            return commandResult.HasValue ? commandResult.Value ? (IActionResult)Ok() : (IActionResult)BadRequest() : NotFound();
        }
    }
}
