
using MicroserviceCommon.ApiUtil;
using MicroserviceCommon.Authentication;
using Newtonsoft.Json.Linq;
using Offer.API.Extensions;
using Offer.Domain.AggregatesModel.ApplicationAggregate;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Offer.API.Services
{
    public class FinancialStatementsService : IFinancialStatementsService
    {
        private readonly ApiEndPoints _apiEndPoints;
        private readonly TokenAuthentication tokenAuthentication;
      //  private readonly ILogger<ContentServiceOld> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public FinancialStatementsService(
            ApiEndPoints apiEndPoints,
            TokenAuthentication tokenAuthentication,
       //     ILogger<ContentServiceOld> logger,
            IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
            this._apiEndPoints = apiEndPoints ?? throw new ArgumentNullException(nameof(apiEndPoints));
            this.tokenAuthentication = tokenAuthentication ?? throw new ArgumentNullException(nameof(tokenAuthentication));
         //   this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<FinancialStatementsData> GetFinancialStatementAsync(string customerNumber, string reportType, string accountingMethod, int year)
        {
            if (string.IsNullOrEmpty(customerNumber))
            {
              //  _logger.LogError("Could not fetch party data without customer number.");
                throw new Exception("No customer number!");
            }

            using (var httpClient = _httpClientFactory.CreateClient())
            {
                httpClient.AddDefaultJsonHeaders();
                try
                {
                    using (HttpResponseMessage response = await httpClient.GetAsync(_apiEndPoints.GetServiceUrl("financial-statements") + "reports/customer/" + customerNumber + "?year="+ year + "&report-type="+reportType + "&accounting-method="+ accountingMethod))
                    {
                        FinancialStatementsData fsd = new FinancialStatementsData();
                        var res = await response.Content.ReadAsStringAsync();
                        if (res.Equals("[]"))
                        {
                            fsd.ReportId = null;
                        }
                        else
                        {
                            JArray list = JArray.Parse(res);
                            

                            //   AutoMapper.Mapper.Map(json, fsd, typeof(JObject), typeof(List<FinancialStatementsData>));




                            foreach (var item in list)
                            {

                                fsd.ReportId = Convert.ToInt64(item["report-header"]["report-id"]);
                                fsd.ReportType = item["report-header"]["report-type"].ToString();
                                fsd.ReportDate = Convert.ToDateTime(item["report-header"]["report-date"].ToString());

                            }

                        }




                        return fsd;
                    }
                
                       
                    //    else
                    //    {
                    //    _logger.LogError("Unknown Party type ({partyKind}) while fetching party data for party number: {partyNumber}", partyKind, party.CustomerNumber);
                    //    throw new Exception("Unknown Party type: " + partyKind);
                    //}

                }
                catch (HttpRequestException e)
                {
                    //_logger.LogError(e, "Could not fetch party data for party number: {partyNumber}", customerNumber);
                    throw e;
                }
            }

        }


    }
}
