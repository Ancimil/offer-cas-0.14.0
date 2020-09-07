using MediatR;
using OfferApplication = Offer.Domain.AggregatesModel.ApplicationAggregate.Application;

namespace Offer.API.Application.Commands
{
    public class UpdateFinancialStatementsCommand : IRequest<bool>
    {
        public OfferApplication Application { get; set; }

        public UpdateFinancialStatementsCommand(OfferApplication Application)
        {
            this.Application = Application;
        }
    }
}
