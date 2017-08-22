using System;
using System.Net.Http;
using System.Net.Http.Headers;
using Fint.Event.Model;
using Fint.Sse.Adapter.SSE;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Serilog;

namespace Fint.Sse.Adapter.Service
{
    public class HttpService : IHttpService
    {
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
                    Log.Information("JSON endpoint: {endpoint}", endpoint);
                    Log.Information("JSON event: {json}", json);
                    var response = await client.PostAsync(endpoint, content);
                    Log.Information("Provider POST response {reponse}", response.Content.ReadAsStringAsync().Result);
                }
                catch (Exception e)
                {
                    Log.Warning("Could not POST {event} to {endpoint}. Error: {error}", serverSideEvent, endpoint, e.Message);
                }
            }
            
        }
    }
}
