using System.Collections.Generic;

namespace Fint.SSE.Adapter.Service
{
    public interface IConfigurationOptions
    {
        string LogLocation { get; }
        string OrganizationsString { get; }
        IEnumerable<string> Organizations { get; }
        string ResponseEndpoint { get; }
        string SseEndpoint { get; }
        string StatusEndpoint { get; }
    }
}