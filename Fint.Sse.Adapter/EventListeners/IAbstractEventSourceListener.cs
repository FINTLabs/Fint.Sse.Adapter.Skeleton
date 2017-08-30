using System;
using System.Collections.Generic;

namespace Fint.Sse.Adapter.EventListeners
{
    public interface IAbstractEventSourceListener
    {
        void HandleEvent(string data);
        EventSource Listen(Uri url, Dictionary<string, string> headers, int timeout);
    }
}