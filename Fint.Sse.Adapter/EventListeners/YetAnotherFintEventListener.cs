using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Fint.Event.Model;
using Fint.Sse.Adapter.Models;
using Fint.Sse.Adapter.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Fint.Sse.Adapter.EventListeners
{
    public class YetAnotherFintEventListener : IFintEventListener
    {
        private readonly IEventHandlerService _eventHandlerService;
        private readonly IAbstractEventSourceListener _abstractEventSourceListener;
        private readonly AppSettings _appSettings;
        private readonly ILogger<FintEventListener> _logger;
        private string _orgId;

        public YetAnotherFintEventListener(
            IEventHandlerService handlerService,
            IOptions<AppSettings> configurationOptions,
            IAbstractEventSourceListener abstractEventSourceListener,
            ILogger<FintEventListener> logger)
        {
            _eventHandlerService = handlerService;
            _abstractEventSourceListener = abstractEventSourceListener;
            _appSettings = configurationOptions.Value;
            _logger = logger;
        }

        public EventSource Listen(string orgId)
        {
            _orgId = orgId;

            return RegisterEventSource(orgId);
        }

        private EventSource RegisterEventSource(string orgId)
        {
            var headers = new Dictionary<string, string>
            {
                {FintHeaders.ORG_ID_HEADER, orgId}
            };

            var uuid = Guid.NewGuid().ToString();
            var url = new Uri(string.Format("{0}/{1}", _appSettings.SseEndpoint, uuid));

            var eventSource = _abstractEventSourceListener.Listen(url, headers, 10000);

            eventSource.StateChanged += new EventHandler<StateChangedEventArgs>((o, e) =>
            {
                _logger.LogInformation("{orgId}: SSE state change {@state} for uuid {uuid}", orgId, e.State, uuid);
            });

            eventSource.EventReceived += new EventHandler<ServerSentEventReceivedEventArgs>((o, e) =>
            {
                if (e?.Message != null)
                {
                    HandleEvent(e.Message.Data, orgId);
                }
            });

            var cancellationTokenSource = new CancellationTokenSource();
            eventSource.Start(cancellationTokenSource.Token);
            eventSource.CancellationToken = cancellationTokenSource;

            return eventSource;
        }

        private void HandleEvent(string data, string orgId)
        {
            var serverSideEvent = EventUtil.ToEvent<object>(data);

            if (serverSideEvent != null)
                if (serverSideEvent.OrgId == orgId)
                {
                    _logger.LogInformation("{orgId}: Event received {@Event}", orgId, data);
                    _eventHandlerService.HandleEvent(serverSideEvent);
                }
                else
                {
                    _logger.LogInformation("This is not EventListener for {org}", orgId);
                }
            else
            {
                _logger.LogError("Could not parse Event object");
            }
        }

    }
}
