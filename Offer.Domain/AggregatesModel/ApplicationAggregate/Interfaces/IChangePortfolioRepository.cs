using System.Threading.Tasks;

namespace Offer.Domain.AggregatesModel.ApplicationAggregate.Interfaces
{
    public interface IChangePortfolioRepository
    {
        Task<PortfolioChangeRequests> GetRequest(long applicationNumber, long portfolioChangeRequestId);
        Task<PortfolioChangeRequests> PostPortfolioChangeRequests(PortfolioChangeRequests portfolioChangeRequest, bool auditLog);
        Task<PortfolioChangeRequests> UpdatePortfolioChangeRequests(PortfolioChangeRequests portfolioChangeRequest, bool auditLog);
    }
}
