using System.Collections.Generic;

namespace Offer.Domain.AggregatesModel.ApplicationAggregate
{
    public class GdprData
    {
        public List<GdprParty> ErasableDataFullMatch { get; set; }
        public List<GdprParty> NonErasableDataFullMatch { get; set; }
        public List<GdprParty> ErasableDataPartialMatch { get; set; }
        public List<GdprParty> NonErasableDataPartialMatch { get; set; }
        public List<GdprParty> DataAfterAnonymization { get; set; }
    }
}
