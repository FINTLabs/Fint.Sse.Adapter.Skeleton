using Fint.Event.Model;

namespace Fint.Sse.Adapter.Services
{
    public interface IHttpService
    {
        void Post(string endpoint, Event<object> serverSideEvent);
    }
}