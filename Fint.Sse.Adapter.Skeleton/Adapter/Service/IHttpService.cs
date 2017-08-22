﻿using Fint.Event.Model;

namespace Fint.Sse.Adapter.Service
{
    public interface IHttpService
    {
        void Post(string endpoint, Event<object> serverSideEvent);
    }
}