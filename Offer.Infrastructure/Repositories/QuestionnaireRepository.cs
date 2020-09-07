using Offer.Domain.AggregatesModel.ApplicationAggregate;
using System;
using MicroserviceCommon.Domain.SeedWork;
using System.Linq;

namespace Offer.Infrastructure.Repositories
{
    public class QuestionnaireRepository : IQuestionnaireRepository
    {
        private readonly OfferDBContext _context;
        private readonly IApplicationRepository _applicationRepository;

        public IUnitOfWork UnitOfWork
        {
            get
            {
                return _context;
            }
        }

        public QuestionnaireRepository(
            OfferDBContext context,
            IApplicationRepository applicationRepository
         )
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _applicationRepository = applicationRepository ?? throw new ArgumentNullException(nameof(applicationRepository));
        }

        public GenericQuestionnaire AddGenericQuestionnaire(GenericQuestionnaire questionnaire)
        {
            GenericQuestionnaire createdQuestionnaire = (GenericQuestionnaire) _context.Questionnaires.Add(questionnaire).Entity;
            return createdQuestionnaire;
        }

        public void UpdateGenericQuestionaire(GenericQuestionnaire questionnaire)
        {
            _context.Update(questionnaire);
        }

        public Questionnaire GetQuestionnaire(long applicationNumber, string questionnaireId)
        {
            var application = _applicationRepository.GetAsync(applicationNumber).Result;
            if (application == null)
            {
                return null;
            }
            return _context.Questionnaires.Where(q => q.ApplicationId == applicationNumber && q.QuestionnaireId.Equals(questionnaireId))
                .FirstOrDefault();
        }

        public QuestionnaireList GetQuestionnaires(long applicationNumber)
        {
            var application = _applicationRepository.GetAsync(applicationNumber).Result;
            if (application == null)
            {
                return null;
            }
            var questionnaires = new QuestionnaireList
            {
                Questionnaires = _context.Questionnaires.Where(q => q.ApplicationId == applicationNumber).ToList()
            };
            return questionnaires;
        }
    }
}
