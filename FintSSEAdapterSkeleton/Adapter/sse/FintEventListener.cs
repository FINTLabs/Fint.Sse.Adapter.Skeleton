using System;
using System.Collections.Generic;
using System.Threading;
using EventSource4Net;
using Fint.SSE.Adapter.Service;
using Fint.SSE.Customcode.Service;
using Fint.Event.Model;
using Serilog;

namespace Fint.SSE.Adapter.SSE
{
    public class FintEventListener
    {
        private EventHandlerService _eventHandlerService;
        private ConfigService _configService;
        private string _orgId;

        public FintEventListener(EventHandlerService handlerService, ConfigService configService)
        {
            _eventHandlerService = handlerService;
            _configService = configService;
        }

        public FintEventListener()
        {
            _configService = new ConfigService();
            _eventHandlerService = new EventHandlerService();
        }

        private void onEvent(string data)
        {
            var evtObj = EventUtil.ToEvent<object>(data);

            if (evtObj == null)
            {
                Log.Error("Could not parse Event object");
            }
            else
            {
                if (evtObj.OrgId == _orgId)
                {
                    Log.Debug("{orgId}: Event received {@Event}", _orgId, data);
                    _eventHandlerService.HandleEvent(evtObj);
                }
                else
                {
                    //Log.Debug("This is not EventListener for {org}", _orgId);
                }
            }
        }
        public EventSource Listen(string orgId)
        {
            _orgId = orgId;
            CancellationTokenSource cts = new CancellationTokenSource();
            var headers = new Dictionary<string, string>
            {
                { FintHeaders.ORG_ID_HEADER, orgId }
            };
            var url = new Uri(string.Format("{0}/{1}", _configService.SseEndpoint, Guid.NewGuid().ToString()));
            EventSource es = new EventSource(url, headers, 10000);
            es.StateChanged += new EventHandler<StateChangedEventArgs>((o, e) =>
            {
                Log.Debug("{orgId}: SSE state change {@state}", _orgId, e.State);
            });
            es.EventReceived += new EventHandler<ServerSentEventReceivedEventArgs>((o, e) =>
            {
                if(e != null && e.Message != null)
                {
                    onEvent(e.Message.Data);
                }
            });
            es.Start(cts.Token);
            es.CancellationToken = cts;

            return es;
        }
    }
}
