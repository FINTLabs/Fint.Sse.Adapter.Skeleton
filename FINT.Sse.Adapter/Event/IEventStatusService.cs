using Fint.Event.Model;

namespace Fint.SSE.Adapter.Event
{
    public interface IEventStatusService
    {
        Event<object> VerifyEvent(Event<object> serverSideEvent);
    }
}