using System;
using System.Collections.Generic;
using System.Linq;
using Fint.SSE.Adapter.Event;
using Fint.SSE.Adapter.Service;
using Fint.Event.Model;
using Fint.Event.Model.Health;
using Fint.Relation.Model;
using Fint.Pwfa.Model;

namespace Fint.SSE.Customcode.Service
{
    public class EventHandlerService : IEventHandlerService
    {
        private IEventStatusService _statusService;
        private IHttpService _httpService;
        private IConfigService _configService;
        private IEnumerable<Owner> _owners;
        private IEnumerable<Dog> _dogs;

        public EventHandlerService(
            IEventStatusService statusService,
            IHttpService httpService,
            IConfigService configService)
        {
            _statusService = statusService;
            _httpService = httpService;
            _configService = configService;

            SetupPwfaData();
        }

        public EventHandlerService()
        {
            _httpService = new HttpService();
            _configService = new ConfigService();
            _statusService = new EventStatusService(_httpService, _configService);

            SetupPwfaData();
        }

        private void SetupPwfaData()
        {
            _owners = new List<Owner>
            {
                new Owner
                {
                    Id = "10",
                    Name = "Mickey Mouse"
                },
                new Owner
                {
                    Id = "20",
                    Name = "Minne Mouse"
                }
            };

            _dogs = new List<Dog>
            {
                new Dog {Id = "1", Name = "Pluto", Breed = "Working Springer Spaniel"},
                new Dog {Id = "2", Name = "Lady", Breed = "Working Springer Spaniel"}
            };
        }

        public void HandleEvent(Event<object> serverSideEvent)
        {
            if (serverSideEvent.IsHealthCheck())
            {
                PostHealthCheckResponse(serverSideEvent);
            }
            else
            {
                if (_statusService.VerifyEvent(serverSideEvent).Status == Status.PROVIDER_ACCEPTED)
                {
                    var action =(PwfaActions) Enum.Parse(typeof(PwfaActions), serverSideEvent.Action, ignoreCase: true);
                    Event<object> responseEvent = serverSideEvent;

                    switch (action)
                    {
                        case PwfaActions.GET_DOG:
                            GetDog(serverSideEvent);
                            break;
                        case PwfaActions.GET_OWNER:
                            GetOwner(serverSideEvent);
                            break;
                        case PwfaActions.GET_ALL_DOGS:
                            GetAllDog(serverSideEvent);
                            break;
                        case PwfaActions.GET_ALL_OWNERS:
                            GetAllOwners(serverSideEvent);

                            break;
                        default:
                            throw new Exception($"Unhandled action: {action}");
                            break;
                    }

                    if (responseEvent != null)
                    {
                        responseEvent.Status = Status.PROVIDER_RESPONSE;
                        _httpService.Post(_configService.ResponseEndpoint, responseEvent);
                    }
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

            _httpService.Post(_configService.ResponseEndpoint, healthCheckEvent);
        }

        private void GetDog(Event<object> serverSideEvent)
        {
            var dog = _dogs.FirstOrDefault(d => d.Id.Equals(serverSideEvent.Query));

            var relation = new RelationBuilder()
                .With(Dog.Relasjonsnavn.OWNER)
                .ForType("Owner") //TODO
                .Value($"{dog?.Id}0")
                .Build();

            var resource = FintResource<Dog>
                .With(dog)
                .AddRelations(relation);

            serverSideEvent.Data.Add(resource);

        }

        private void GetOwner(Event<object> serverSideEvent)
        {
            var owner = _owners.FirstOrDefault(o => o.Id.Equals(serverSideEvent.Query));

            var relation = new RelationBuilder()
                .With(Owner.Relasjonsnavn.DOG)
                .ForType("Dog") //TODO
                .Value($"{owner?.Id}")
                .Build();

            var resource = FintResource<Owner>
                .With(owner)
                .AddRelations(relation);

            serverSideEvent.Data.Add(resource);

        }

        private void GetAllDog(Event<object> serverSideEvent)
        {
            var relationOwner1 = new RelationBuilder()
                .With(Dog.Relasjonsnavn.OWNER)
                .ForType("Owner") //TODO
                .Value("10")
                .Build();

            var relationOwner2 = new RelationBuilder()
                .With(Dog.Relasjonsnavn.OWNER)
                .ForType("Owner") //TODO
                .Value("20")
                .Build();

            var dogs = _dogs.ToList();

            serverSideEvent.Data.Add(FintResource<Dog>.With(dogs[0]).AddRelations(relationOwner1));
            serverSideEvent.Data.Add(FintResource<Dog>.With(dogs[1]).AddRelations(relationOwner2));

        }

        private void GetAllOwners(Event<object> serverSideEvent)
        {
            var relationDog1 = new RelationBuilder()
                .With(Owner.Relasjonsnavn.DOG)
                .ForType("Dog") //TODO
                .Value("1")
                .Build();

            var relationDog2 = new RelationBuilder()
                .With(Owner.Relasjonsnavn.DOG)
                .ForType("Dog") //TODO
                .Value("2")
                .Build();

            var owners = _owners.ToList();

            serverSideEvent.Data.Add(FintResource<Owner>.With(owners[0]).AddRelations(relationDog1));
            serverSideEvent.Data.Add(FintResource<Owner>.With(owners[1]).AddRelations(relationDog2));

        }

        private Event<object> onHealthCheck(Event<object> evtObj)
        {
            if (evtObj.Data == null)
            {
                evtObj.Data = new List<object>();
            }

            if (IsHealthy())
            {
                evtObj.Data.Add("I'm fine thanks! How are you?");
            }
            else
            {
                evtObj.Data.Add("Oh, I'm feeling bad! How are you?");
            }
            return evtObj;
        }

        private bool IsHealthy()
        {
            /**
             * Check application connectivity etc.
             */
            return true;
        }
    }
}