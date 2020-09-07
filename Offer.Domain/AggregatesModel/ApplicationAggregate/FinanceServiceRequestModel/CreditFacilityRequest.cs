using MicroserviceCommon.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace Offer.Domain.AggregatesModel.ApplicationAggregate
{
    public class CreditFacilityRequest :FinanceServiceArrangementRequest
    {
        public Currency MinimalRepaymentAmount { get; set; }
        [Column("MinRepPerc")]
        public decimal MinimalRepaymentPercentage { get; set; }
    }
}
