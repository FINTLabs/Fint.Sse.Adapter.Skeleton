using System.Collections.Generic;

namespace Fint.SSE.Adapter.Service
{
    public interface IConfigService
    {
        string LogLocation { get; }
        IEnumerable<string> Organizations { get; }
        string ResponseEndpoint { get; }
        string SseEndpoint { get; }
        string StatusEndpoint { get; }
    }
}