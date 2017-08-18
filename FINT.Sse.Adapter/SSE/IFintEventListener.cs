using EventSource4Net;

namespace Fint.SSE.Adapter.SSE
{
    public interface IFintEventListener
    {
        EventSource Listen(string orgId);
    }
}