using Fint.Event.Model;

namespace Fint.Sse.Adapter.Services
{
    public class FintEventHandler : IEventHandler
    {
        private readonly IEventHandlerService _eventHandlerService;

        public FintEventHandler(IEventHandlerService eventHandlerService)
        {
            _eventHandlerService = eventHandlerService;
        }

        public void HandleEvent(Event<object> serverSideEvent)
        {
            _eventHandlerService.HandleEvent(serverSideEvent);
        }
    }
}