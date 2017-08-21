using System.Collections.Generic;
using Fint.Event.Model;
using Fint.Pwfa.Model;
using Fint.Sse.Adapter.Models;
using Fint.Sse.Adapter.Services;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Fint.Sse.Adapter.Test.Services
{
    [TestClass]
    public class EventHandlerServiceTest
    {
        [TestMethod]
        public void DoNothingIfEventStatusIsNotPROVIDER_ACCEPTED()
        {
            // Arrange
            var json = "{\"corrId\":\"c978c986-8d50-496f-8afd-8d27bd68049b\",\"action\":\"health\",\"status\":\"NEW\",\"time\":1481116509260,\"orgId\":\"rogfk.no\",\"source\":\"source\",\"client\":\"client\",\"message\":null,\"data\": \"\"}";
            var evtObj = EventUtil.ToEvent<object>(json);

            var appSettings = new AppSettings
            {
                ResponseEndpoint = "https://example.com/api/response",
                StatusEndpoint = "https://example.com/api/status"
            };
            var configServiceMock = new Mock<IOptions<AppSettings>>();
            configServiceMock.Setup(ap => ap.Value).Returns(appSettings);

            var statusService = new Mock<IEventStatusService>();
            statusService.Setup(s => s.VerifyEvent(It.IsAny<Event<object>>())).Returns(() =>
            {
                evtObj.Status = Status.PROVIDER_REJECTED;
                return evtObj;
            });

            var httpService = new Mock<IHttpService>();
            httpService.Setup(x => x.Post(It.IsAny<string>(), It.IsAny<Event<object>>()));
            var handlerService = new EventHandlerService(statusService.Object, httpService.Object, configServiceMock.Object);
            // Act
            handlerService.HandleEvent(evtObj);
            // Verify
            httpService.Verify(x => x.Post(It.IsAny<string>(), It.IsAny<Event<object>>()), Times.Never());

        }

        [TestMethod]
        public void PostResponseIfEventStatusIsPROVIDER_ACCEPTED()
        {
            // Arrange
            var json = "{\"corrId\":\"c978c986-8d50-496f-8afd-8d27bd68049b\",\"action\":\"health\",\"status\":\"NEW\",\"time\":1481116509260,\"orgId\":\"rogfk.no\",\"source\":\"source\",\"client\":\"client\",\"message\":null,\"data\": \"\"}";
            var evtObj = EventUtil.ToEvent<object>(json);

            var appSettings = new AppSettings
            {
                ResponseEndpoint = "https://example.com/api/response",
                StatusEndpoint = "https://example.com/api/status"
            };
            var configServiceMock = new Mock<IOptions<AppSettings>>();
            configServiceMock.Setup(ap => ap.Value).Returns(appSettings);

            var statusService = new Mock<IEventStatusService>();
            statusService.Setup(s => s.VerifyEvent(It.IsAny<Event<object>>())).Returns(() =>
            {
                evtObj.Status = Status.PROVIDER_ACCEPTED;
                evtObj.Action = PwfaActions.GET_ALL_DOGS.ToString();
                evtObj.Data = new List<object>();

                return evtObj;
            });

            var httpService = new Mock<IHttpService>();
            httpService.Setup(x => x.Post(It.IsAny<string>(), It.IsAny<Event<object>>()));
            var handlerService = new EventHandlerService(statusService.Object, httpService.Object, configServiceMock.Object);
            // Act
            handlerService.HandleEvent(evtObj);
            // Verify
            httpService.Verify(x => x.Post(It.IsAny<string>(), It.IsAny<Event<object>>()), Times.Once());

        }
    }
}

