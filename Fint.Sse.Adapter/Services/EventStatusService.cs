using Fint.Event.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Fint.Sse.Adapter.Services
{
    public class EventStatusService : IEventStatusService
    {
        private readonly IHttpService _httpService;
        private readonly AppSettings _appSettings;
        private readonly ILogger _logger;

        public EventStatusService(ILogger<HttpService> logger, IHttpService httpService,
            IOptions<AppSettings> appSettings)
        {
            _httpService = httpService;
            _appSettings = appSettings.Value;
            _logger = logger;
        }

        public Event<object> VerifyEvent(Event<object> serverSideEvent)
        {
            if (ActionUtils.IsValidStatusAction(serverSideEvent.Action)
                || ActionUtils.IsValidPwfaAction(serverSideEvent.Action))
            {
                serverSideEvent.Status = Status.ADAPTER_ACCEPTED;
            }
            else
            {
                serverSideEvent.Status = Status.ADAPTER_REJECTED;
            }

            serverSideEvent.Data?.Clear();

            PostStatus(serverSideEvent);

            return serverSideEvent;
        }

        private void PostStatus(Event<object> evt)
        {
            _logger.LogInformation("POST Status");
            _httpService.Post(_appSettings.StatusEndpoint, evt);
        }
    }
}