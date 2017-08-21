using EventSource4Net;

namespace Fint.SSE.Adapter.EventListeners
{
    public interface IFintEventListener
    {
        EventSource Listen(string orgId);
    }
}