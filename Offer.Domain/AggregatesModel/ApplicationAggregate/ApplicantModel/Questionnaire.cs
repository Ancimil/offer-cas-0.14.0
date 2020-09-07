using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Offer.Domain.AggregatesModel.ApplicationAggregate
{
    public abstract class Questionnaire
    {
        [Required]
        [JsonIgnore]
        public long ApplicationId { get; set; } // references parent app number, part of key

        [NotMapped]
        public string ApplicationNumber
        {
            get
            {
                var result = "0000000000" + ApplicationId;
                return result.Substring(result.Length - 10);
            }
        }
        //public int PartyId { get; set; } // references parent party, part of key // removed for siplicity
        [MaxLength(256)]
        public string QuestionnaireId { get; set; } // part of key
        [MaxLength(256)]
        public string Purpose { get; set; } // KYC, FATCA, Security, Risk...
        public DateTime Date { get; set; }
        [MaxLength(256)]
        public string QuestionnaireName { get; set; } // key at conf. repo for question specification
    }

    public class GenericQuestionnaire: Questionnaire
    {
        [JsonIgnore]
        public string _Entries { get; set; }
        [NotMapped]
        public Dictionary<string, object> Entries
        {
            get { return _Entries == null ? null : JsonConvert.DeserializeObject<Dictionary<string, object>>(_Entries); }
            set
            {
                _Entries = JsonConvert.SerializeObject(value);
            }
        } // persist as json (question/answer => key/value)
    }

    public class FinancialQuestionnaire: Questionnaire
    {
        [JsonIgnore]
        public string _Entries { get; set; }
        [NotMapped]
        public List<FinancialEntry> Entries
        {
            get { return _Entries == null ? null : JsonConvert.DeserializeObject<List<FinancialEntry>>(_Entries); }
            set
            {
                _Entries = JsonConvert.SerializeObject(value);
            }
        } // persist as json
    }

    [NotMapped]
    public class FinancialEntry
    {
        [MaxLength(256)]
        public string Kind { get; set; } // kind or unique position code (read from conf repo due to questionnaire name)
        public string Description { get; set; }
        public decimal Amount { get; set; } // allow different currencies in single statement
        [MaxLength(10)]
        public string Currency { get; set; } // allow different currencies in single statement
        public decimal ConvertedAmount { get; set; } // calculate amount in target currency
    }

    // provide concrete classes for well known questionnaires
    public class FATCAQuestionnaire: Questionnaire
    {
        [JsonIgnore]
        public string _Entries { get; set; }
        [NotMapped]
        public List<FATCAEntry> Entries
        {
            get { return _Entries == null ? null : JsonConvert.DeserializeObject<List<FATCAEntry>>(_Entries); }
            set
            {
                _Entries = JsonConvert.SerializeObject(value);
            }
        } // persist as json
    }
    [NotMapped]
    public class FATCAEntry
    {
        public bool AccountOwner { get; set; }
        public bool RelatedCustomers { get; set; }
        public bool PoliticallyExposedPerson { get; set; }
        public bool InfluenceGroup { get; set; }
        public bool BankAffiliated { get; set; }
        public bool IsAmericanCitizen { get; set; }
    }
}
