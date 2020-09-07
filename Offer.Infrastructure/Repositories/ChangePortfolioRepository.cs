using AuditClient;
using AuditClient.Model;
using MicroserviceCommon.Domain.SeedWork;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using Offer.Domain.AggregatesModel.ApplicationAggregate;
using Offer.Domain.AggregatesModel.ApplicationAggregate.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Offer.Infrastructure.Repositories
{
    public class ChangePortfolioRepository : IChangePortfolioRepository
    {
        private readonly OfferDBContext _context;
        private readonly IAuditClient _auditClient;

        public ChangePortfolioRepository(OfferDBContext context, IAuditClient auditClient)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _auditClient = auditClient;
        }

        public IUnitOfWork UnitOfWork
        {
            get
            {
                return _context;
            }
        }

        public async Task<PortfolioChangeRequests> GetRequest(long applicationNumber, long portfolioChangeRequestId)
        {
            try
            {
                PortfolioChangeRequests portfolioChangeRequests = await _context.PortfolioChangeRequests.
                                    Where(p => p.PortfolioChangeRequestId.Equals(portfolioChangeRequestId)
                                        && p.ApplicationId.Equals(applicationNumber)).FirstOrDefaultAsync();
                if (portfolioChangeRequests != null)
                {
                    await _auditClient.WriteLogEntry(AuditLogEntryAction.Get, AuditLogEntryStatus.Success, "portfolio", applicationNumber.ToString(), "Portfolio change request", portfolioChangeRequests);
                    return portfolioChangeRequests;
                } else
                {
                    return null;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<PortfolioChangeRequests> PostPortfolioChangeRequests(PortfolioChangeRequests portfolioChangeRequest, bool auditLog)
        {
            try
            {                                
                PortfolioChangeRequests _savedRequest = _context.PortfolioChangeRequests.Add(portfolioChangeRequest).Entity;
                await _context.SaveChangesAsync();
                if (auditLog)
                {
                    await _auditClient.WriteLogEntry(AuditLogEntryAction.Add, AuditLogEntryStatus.Success, "portfolio", portfolioChangeRequest.ApplicationNumber.ToString(), "Portfolio added", _savedRequest);
                }
                return _savedRequest;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<PortfolioChangeRequests> UpdatePortfolioChangeRequests(PortfolioChangeRequests portfolioChangeRequest, bool auditLog)
        {
                PortfolioChangeRequests _portfolioChangeRequest = await _context.PortfolioChangeRequests
                            .Where(p => p.PortfolioChangeRequestId.Equals(portfolioChangeRequest.PortfolioChangeRequestId)).FirstOrDefaultAsync();
                if (_portfolioChangeRequest != null)
                {
                    _portfolioChangeRequest.FinalValue = portfolioChangeRequest.FinalValue;
                    _portfolioChangeRequest.Status = ChangeRequestsKindApp.UpdateCompleted;

                    PortfolioChangeRequests _savedRequest = _context.PortfolioChangeRequests.Update(_portfolioChangeRequest).Entity;
                    await _context.SaveChangesAsync();

                if (auditLog)
                {
                    await _auditClient.WriteLogEntry(AuditLogEntryAction.Update, AuditLogEntryStatus.Success, "portfolio", portfolioChangeRequest.ApplicationNumber.ToString(), "Portfolio has been updated", _savedRequest);
                }

                return _savedRequest;
                }else
                {
                    return null;
                }
        }
    }
}
