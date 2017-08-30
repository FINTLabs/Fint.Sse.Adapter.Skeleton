using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Fint.Event.Model;
using Microsoft.Extensions.Logging;

namespace Fint.Sse.Adapter.EventListeners
{
    public class AbstractEventSourceListener : IAbstractEventSourceListener
    {
        private readonly ILogger<AbstractEventSource> _logger;
        private readonly int MAX_UUIDS = 50;
        private ConcurrentBag<string> OrgIdList = new ConcurrentBag<string>();
        private ConcurrentBag<string> Uuids = new ConcurrentBag<string>();
        private EventSource EventSource { get; set; }
        private EventSource EventSource2 { get; set; }

        public AbstractEventSourceListener(ILogger<AbstractEventSource> logger)
        {
            _logger = logger;
        }

        public EventSource Listen(Uri url, Dictionary<string, string> headers, int timeout)
        {
            CancellationTokenSource cts = new CancellationTokenSource();

            var t = Task.Run(async delegate
            {
                CreateEventSource(url, headers, timeout);

                await Task.Delay(1500, cts.Token)
                    .ContinueWith(_ => CreateEventSource2(url, headers, timeout), cts.Token);
            });

            cts.Dispose();
            throw new NotImplementedException();
        }

        private void CreateEventSource(Uri url, Dictionary<string, string> headers, int timeout)
        {
            _logger.LogInformation("Starting First Event Source Listener");

            EventSource = new EventSource(url, headers, timeout);

            EventSource.StateChanged += new EventHandler<StateChangedEventArgs>((o, e) =>
            {
                _logger.LogInformation("{orgId}: SSE state change {@state} for uuid {uuid}");//, _orgId, e.State, uuid
            });

            EventSource.EventReceived += new EventHandler<ServerSentEventReceivedEventArgs>((o, e) =>
            {
                if (e?.Message != null)
                {
                    HandleEvent(e.Message.Data);
                }
            });
        }

        private void CreateEventSource2(Uri url, Dictionary<string, string> headers, int timeout)
        {
            _logger.LogInformation("Starting Second Event Source Listener");

            EventSource2 = new EventSource(url, headers, timeout);

            EventSource2.StateChanged += new EventHandler<StateChangedEventArgs>((o, e) =>
            {
                _logger.LogInformation("{orgId}: SSE state change {@state} for uuid {uuid}"); //, _orgId, e.State, uuid
            });

            EventSource2.EventReceived += new EventHandler<ServerSentEventReceivedEventArgs>((o, e) =>
            {
                if (e?.Message != null)
                {
                    HandleEvent(e.Message.Data);
                }
            });
        }

        public virtual void HandleEvent(string data)
        {
            var serverSideEvent = EventUtil.ToEvent<object>(data);

            if (serverSideEvent != null)
            {
                _logger.LogInformation("Event received {@Event}", data);
                //if (serverSideEvent.OrgId == _orgId)
                //{
                //    _logger.LogInformation("{orgId}: Event received {@Event}", _orgId, data);
                //    _eventHandlerService.HandleEvent(serverSideEvent);
                //}
                //else
                //{
                //    _logger.LogInformation("This is not EventListener for {org}", _orgId);
                //}
            }
            else
            {
                _logger.LogError("Could not parse Event object");
            }
        }


        //public AbstractEventSource(Uri url, int timeout) : base(url, timeout)
        //{
        //}

        //public AbstractEventSource(Uri url, Dictionary<string, string> headers, int timeout) : base(url, headers, timeout)
        //{
        //}

        //protected AbstractEventSource(Uri url, IWebRequesterFactory factory) : base(url, factory)
        //{
        //}

        //protected AbstractEventSource(Uri url, IWebRequesterFactory factory, Dictionary<string, string> headers) : base(url, factory, headers)
        //{
        //}

        //public override void OnEventReceived(ServerSentEvent sse)
        //{
        //    var json = sse.Data;

        //    var serverSideEvent = EventUtil.ToEvent<object>(sse.Data);

        //    if (IsNewCorrId(serverSideEvent.CorrId) && ContainsOrgId(serverSideEvent.OrgId)) {
        //        base.OnEventReceived(sse);
        //    }
        //}

        //private bool IsNewCorrId(string corrId)
        //{
        //    if (Uuids.Contains(corrId))
        //    {
        //        return false;
        //    }

        //    if (Uuids.Count >= MAX_UUIDS)
        //    {
        //        Uuids.First().Remove(0);
        //    }

        //    Uuids.Add(corrId);

        //    return true;
        //}

        //private bool ContainsOrgId(string orgId)
        //{
        //    if (OrgIdList != null && OrgIdList.Contains(orgId))
        //    {
        //        return true;
        //    }

        //    //TODO: Find out how to parse orgIds
        //    return true;
        //    return false;
        //}

    }
}
