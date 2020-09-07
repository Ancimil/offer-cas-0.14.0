using System;
using System.Collections.Generic;

namespace Offer.Domain.AggregatesModel.ApplicationAggregate
{
    public class PagedApplicationList
    {
        public List<Application> Applications { get; set; }
        public int TotalCount { get; set; }
        public int PageSize { get; set; }
        public int Page { get; set; }
        public int TotalPages { get; set; }
        public string SortOrder { get; set; }
        public string SortBy { get; set; }

        public void ForEach(Func<object, object> p)
        {
            throw new NotImplementedException();
        }
    }

    public class ApplicationList
    {
        public List<ApplicationView> Applications { get; set; }
        public int TotalCount { get; set; }
        public int PageSize { get; set; }
        public int Page { get; set; }
        public int TotalPages { get; set; }
        public string SortOrder { get; set; }
        public string SortBy { get; set; }
    }

    public class ApplicationView
    {
        public string ApplicationNumber { get; set; }
        public ArrangementKind? Kind { get; set; }
        public ApplicationStatus Status { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public string CustomerNumber { get; set; }
        public string CustomerName { get; set; }
        public string OrganizationUnitCode { get; set; }
        public string ChannelCode { get; set; }
        public string PortfolioId { get; set; }
        public string CampaignCode { get; set; }
        public long? LeadId { get; set; }
        public string SigningOption { get; set; }
        public DateTime? RequestDate { get; set; }
        public DateTime? Created { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public DateTime? StatusChangeDate { get; set; }
        public DateTime? LastModified { get; set; }
        public string CreatedByName { get; set; }
        public string Term { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public bool? OriginatesBundle { get; set; } = false;
        public string Phase { get; set; }
    }
}
