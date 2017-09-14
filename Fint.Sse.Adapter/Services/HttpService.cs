using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Fint.Event.Model;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Fint.Sse.Adapter.Services
{
    public class HttpService : IHttpService
    {
        private readonly ILogger<HttpService> _logger;
        private readonly HttpClient _httpClient;
        private readonly ITokenService _tokenService;

        public HttpService(ILogger<HttpService> logger, HttpClient httpClient, ITokenService tokenService)
        {
            _logger = logger;
            _httpClient = httpClient;
            _tokenService = tokenService;
        }

        public async void Post(string endpoint, Event<object> serverSideEvent)
        {
            var accessToken = "";

            if (_tokenService.UseAuthentication)
            {
                var task = Task.Run(async () => {
                    return await _tokenService.GetAccessTokenAsync();
                });

                accessToken = task.Result;
            }

            var contentType = new MediaTypeWithQualityHeaderValue("application/json");
            _httpClient.DefaultRequestHeaders.Accept.Add(contentType);

            if (_tokenService.UseAuthentication)
            {
                _httpClient.SetBearerToken(accessToken);
            }
            
            var json = JsonConvert.SerializeObject(serverSideEvent);
            StringContent content = new StringContent(json);

            content.Headers.Add(FintHeaders.ORG_ID_HEADER, serverSideEvent.OrgId);
            content.Headers.ContentType = contentType;

            try
            {
                LoggerExtensions.LogInformation(_logger, "JSON endpoint: {endpoint}", endpoint);
                LoggerExtensions.LogInformation(_logger, "JSON event: {json}", json);
                var response = await _httpClient.PostAsync(endpoint, content);
                LoggerExtensions.LogInformation(_logger, "Provider POST response {reponse}", response.Content.ReadAsStringAsync().Result);
            }
            catch (Exception e)
            {
                LoggerExtensions.LogWarning(_logger, "Could not POST {event} to {endpoint}. Error: {error}", serverSideEvent, endpoint, e.Message);
            }
            
            
        }        
    }
}
