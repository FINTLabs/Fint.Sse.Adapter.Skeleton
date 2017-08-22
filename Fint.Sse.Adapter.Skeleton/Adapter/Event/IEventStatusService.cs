using Fint.Event.Model;

namespace Fint.Sse.Adapter.Event
{
    public interface IEventStatusService
    {
        Event<object> VerifyEvent(Event<object> serverSideEvent);
    }
}