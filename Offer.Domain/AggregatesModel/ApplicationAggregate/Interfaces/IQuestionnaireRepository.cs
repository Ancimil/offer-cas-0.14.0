using MicroserviceCommon.Domain.SeedWork;

namespace Offer.Domain.AggregatesModel.ApplicationAggregate
{
    public interface IQuestionnaireRepository : IRepository<Application>
    {
        GenericQuestionnaire AddGenericQuestionnaire(GenericQuestionnaire questionnaire);
        void UpdateGenericQuestionaire(GenericQuestionnaire questionnaire);
        Questionnaire GetQuestionnaire(long applicationNumber, string questionnaireId);
        QuestionnaireList GetQuestionnaires(long applicationNumber);
    }
}
