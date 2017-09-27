# FINT SSE Adapter Skeleton

## Introduction
This project is a skeleton for develop FINT .NET SSE adapter. You can find more information about adapter development in
the following links:

* [Overview](https://fintprosjektet.github.io/adapter/overview/)
* [Tutorial](https://fintprosjektet.github.io/adapter/tut-dotnet-sse/)

## Packages and files
The solution Fint.SSE.Adapter is divided into 2 main projects. The `Fint.Sse.Adapter.Console` is a console application for with log information. The `Fint.SSe.Adapter` is the actual adapter.
In this project there is a test implementation of a PwfaService. In order to implement your own service you can take this as a starting point.  

### EventHandlerService.cs
The actions is handled in the `handleEvent()` method. The actions are defined as enums in the models:

```java
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
                    var action =(PwfaActions) Enum.Parse(typeof(PwfaActions), serverSideEvent.Action, ignoreCase: true);
                    Event<object> responseEvent = serverSideEvent;

					/*
					* Add statements for all the actions
					*/
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
                    LoggerExtensions.LogInformation(_logger, "POST EventResponse");
                    _httpService.Post(_appSettings.ResponseEndpoint, responseEvent);
                }
            }
        }
```

### EventStatusService.cs
The status events are handled in the `VerifyEvent` method. 

```java
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
```

## Adapter configuration
| Key | Description | Example |
|-----|-------------|---------|
| fint.adapter.organizations | List of orgIds the adapter handles. | rogfk.no, vaf.no, ofk.no |
| fint.adapter.sse-endpoint | Url to the sse endpoint. | https://play-with-fint-adapter.felleskomponent.no/provider/sse/%s |
| fint.adapter.status-endpoint | Url to the status endpoint. | https://play-with-fint-adapter.felleskomponent.no//provider/status |
| fint.adapter.response-endpoint | Url to the response endpoint. | https://play-with-fint-adapter.felleskomponent.no/provider/response |

## Links
[App settings as environment varibles](https://www.scottbrady91.com/Docker/ASPNET-Core-and-Docker-Environment-Variables)
