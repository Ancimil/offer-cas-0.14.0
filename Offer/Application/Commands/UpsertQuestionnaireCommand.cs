using System;
using System.Collections.Generic;
using MediatR;

namespace Offer.API.Application.Commands
{

    public class UpsertQuestionnaireCommand : IRequest<bool?>
    {
        public long ApplicationNumber { get; set; }
        public DateTime Date { get; private set; }
        public Dictionary<string, object> Entries { get; set; }
        public string QuestionnaireName { get; set; }
        public string Purpose { get; set; }
        public string QuestionnaireId { get; set; }

        public UpsertQuestionnaireCommand(long applicationNumber, Dictionary<string, object> entries, string questionnaireName, string purpose, string questionnaireId)
        {
            ApplicationNumber = applicationNumber;
            Date = DateTime.UtcNow.Date;
            Entries = entries;
            QuestionnaireName = questionnaireName;
            QuestionnaireId = questionnaireId;
            Purpose = purpose;
        }
    }
}
