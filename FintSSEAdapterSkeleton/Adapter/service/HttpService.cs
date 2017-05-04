using Fint.SSE.Adapter.sse;
using FintEventModel.Model;
using Serilog;
using System;
using System.Collections.Specialized;
using System.Net;
using System.Reflection;
using System.Text;

namespace Fint.SSE.Adapter.service
{
    public class HttpService : IHttpService
    {
        public void Post(string endpoint, Event evt)
        {
            using (WebClient client = new WebClient())
            {
                client.Headers.Add(FintHeaders.ORG_ID_HEADER, evt.OrgId);
                var keysValues = ConvertToKeyValuePair(evt);
                // Before having tested this with a proper backend system, 
                // I believe this is the method that would work best.
                try
                {
                    var response = client.UploadValues(endpoint, keysValues);
                    Log.Information("Provider POST response {reponse}", Encoding.ASCII.GetString(response));
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
