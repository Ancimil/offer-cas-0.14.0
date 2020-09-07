using System.Threading.Tasks;
using MediatR;
using MicroserviceCommon.Application.Commands;
using MicroserviceCommon.Infrastructure.Idempotency;
using Offer.Domain.AggregatesModel.ApplicationAggregate;
using System.Threading;
using AuditClient;
using AuditClient.Model;

namespace Offer.API.Application.Commands
{
    public class UpsertQuestionnaireCommandHandler : IRequestHandler<UpsertQuestionnaireCommand, bool?>
    {
        private readonly IQuestionnaireRepository _questionnaireRepository;
        private readonly IApplicationRepository _applicationRepository;
        private readonly IAuditClient _auditClient;

        public UpsertQuestionnaireCommandHandler(
            IQuestionnaireRepository questionnaireRepository,
             IApplicationRepository applicationRepository, IAuditClient auditClient)
        {
            this._questionnaireRepository = questionnaireRepository;
            this._applicationRepository = applicationRepository;
            _auditClient = auditClient;
        }

        public async Task<bool?> Handle(UpsertQuestionnaireCommand message, CancellationToken cancellationToken)
        {
            var application = _applicationRepository.GetAsync(message.ApplicationNumber).Result;
            if (application == null)
            {
                return null;
            }
            var questionnaire = (GenericQuestionnaire)_questionnaireRepository.GetQuestionnaire(message.ApplicationNumber, message.QuestionnaireId);
            if (questionnaire == null)
            {
                questionnaire = CreateQuestionnaire(message);
                await _auditClient.WriteLogEntry(AuditLogEntryAction.Create, AuditLogEntryStatus.Success, "questionnaire", application.ApplicationNumber, "Questionnaire created", questionnaire);
            }
            else
            {
                UpdateQuestionnaire(message, questionnaire);
                await _auditClient.WriteLogEntry(AuditLogEntryAction.Update, AuditLogEntryStatus.Success, "questionnaire", application.ApplicationNumber, "Questionnaire updated", questionnaire);
            }
            
            return await _questionnaireRepository.UnitOfWork.SaveEntitiesAsync();
        }

        private GenericQuestionnaire CreateQuestionnaire(UpsertQuestionnaireCommand message)
        {
            var questionnaire = new GenericQuestionnaire
            {
                ApplicationId = message.ApplicationNumber,
                QuestionnaireName = message.QuestionnaireName,
                QuestionnaireId = message.QuestionnaireId,
                Purpose = message.Purpose,
                Date = message.Date,
                Entries = message.Entries
            };
            _questionnaireRepository.AddGenericQuestionnaire(questionnaire);
            return questionnaire;
        }

        private void UpdateQuestionnaire(UpsertQuestionnaireCommand message, GenericQuestionnaire questionnaire)
        {
            questionnaire.Entries = message.Entries;
            questionnaire.QuestionnaireName = message.QuestionnaireName;
            questionnaire.Purpose = message.Purpose;
            _questionnaireRepository.UpdateGenericQuestionaire(questionnaire);
        }
    }

    public class UpsertQuestionnaireCommandHandlerIdentifiedHandler : IdentifiedCommandHandler<UpsertQuestionnaireCommand, bool?>
    {
        public UpsertQuestionnaireCommandHandlerIdentifiedHandler(IMediator mediator, IRequestManager requestManager) : base(mediator, requestManager)
        {

        }
    }
}
