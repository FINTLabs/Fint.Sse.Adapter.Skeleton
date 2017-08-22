using System.Collections.Generic;
using System.Configuration;

namespace Fint.Sse.Adapter.Service
{
    public class ConfigService : IConfigService
    {
        public string SseEndpoint
        {
            get
            {
                return GetFromConfig("fint.provider.adapter.sse-endpoint");
            }
        }
        public string StatusEndpoint {
            get
            {
                return GetFromConfig("fint.provider.adapter.status-endpoint");
            }
        }
        public string ResponseEndpoint {
            get
            {
                return GetFromConfig("fint.provider.adapter.response-endpoint");
            }
        }
        public IEnumerable<string> Organizations
        {
            get
            {
                var orgs = GetFromConfig("fint.provider.adapter.organizations");
                return orgs.Split(',');
            }
        }

        public string LogLocation {
            get
            {
                return GetFromConfig("fint.provider.logLocation");
            }
        }

        private string GetFromConfig(string name)
        {
            var setting = ConfigurationManager.AppSettings[name];
            if (string.IsNullOrEmpty(setting))
            {
                throw new KeyNotFoundException($"Could not find {name} in app.config");
            }
            return setting;
        }
    }
}
