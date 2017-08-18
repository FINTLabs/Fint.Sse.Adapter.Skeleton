using System;
using System.Net.Http;
using System.Net.Http.Headers;
using Fint.SSE.Adapter.SSE;
using Fint.Event.Model;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace Fint.SSE.Adapter.Service
{
    public class HttpService : IHttpService
    {
        private readonly ILogger<HttpService> _logger;

        public HttpService(ILogger<HttpService> logger)
        {
            _logger = logger;
        }

        public async void Post(string endpoint, Event<object> serverSideEvent)
        {
            using (HttpClient client = new HttpClient())
            {
                //TODO: Move this to a formatter in Startup.cs
                JsonConvert.DefaultSettings = (() =>
                {
                    var settings = new JsonSerializerSettings();
                    settings.Converters.Add(new StringEnumConverter { CamelCaseText = false });
                    settings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                    return settings;
                });

                var contentType = new MediaTypeWithQualityHeaderValue("application/json");
                client.DefaultRequestHeaders.Accept.Add(contentType);

                var json = JsonConvert.SerializeObject(serverSideEvent);
                StringContent content = new StringContent(json);

                content.Headers.Add(FintHeaders.ORG_ID_HEADER, serverSideEvent.OrgId);
                content.Headers.ContentType = contentType;

                try
                {
                    _logger.LogInformation("JSON endpoint: {endpoint}", endpoint);
                    _logger.LogInformation("JSON event: {json}", json);
                    var response = await client.PostAsync(endpoint, content);
                    _logger.LogInformation("Provider POST response {reponse}", response.Content.ReadAsStringAsync().Result);
                }
                catch (Exception e)
                {
                    _logger.LogWarning("Could not POST {event} to {endpoint}. Error: {error}", serverSideEvent, endpoint, e.Message);
                }
            }
            
        }
    }
}
