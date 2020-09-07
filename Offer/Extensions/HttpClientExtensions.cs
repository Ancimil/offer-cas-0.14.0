using System.Net.Http;
using System.Net.Http.Headers;

namespace Offer.API.Extensions
{
    public static class HttpClientExtensions
    {
        public static void AddDefaultJsonHeaders(this HttpClient httpClient)
        {
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
        }
    }
}
