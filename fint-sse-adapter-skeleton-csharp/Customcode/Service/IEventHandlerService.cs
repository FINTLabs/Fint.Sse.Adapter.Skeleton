using FintEventModel.Model;

namespace Fint.SSE.Customcode.Service
{
    public interface IEventHandlerService
    {
        void HandleEvent(Event evtObj);
    }
}