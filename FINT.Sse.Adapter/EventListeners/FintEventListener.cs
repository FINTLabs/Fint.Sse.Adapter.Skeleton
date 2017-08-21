using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using EventSource4Net;
using Fint.Event.Model;
using Fint.Sse.Adapter.Models;
using Fint.Sse.Adapter.Services;

namespace Fint.SSE.Adapter.EventListeners
{
    public class FintEventListener : IFintEventListener
    {
        private readonly IEventHandlerService _eventHandlerService;
        private readonly AppSettings _configurationOptions;
        private readonly ILogger<FintEventListener> _logger;
        private string _orgId;

        public FintEventListener(
            IEventHandlerService handlerService,
            IOptions<AppSettings> configurationOptions,
            ILogger<FintEventListener> logger)
        {
            _eventHandlerService = handlerService;
            _configurationOptions = configurationOptions.Value;
            _logger = logger;
        }

       
        public EventSource Listen(string orgId)
        {
            _orgId = orgId;

            var cancellationTokenSource = new CancellationTokenSource();

            var headers = new Dictionary<string, string>
            {
                { FintHeaders.ORG_ID_HEADER, orgId }
            };

            var url = new Uri(string.Format("{0}/{1}", _configurationOptions.SseEndpoint, Guid.NewGuid().ToString()));

            var eventSource = new EventSource(url, headers, 10000);

            eventSource.StateChanged += new EventHandler<StateChangedEventArgs>((o, e) =>
            {
                _logger.LogInformation("{orgId}: SSE state change {@state}", _orgId, e.State);
            });

            eventSource.EventReceived += new EventHandler<ServerSentEventReceivedEventArgs>((o, e) =>
            {
                if(e != null && e.Message != null)
                {
                    HandleEvent(e.Message.Data);
                }
            });

            eventSource.Start(cancellationTokenSource.Token);
            eventSource.CancellationToken = cancellationTokenSource;

            return eventSource;
        }

        private void HandleEvent(string data)
        {
            var serverSideEvent = EventUtil.ToEvent<object>(data);

            if (serverSideEvent == null)
            {
                _logger.LogError("Could not parse Event object");
            }
            else
            {
                if (serverSideEvent.OrgId == _orgId)
                {
                    _logger.LogInformation("{orgId}: Event received {@Event}", _orgId, data);
                    _eventHandlerService.HandleEvent(serverSideEvent);
                }
                else
                {
                    _logger.LogInformation("This is not EventListener for {org}", _orgId);
                }
            }
        }
    }
}
