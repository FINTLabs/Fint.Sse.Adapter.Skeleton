using Fint.Event.Model;

namespace Fint.SSE.Customcode.Service
{
    public interface IEventHandlerService
    {
        void HandleEvent(Event<object> evtObj);
    }
}