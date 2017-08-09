using Fint.SSE.Adapter.sse;
using FintEventModel.Model;
using Serilog;
using System;
using System.Collections.Specialized;
using System.Net;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace Fint.SSE.Adapter.service
{
    public class HttpService : IHttpService
    {
        public void Post(string endpoint, Event<object> evt)
        {
            using (WebClient client = new WebClient())
            {
                JsonConvert.DefaultSettings = (() =>
                {
                    var settings = new JsonSerializerSettings();
                    settings.Converters.Add(new StringEnumConverter { CamelCaseText = false });
                    settings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                    return settings;
                });

                client.Headers.Add(FintHeaders.ORG_ID_HEADER, evt.OrgId);
                client.Headers.Add(HttpRequestHeader.ContentType, "application/json");
                //var keysValues = ConvertToKeyValuePair(evt);
                var json = JsonConvert.SerializeObject(evt);

                // Before having tested this with a proper backend system, 
                // I believe this is the method that would work best.
                try
                {
                    Log.Information("JSON event: {json}", json);
                    var response = client.UploadString(endpoint, json);
                    Log.Information("Provider POST response {reponse}", response);
                }
                catch (Exception e)
                {
                    Log.Warning("Could not POST {event} to {endpoint}. Error: {error}", evt, endpoint, e.Message);
                }
            }
        }

        private static NameValueCollection ConvertToKeyValuePair(object objectItem)
        {
            Type type = objectItem.GetType();
            PropertyInfo[] propertyInfos = type.GetProperties();
            NameValueCollection propNames = new NameValueCollection();

            foreach (PropertyInfo propertyInfo in objectItem.GetType().GetProperties())
            {
                if (propertyInfo.CanRead)
                {
                    var pName = propertyInfo.Name;
                    var pValue = propertyInfo.GetValue(objectItem, null);
                    if (pValue != null)
                    {
                        propNames.Add(pName, pValue.ToString());
                    }
                }
            }

            return propNames;
        }
    }
}
