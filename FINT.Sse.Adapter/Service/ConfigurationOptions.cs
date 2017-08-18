using System.Collections.Generic;

namespace Fint.SSE.Adapter.Service
{
    public class ConfigurationOptions
    {
        public string SseEndpoint { get; set; }
        public string StatusEndpoint { get; set; }
        public string ResponseEndpoint { get; set; }
        public string Organizations { get; set; }
        //public IEnumerable<string> Organizations
        //{
        //    get
        //    {
        //        //var orgs = GetFromConfig("fint.provider.adapter.organizations");
        //        return OrganizationsString.Split(',');
        //    }
        //}

        public string LogLocation { get; set; }

        //private string GetFromConfig(string name)
        //{
        //    var setting = ConfigurationManager.AppSettings[name];
        //    if (string.IsNullOrEmpty(setting))
        //    {
        //        throw new KeyNotFoundException($"Could not find {name} in app.config");
        //    }
        //    return setting;
        //}
    }
}
