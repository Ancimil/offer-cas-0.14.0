using AutoMapper;
using Offer.Domain.AggregatesModel.ApplicationAggregate;
using Offer.Infrastructure.View;

namespace Offer.Infrastructure.Mappings
{
    public static class ApplicationDocumentMappingExtensions
    {
        public static ApplicationDocumentView GetDocumentView(this ApplicationDocument document)
        {
            var mapped = Mapper.Map<ApplicationDocumentView>(document);
            string contextInfo = null;
            if (document.ArrangementRequest != null)
            {
                contextInfo = document.ArrangementRequest.ProductName;
            }
            if (document.Party != null)
            {
                contextInfo = (contextInfo != null) ? contextInfo + " - " + document.Party.CustomerName : document.Party.CustomerName;
            }
            mapped.DocumentContextInfo = contextInfo;
            return mapped;
        }
    }
}
