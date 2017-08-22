using Fint.Event.Model;

namespace Fint.Sse.Adapter.Services
{
    public interface IPwfaService
    {
        void GetAllDogs(Event<object> serverSideEvent);
        void GetDog(Event<object> serverSideEvent);
        void GetAllOwners(Event<object> serverSideEvent);
        void GetOwner(Event<object> serverSideEvent);
    }
}