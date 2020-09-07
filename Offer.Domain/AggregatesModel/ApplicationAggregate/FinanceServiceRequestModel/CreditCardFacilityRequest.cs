using MicroserviceCommon.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace Offer.Domain.AggregatesModel.ApplicationAggregate
{
    public partial class CreditCardFacilityRequest : FinanceServiceArrangementRequest
    {
        public Currency MinimalRepaymentAmount { get; set; }
        [Column("MinRepPerc")]
        public decimal MinimalRepaymentPercentage { get; set; }
        [Column("RevPerc")]
        public decimal RevolvingPercentage { get; set; }

        public override bool IsFinanceService()
        {
            return true;
        }
    }
}
