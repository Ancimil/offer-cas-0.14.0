using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Offer.API.Application.Commands
{
    public class GetDataCompletenessCommand : IRequest<DataCompletenessResponse>
    {
        public long ApplicationNumber { get; set; }
        public int PartyId { get; set; }
    }

    public class DataCompletenessResponse
    {
        public bool? EmailVerfied { get; set; }
        public bool? IsRegisteredAsCustomer { get; set; }
        public bool? PartyDataLoaded { get; set; }
        public bool? ProfileDataLoaded { get; set; }
        public bool? IsRegisteredAsProspect { get; set; }
        public string ProspectNumber { get; set; }
        public bool? HasRepresentative { get; set; }
        public bool? IdDataComplete { get; set; }
        public bool? IdDataVerified { get; set; }
        public bool? IncomeStated { get; set; }
        public bool? IncomeVerified { get; set; }
        public bool? EmploymentDataStated { get; set; }
        public bool? EmploymentDataVerified { get; set; }
        public bool? HouseholdInformationStated { get; set; }
        public bool? HouseholdInformationVerified { get; set; }
        public bool? KycQuestionnaireFilled { get; set; }
        public string ConsentsGiven { get; set; }
    }
}
