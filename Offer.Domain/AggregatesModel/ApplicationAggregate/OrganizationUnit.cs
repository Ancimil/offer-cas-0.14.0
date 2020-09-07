using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Offer.Domain.AggregatesModel.ApplicationAggregate
{
    public class OrganizationUnit
    {
        public string Code { get; set; }
        public string ParentCode { get; set; }
        public string Name { get; set; }
        public string NavigationCode { get; set; }

        public DateTime SyncedOn { get; set; }
        [JsonIgnore]
        public List<Application> Applications { get; set; }
    }
}
