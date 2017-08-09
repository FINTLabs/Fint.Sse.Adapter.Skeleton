﻿using FintEventModel.Model;

namespace Fint.SSE.Adapter
{
    public interface IEventStatusService
    {
        Event<object> VerifyEvent(Event<object> evt);
    }
}