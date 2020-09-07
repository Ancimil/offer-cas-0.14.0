using System.Threading.Tasks;

namespace Offer.Domain.AggregatesModel.ApplicationAggregate
{
    public  interface IFinancialStatementsService
    {
        Task<FinancialStatementsData> GetFinancialStatementAsync(string customerNumber, string reportType, string accountingMethod, int year);
    }
}
