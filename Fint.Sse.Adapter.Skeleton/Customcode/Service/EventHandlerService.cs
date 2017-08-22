using System;
using System.Collections.Generic;
using System.Linq;
using Fint.Event.Model;
using Fint.Event.Model.Health;
using Fint.Pwfa.Model;
using Fint.Relation.Model;
using Fint.Sse.Adapter.Event;
using Fint.Sse.Adapter.Service;

namespace Fint.Sse.Customcode.Service
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
                .ForType("no.fint.pwfa.model.Owner") //TODO
                .Value($"{dog?.Id}0")
                .Build();

            var resource = FintResource<Dog>
                .With(dog)
                .AddRelasjoner(relation);

            serverSideEvent.Data.Add(resource);

        }

        private void GetOwner(Event<object> serverSideEvent)
        {
            var owner = _owners.FirstOrDefault(o => o.Id.Equals(serverSideEvent.Query));

            var relation = new RelationBuilder()
                .With(Owner.Relasjonsnavn.DOG)
                .ForType("no.fint.pwfa.model.Dog") //TODO
                .Value($"{owner?.Id.Substring(0, 1)}")
                .Build();

            var resource = FintResource<Owner>
                .With(owner)
                .AddRelasjoner(relation);

            serverSideEvent.Data.Add(resource);

        }

        private void GetAllDog(Event<object> serverSideEvent)
        {
            var relationOwner1 = new RelationBuilder()
                .With(Dog.Relasjonsnavn.OWNER)
                .ForType("no.fint.pwfa.model.Owner") //TODO
                .Value("10")
                .Build();

            var relationOwner2 = new RelationBuilder()
                .With(Dog.Relasjonsnavn.OWNER)
                .ForType("no.fint.pwfa.model.Owner") //TODO
                .Value("20")
                .Build();

            var dogs = _dogs.ToList();

            serverSideEvent.Data.Add(FintResource<Dog>.With(dogs[0]).AddRelasjoner(relationOwner1));
            serverSideEvent.Data.Add(FintResource<Dog>.With(dogs[1]).AddRelasjoner(relationOwner2));

        }

        private void GetAllOwners(Event<object> serverSideEvent)
        {
            var relationDog1 = new RelationBuilder()
                .With(Owner.Relasjonsnavn.DOG)
                .ForType("no.fint.pwfa.model.Dog") //TODO
                .Value("1")
                .Build();

            var relationDog2 = new RelationBuilder()
                .With(Owner.Relasjonsnavn.DOG)
                .ForType("no.fint.pwfa.model.Dog") //TODO
                .Value("2")
                .Build();

            var owners = _owners.ToList();

            serverSideEvent.Data.Add(FintResource<Owner>.With(owners[0]).AddRelasjoner(relationDog1));
            serverSideEvent.Data.Add(FintResource<Owner>.With(owners[1]).AddRelasjoner(relationDog2));

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