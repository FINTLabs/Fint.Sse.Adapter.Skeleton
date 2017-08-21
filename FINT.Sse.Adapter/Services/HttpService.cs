using System;
using System.Net.Http;
using System.Net.Http.Headers;
using Fint.Event.Model;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Fint.Sse.Adapter.Models;

namespace Fint.Sse.Adapter.Services
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
                ////DONE: Moved this to a formatter in Program.cs
                //JsonConvert.DefaultSettings = (() =>
                //{
                //    var settings = new JsonSerializerSettings();
                //    settings.Converters.Add(new StringEnumConverter { CamelCaseText = false });
                //    settings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                //    return settings;
                //});

                var contentType = new MediaTypeWithQualityHeaderValue("application/json");
                client.DefaultRequestHeaders.Accept.Add(contentType);

                var json = JsonConvert.SerializeObject(serverSideEvent);
                StringContent content = new StringContent(json);

                content.Headers.Add(FintHeaders.ORG_ID_HEADER, serverSideEvent.OrgId);
                content.Headers.ContentType = contentType;

                try
                {
                    LoggerExtensions.LogInformation(_logger, "JSON endpoint: {endpoint}", endpoint);
                    LoggerExtensions.LogInformation(_logger, "JSON event: {json}", json);
                    var response = await client.PostAsync(endpoint, content);
                    LoggerExtensions.LogInformation(_logger, "Provider POST response {reponse}", response.Content.ReadAsStringAsync().Result);
                }
                catch (Exception e)
                {
                    LoggerExtensions.LogWarning(_logger, "Could not POST {event} to {endpoint}. Error: {error}", serverSideEvent, endpoint, e.Message);
                }
            }
            
        }
    }
}
