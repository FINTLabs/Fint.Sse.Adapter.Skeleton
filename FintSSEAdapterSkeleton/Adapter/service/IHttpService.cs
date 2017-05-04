using FintEventModel.Model;

namespace Fint.SSE.Adapter.service
{
    public interface IHttpService
    {
        void Post(string endpoint, Event evt);
    }
}