using MicroserviceCommon.ApiUtil;
using Offer.Domain.AggregatesModel.ApplicationAggregate;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Offer.API.Extensions;

namespace Offer.API.Services
{
    public class MasterPartyDataService : IMasterPartyDataService
    {
        private readonly ApiEndPoints _apiEndPoints;
        private readonly ILogger<ContentServiceOld> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public MasterPartyDataService(
            ApiEndPoints apiEndPoints,
            ILogger<ContentServiceOld> logger,
            IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
            this._apiEndPoints = apiEndPoints ?? throw new ArgumentNullException(nameof(apiEndPoints));
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }


        public async Task<Party> GetPartyData(Party party)
        {
            if (string.IsNullOrEmpty(party.CustomerNumber))
            {
                _logger.LogError("Could not fetch party data without customer number.");
                throw new Exception("No customer number!");
            }

            using (var httpClient = _httpClientFactory.CreateClient())
            {
                httpClient.AddDefaultJsonHeaders();
                try
                {
                    using (HttpResponseMessage response = await httpClient.GetAsync(_apiEndPoints.GetServiceUrl("party") + "parties/" + party.CustomerNumber + "?x-asee-auth=true"))
                    {
                        var res = await response.Content.ReadAsStringAsync();
                        if (res.Equals(""))
                        {
                            _logger.LogError("Could not fetch party data for party number: {partyNumber}. Response is empty", party.CustomerNumber);
                            throw new Exception("Could not fetch Party");
                        }
                        JObject json = JObject.Parse(res);
                        string partyKind = json["kind"].ToString();
                        party.OrganizationUnitCode = json["origination-info"] != null && json["origination-info"]["org-unit"] != null ? json["origination-info"]["org-unit"].ToString() : null;

                        var partyKindString = party is IndividualParty ? "individual" : "organization";
                        if (partyKind != partyKindString)
                        {
                            throw new InvalidCastException("Party kind from relationship (" + partyKindString + ") is different that " +
                                "original party kind (" + partyKind + ")");
                        }

                        if (partyKind.Equals("individual") && party is IndividualParty)
                        {
                            var partyIndividual = (IndividualParty)party;
                            AutoMapper.Mapper.Map(json, partyIndividual, typeof(JObject), typeof(IndividualParty));
                            if (string.IsNullOrEmpty(party.CustomerName))
                            {
                                if (!string.IsNullOrEmpty(partyIndividual.GivenName) || !string.IsNullOrEmpty(partyIndividual.Surname))
                                    party.CustomerName = partyIndividual.GivenName + " " + partyIndividual.Surname;
                            }
                            return party;
                        }
                        else if (partyKind.Equals("organization"))
                        {
                            // TODO: Fix corporate
                            var partyOrg = (OrganizationParty)party;
                            AutoMapper.Mapper.Map(json, partyOrg, typeof(JObject), typeof(OrganizationParty));
                            //OrganizationParty partyOrg = (OrganizationParty)CaseUtil.ConvertFromJsonToObject(res, typeof(OrganizationParty));
                            return party;
                        }
                        else
                        {
                            _logger.LogError("Unknown Party type ({partyKind}) while fetching party data for party number: {partyNumber}", partyKind, party.CustomerNumber);
                            throw new Exception("Unknown Party type: " + partyKind);
                        }
                    }
                }
                catch (HttpRequestException e)
                {
                    _logger.LogError(e, "Could not fetch party data for party number: {partyNumber}", party.CustomerNumber);
                    throw e;
                }
            }

        }
    }
}