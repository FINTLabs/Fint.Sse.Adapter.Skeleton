using System;
using Fint.Event.Model;
using Fint.Event.Model.Health;
using Fint.Pwfa.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Fint.Sse.Adapter.Services
{
    public class EventHandlerService : IEventHandlerService
    {
        private readonly IEventStatusService _statusService;
        private readonly IHttpService _httpService;
        private readonly IPwfaService _pwfaService;
        private readonly ILogger<EventHandlerService> _logger;
        private readonly AppSettings _appSettings;

        public EventHandlerService(
            IEventStatusService statusService,
            IHttpService httpService,
            IPwfaService pwfaService,
            IOptions<AppSettings> appSettings,
            ILogger<EventHandlerService> logger)
        {
            _statusService = statusService;
            _httpService = httpService;
            _pwfaService = pwfaService;
            _logger = logger;
            _appSettings = appSettings.Value;
        }

        public void HandleEvent(Event<object> serverSideEvent)
        {
            if (serverSideEvent.IsHealthCheck())
            {
                PostHealthCheckResponse(serverSideEvent);
            }
            else
            {
                if (_statusService.VerifyEvent(serverSideEvent).Status == Status.ADAPTER_ACCEPTED)
                {
                    var action =
                        (PwfaActions) Enum.Parse(typeof(PwfaActions), serverSideEvent.Action, ignoreCase: true);
                    var responseEvent = serverSideEvent;

                    switch (action)
                    {
                        case PwfaActions.GET_DOG:
                            _pwfaService.GetDog(serverSideEvent);
                            break;
                        case PwfaActions.GET_OWNER:
                            _pwfaService.GetOwner(serverSideEvent);
                            break;
                        case PwfaActions.GET_ALL_DOGS:
                            _pwfaService.GetAllDogs(serverSideEvent);
                            break;
                        case PwfaActions.GET_ALL_OWNERS:
                            _pwfaService.GetAllOwners(serverSideEvent);
                            break;
                        default:
                            var message = $"Unhandled action: {action}";
                            _logger.LogError(message);
                            throw new Exception(message);
                    }

                    responseEvent.Status = Status.ADAPTER_RESPONSE;
                    _logger.LogInformation("POST EventResponse");
                    _httpService.Post(_appSettings.ResponseEndpoint, responseEvent);
                }
            }
        }

        private void PostHealthCheckResponse(Event<object> serverSideEvent)
        {
            var healthCheckEvent = serverSideEvent;
            healthCheckEvent.Status = Status.TEMP_UPSTREAM_QUEUE;

            if (IsHealthy())
            {
                healthCheckEvent.Data.Add(new Health("adapter", HealthStatus.APPLICATION_HEALTHY));
            }
            else
            {
                healthCheckEvent.Data.Add(new Health("adapter", HealthStatus.APPLICATION_UNHEALTHY));
                healthCheckEvent.Message = "The adapter is unable to communicate with the application.";
            }

            _httpService.Post(_appSettings.ResponseEndpoint, healthCheckEvent);
        }

        private bool IsHealthy()
        {
            //Check application connectivity etc.
            return true;
        }
    }
}