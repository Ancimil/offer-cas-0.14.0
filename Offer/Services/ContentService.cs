using MicroserviceCommon.ApiUtil;
using MicroserviceCommon.Authentication;
using Newtonsoft.Json;
using Offer.Domain.AggregatesModel.ApplicationAggregate;
using Offer.Domain.AggregatesModel.ContentModel;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Web;
using Offer.API.Extensions;

namespace Offer.API.Services
{
    public class ContentServiceOld : IContentServiceOld
    {
        private readonly ApiEndPoints _apiEndPoints;
        private readonly TokenAuthentication tokenAuthentication;
        private readonly ILogger<ContentServiceOld> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public ContentServiceOld(
            ApiEndPoints apiEndPoints,
            TokenAuthentication tokenAuthentication,
            ILogger<ContentServiceOld> logger,
            IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
            this._apiEndPoints = apiEndPoints ?? throw new ArgumentNullException(nameof(apiEndPoints));
            this.tokenAuthentication = tokenAuthentication ?? throw new ArgumentNullException(nameof(tokenAuthentication));
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Metadata> CreateFolder(string name, string path, string kind, string purpose)
        {
            using (var client = GetHttpClient())
            {
                var payloadObject = new Dictionary<string, string>
            {
                { "name", name },
                { "path", path },
                { "kind", kind },
                { "folder-purpose", purpose }
            };
                _logger.LogInformation("Creating content folder {name} at '{path}'. Kind is {kind}, and purpose: {purpose}", name, path, kind, purpose);
                var dataAsString = JsonConvert.SerializeObject(payloadObject);
                _logger.LogDebug("Payload: {Paylaod}", dataAsString);
                var requestContent = new StringContent(dataAsString, Encoding.UTF8, "application/json");
                requestContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                var url = _apiEndPoints.GetServiceUrl("content") + "reponame/folders?x-asee-auth=true";
                using (HttpResponseMessage response = await client.PostAsync(url, requestContent))
                {
                    var res = await response.Content.ReadAsStringAsync();
                    _logger.LogDebug("Response code for creating content folder {name} is {ResponseCode}", name, response.StatusCode);
                    if (response.StatusCode.Equals(HttpStatusCode.OK) || response.StatusCode.Equals(HttpStatusCode.Created))
                    {
                        var metadata = (Metadata)CaseUtil.ConvertFromJsonToObject(res, typeof(Metadata));
                        return metadata;
                    }
                    else if (response.StatusCode.Equals((HttpStatusCode)440))
                    {
                        var errorResponse = (ErrorResponse)CaseUtil.ConvertFromJsonToObject(res, typeof(ErrorResponse));
                        _logger.LogError("Folder {name} is not created because it already exists. Content service error: {err}", name, errorResponse?.Message);
                        return null; // throw new DuplicateObjectException(errorResponse?.Message);
                    }
                    else
                    {
                        var errorResponse = (ErrorResponse)CaseUtil.ConvertFromJsonToObject(res, typeof(ErrorResponse));
                        _logger.LogError("Error while creating {name} folder. Response code is {resCode}. Content service error: {err}.",
                            name, response.StatusCode, errorResponse.Message);
                        return null; // throw new Exception("Response code from Content service is 400+ and not 440");
                    }
                }
            }
        }

        public async Task<bool> DeleteFolderByPath(string folderPath, bool deleteRecursive = true)
        {
            _logger.LogInformation("Deleting content folder on '{path}'.", folderPath);
            var metadata = GetFolderMetadata(folderPath).Result;
            using (var client = GetHttpClient())
            {
                var builder = new UriBuilder(_apiEndPoints.GetServiceUrl("content") + "reponame/folders/" + metadata?.Id + "?x-asee-auth=true");
                var query = HttpUtility.ParseQueryString(string.Empty);
                query["delete-content-and-subfolders"] = "" + deleteRecursive;
                builder.Query = query.ToString();
                try
                {
                    using (HttpResponseMessage response = await client.DeleteAsync(builder.ToString()))
                    {

                        if (response.StatusCode.Equals((HttpStatusCode)404))
                        {
                            _logger.LogError("Folder on {path} not found.", folderPath);
                            return false; // throw new KeyNotFoundException("Folder not found");
                        }
                        var res = await response.Content.ReadAsStringAsync();
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "An error occurred during deleting folder from path");
                    return false;
                }
                return true;
            }
        }

        public async Task<Metadata> GetFolderMetadata(string folderPath)
        {
            _logger.LogInformation("Getting metadata for content folder on '{path}'.", folderPath);
            if (string.IsNullOrEmpty(folderPath))
            {
                _logger.LogError("Folder path is null while trying to get folder metadata");
                throw new NullReferenceException("Folder path is null while trying to get folder metadata");
            }
            folderPath = folderPath.Trim('/');
            using (var client = GetHttpClient())
            {
                var builder = new UriBuilder(_apiEndPoints.GetServiceUrl("content") + "reponame/" + folderPath + "/metadata");
                var url = builder.ToString();
                _logger.LogInformation("Metadata url: {url}", url);
                using (HttpResponseMessage response = await client.GetAsync(url))
                {
                    if (response.StatusCode.Equals((HttpStatusCode)404))
                    {
                        _logger.LogError("Folder on {path} not found.", folderPath);
                        throw new KeyNotFoundException("Folder not found");
                    }
                    var res = await response.Content.ReadAsStringAsync();
                    return (Metadata)CaseUtil.ConvertFromJsonToObject(res, typeof(Metadata));
                }
            }
        }

        private HttpClient GetHttpClient()
        {
            var client = _httpClientFactory.CreateClient();
            client.AddDefaultJsonHeaders();
            return client;
        }
    }
}
