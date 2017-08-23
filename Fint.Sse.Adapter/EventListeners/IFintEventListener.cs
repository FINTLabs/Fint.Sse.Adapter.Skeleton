namespace Fint.Sse.Adapter.EventListeners
{
    public interface IFintEventListener
    {
        EventSource Listen(string orgId);
    }
}