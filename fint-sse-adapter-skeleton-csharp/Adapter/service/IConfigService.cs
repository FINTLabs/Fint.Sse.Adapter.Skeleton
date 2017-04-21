using System.Collections.Generic;

namespace Fint.SSE.Adapter.service
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