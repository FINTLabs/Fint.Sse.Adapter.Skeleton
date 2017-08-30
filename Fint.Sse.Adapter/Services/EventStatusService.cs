using Fint.Event.Model;
using Microsoft.Extensions.Options;

namespace Fint.Sse.Adapter.Services
{
    public class EventStatusService : IEventStatusService
    {
        private readonly IHttpService _httpService;
        private readonly AppSettings _appSettings;

        public EventStatusService(IHttpService httpService, IOptions<AppSettings> appSettings)
        {
            _httpService = httpService;
            _appSettings = appSettings.Value;
        }

        public Event<object> VerifyEvent(Event<object> serverSideEvent)
        {
            if (ActionUtils.IsValidStatusAction(serverSideEvent.Action) 
                || ActionUtils.IsValidPwfaAction(serverSideEvent.Action))
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
            _httpService.Post(_appSettings.StatusEndpoint, evt);
        }
    }
}
