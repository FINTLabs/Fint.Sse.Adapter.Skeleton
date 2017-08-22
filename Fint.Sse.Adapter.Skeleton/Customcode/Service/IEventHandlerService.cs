using Fint.Event.Model;

namespace Fint.Sse.Customcode.Service
{
    public interface IEventHandlerService
    {
        void HandleEvent(Event<object> serverSideEvent);
    }
}