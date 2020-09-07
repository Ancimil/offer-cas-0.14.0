using MediatR;
using MicroserviceCommon.Models;
using Offer.Domain.AggregatesModel.ApplicationAggregate;

namespace Offer.API.Application.Commands
{
    public class UpdateHouseholdInfoCommand : IRequest<Household>
    {
        public long ApplicationNumber { get; set; }
        public int PartyId { get; set; }
        public Currency TotalHouseholdIncome { get; set; }
        public int SizeOfHousehold { get; set; } = 0;
        public int EmployedHousholdMembers { get; set; } = 0;
        public int DependentChildren { get; set; } = 0;
        public int DependentAdults { get; set; } = 0;
        
        public UpdateHouseholdInfoCommand() {
        
        }
        

        public UpdateHouseholdInfoCommand(Currency totalHouseholdIncome, int sizeOfHousehold, int employedHousholdMembers,
            int dependentChildren, int dependentAdults)
        {
            TotalHouseholdIncome = totalHouseholdIncome;
            SizeOfHousehold = sizeOfHousehold;
            EmployedHousholdMembers = employedHousholdMembers;
            DependentChildren = dependentChildren;
            DependentAdults = dependentAdults;
        }
    }
}
