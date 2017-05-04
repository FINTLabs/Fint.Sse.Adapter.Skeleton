using System;
using Fint.SSE.Adapter;
using FintEventModel.Model;
using Fint.SSE.Adapter.service;

namespace Fint.SSE.Customcode.Service
{
    public class EventHandlerService : IEventHandlerService
    {
        private IEventStatusService _statusService;
        private IHttpService _httpService;
        private IConfigService _configService;
        public EventHandlerService(IEventStatusService statusService,
            IHttpService httpService, IConfigService configService)
        {
            _statusService = statusService;
            _httpService = httpService;
            _configService = configService;
        }

        public EventHandlerService()
        {
            _httpService = new HttpService();
            _configService = new ConfigService();
            _statusService = new EventStatusService(_httpService, _configService);

        }

        public void HandleEvent(Event evtObj)
        {
            if (evtObj != null
                && _statusService.VerifyEvent(evtObj).Status == Status.PROVIDER_ACCEPTED)
            {
                if (ActionUtils.IsValidAction(evtObj.Action))
                {
                    Event responseEvent = null;
                    var action = (Action) Enum.Parse(typeof(Action), evtObj.Action, ignoreCase: true);

                    if (action == Action.HEALTH)
                    {
                        responseEvent = onHealthCheck(evtObj);
                    }

                    /**
                     * Add if statements for all the actions
                     */

                    if (responseEvent != null)
                    {
                        responseEvent.Status = Status.PROVIDER_RESPONSE;
                        _httpService.Post(_configService.ResponseEndpoint, responseEvent);
                    }
                }
            }
        }

        private Event onHealthCheck(Event evtObj)
        {
            if (isHealthy())
            {
                evtObj.Data = "I'm fine thanks! How are you?";
            }
            else
            {
                evtObj.Data = "Oh, I'm feeling bad! How are you?";
            }
            return evtObj;
        }

        private bool isHealthy()
        {
            /**
             * Check application connectivity etc.
             */
            return true;
        }
    }
}
