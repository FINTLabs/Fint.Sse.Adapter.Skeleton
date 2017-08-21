using Fint.SSE.Adapter.Service;
using Fint.SSE.Customcode;
using Fint.Event.Model;
using Microsoft.Extensions.Options;

namespace Fint.SSE.Adapter.Event
{
    public class EventStatusService : IEventStatusService
    {
        private readonly IHttpService _httpService;
        private readonly AppSettings _configService;

        public EventStatusService(IHttpService httpService, IOptions<AppSettings>  configService)
        {
            _httpService = httpService;
            _configService = configService.Value;
        }

        public Event<object> VerifyEvent(Event<object> serverSideEvent)
        {
            if (ActionUtils.IsValidAction(serverSideEvent.Action))
            {
                serverSideEvent.Status = Status.PROVIDER_ACCEPTED;
            }
            else
            {
                serverSideEvent.Status = Status.PROVIDER_REJECTED;
            }

            serverSideEvent.Data?.Clear();

            PostStatus(serverSideEvent);

            return serverSideEvent;
        }

        private void PostStatus(Event<object> evt)
        {
            _httpService.Post(_configService.StatusEndpoint, evt);
        }
    }
}
