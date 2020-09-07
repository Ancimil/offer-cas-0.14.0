using Asseco.EventBus.Abstractions;
using Asseco.EventBus.Events;
using MicroserviceCommon.ApiUtil;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Offer.Domain.AggregatesModel.ApplicationAggregate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Offer.Infrastructure.Services
{
    public class OrganizationUnitSync : BackgroundService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IApiEndPoints _apiEndPoints;
        private readonly IServiceProvider _serviceProvider;
        private readonly IEventBus _eventBus;
        public OrganizationUnitSync(
            IHttpClientFactory httpClientFactory,
            IApiEndPoints apiEndPoints,
          IServiceProvider serviceProvider,
            IEventBus eventBus
        )
        {
            _httpClientFactory = httpClientFactory;
            _eventBus = eventBus;
            _serviceProvider = serviceProvider;
            _apiEndPoints = apiEndPoints;
        }
        private long ConvertToTimestamp(DateTime value)
        {
            long epoch = (value.Ticks - 621355968000000000) / 10000000;
            return epoch;
        }
        public async Task Syncronize(OfferDBContext _offerDbContext)
        {
            using (var client = _httpClientFactory.CreateClient())
            {
                DateTime lastSyncDate = _offerDbContext.OrganizationUnits.Select(x => x.SyncedOn)
                    .DefaultIfEmpty(DateTime.MinValue).Max();

                DateTime syncDate = DateTime.UtcNow;
                long syncTimestampStart = 1;
                if (lastSyncDate != DateTime.MinValue)
                {
                    syncTimestampStart = ConvertToTimestamp(lastSyncDate);
                }

                var identityUrl = string.Concat(_apiEndPoints.GetServiceUrl("identity"), "organization-units/sync?sync-timestamp="
                        + syncTimestampStart + "&x-asee-auth=true");

                using (var result = await client.GetAsync(identityUrl))
                {
                    if (result.IsSuccessStatusCode)
                    {
                        var contentData = await result.Content.ReadAsStringAsync();
                        var organizationUnitResults = (List<OrganizationUnit>)CaseUtil.ConvertFromJsonToObject(contentData, typeof(List<OrganizationUnit>));
                        await ProcessOrganizationUnits(organizationUnitResults, _offerDbContext, syncDate);
                    }
                }
            }
        }

        public async Task ProcessOrganizationUnits(List<OrganizationUnit> organizationUnits, OfferDBContext _offerDbContext, DateTime syncDate)
        {
            foreach (OrganizationUnit organizationUnit in organizationUnits)
            {
                var ouExists = _offerDbContext.OrganizationUnits.Any(x => x.Code == organizationUnit.Code);
                if (!ouExists)
                {
                    OrganizationUnit parentOu = null;
                    if (organizationUnit.ParentCode != null)
                    {
                        parentOu = _offerDbContext.OrganizationUnits.Where(x => x.Code == organizationUnit.ParentCode).FirstOrDefault();
                    }
                    var createOrganizationUnit = new OrganizationUnit
                    {
                        Code = organizationUnit.Code,
                        Name = organizationUnit.Name,
                        NavigationCode = parentOu != null ? string.Concat(parentOu.NavigationCode, "/", organizationUnit.Code) : organizationUnit.Code,
                        ParentCode = organizationUnit.ParentCode,
                        SyncedOn = syncDate
                    };
                    _offerDbContext.Add(createOrganizationUnit);
                }
                else
                {
                    var existingOu = _offerDbContext.OrganizationUnits.Where(x => x.Code == organizationUnit.Code).FirstOrDefault();
                    // Handle OU name
                    if (!organizationUnit.Name.Equals(existingOu.Name))
                    {
                        existingOu.Name = organizationUnit.Name;
                    }
                    existingOu.SyncedOn = syncDate;
                    // Handle parent code change
                    if (organizationUnit.ParentCode == null && existingOu.ParentCode != null)
                    {
                        existingOu.ParentCode = null;
                        existingOu.NavigationCode = existingOu.Code;
                    }
                    else if (organizationUnit.ParentCode != null &&
                            !organizationUnit.ParentCode.Equals(existingOu.ParentCode))
                    {
                        var parentOu = _offerDbContext.OrganizationUnits.Where(x => x.Code == organizationUnit.ParentCode).FirstOrDefault();
                        existingOu.ParentCode = organizationUnit.ParentCode;
                        existingOu.NavigationCode = string.Concat(parentOu.NavigationCode, "/", existingOu.Code);
                    }

                }
                await _offerDbContext.SaveChangesAsync();
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var dbContext = _serviceProvider.GetService<OfferDBContext>();
                await Syncronize(dbContext);
            }
            _eventBus.Subscribe("identity", "offer_organization_unit_sync",
                new OrganizationUnitSyncMessageEventListener(this, _serviceProvider)
            );
        }
    }

    public class OrganizationUnitSyncMessageEventListener : IIntegrationEventHandler<MessageEvent>
    {
        private readonly OrganizationUnitSync _organizationUnitSync;
        private readonly IServiceProvider _serviceProvider;

        public OrganizationUnitSyncMessageEventListener(
            OrganizationUnitSync organizationUnitSync,
            IServiceProvider serviceProvider
            )
        {
            _serviceProvider = serviceProvider;
            _organizationUnitSync = organizationUnitSync;
        }
        public Task Handle(MessageEvent messageEvent)
        {
            var messageName = messageEvent.getStringProperty("messageName");
            if (messageName != null
                && (messageName.Equals("organization-units-synced")
                || messageName.Equals("organization-unit-changed")
                || messageName.Equals("organization-unit-removed")
                || messageName.Equals("organization-unit-added")))
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var dbContext = _serviceProvider.GetService<OfferDBContext>();
                    _organizationUnitSync.Syncronize(dbContext).Wait();
                }
            }
            return Task.CompletedTask;
        }
    }
}
