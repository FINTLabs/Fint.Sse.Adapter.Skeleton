using Fint.Event.Model;

namespace Fint.Sse.Adapter.Services
{
    public interface IEventStatusService
    {
        Event<object> VerifyEvent(Event<object> serverSideEvent);
    }
}