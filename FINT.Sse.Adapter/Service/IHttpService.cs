﻿using Fint.Event.Model;

namespace Fint.SSE.Adapter.Service
{
    public interface IHttpService
    {
        void Post(string endpoint, Event<object> serverSideEvent);
    }
}