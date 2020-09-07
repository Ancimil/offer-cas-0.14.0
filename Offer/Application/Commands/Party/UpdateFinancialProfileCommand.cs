using MediatR;
using Offer.Domain.AggregatesModel.ApplicationAggregate.ApplicantModel;
using System.Collections.Generic;

namespace Offer.API.Application.Commands
{
    public class UpdateFinancialProfileCommand : IRequest<FinancialProfile>
    {
        public long ApplicationNumber { get; set; }
        public int PartyId { get; set; }
        public List<Income> IncomeInfo { get; set; }
        public List<Expense> ExpenseInfo { get; set; }

        public UpdateFinancialProfileCommand(List<Income> incomeInfo, List<Expense> expenseInfo)
        {
            IncomeInfo = incomeInfo;
            ExpenseInfo = expenseInfo;
        }
    }
}
