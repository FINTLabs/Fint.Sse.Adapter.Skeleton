using Fint.SSE.Adapter;
using Fint.SSE.Adapter.service;
using Fint.SSE.Customcode.Service;
using FintEventModel.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Fint.SSE.Test.Customcode
{
    [TestClass]
    public class EventHandlerServiceTest
    {
        [TestMethod]
        public void DoNothingIfEventStatusIsNotPROVIDER_ACCEPTED()
        {
            // Arrange
            var json = "{\"corrId\":\"c978c986-8d50-496f-8afd-8d27bd68049b\",\"action\":\"health\",\"status\":\"NEW\",\"time\":1481116509260,\"orgId\":\"rogfk.no\",\"source\":\"source\",\"client\":\"client\",\"message\":null,\"data\": \"\"}";
            var evtObj = EventUtil.ToEvent(json);

            var configService = new Mock<IConfigService>();
            configService.Setup(x => x.ResponseEndpoint).Returns("https://example.com/api/response");
            configService.Setup(x => x.StatusEndpoint).Returns("https://example.com/api/status");

            var statusService = new Mock<IEventStatusService>();
            statusService.Setup(s => s.VerifyEvent(It.IsAny<Event>())).Returns(() =>
            {
                evtObj.Status = Status.PROVIDER_REJECTED;
                return evtObj;
            });

            var httpService = new Mock<IHttpService>();
            httpService.Setup(x => x.Post(It.IsAny<string>(), It.IsAny<Event>()));
            var handlerService = new EventHandlerService(statusService.Object, httpService.Object, configService.Object);
            // Act
            handlerService.HandleEvent(evtObj);
            // Verify
            httpService.Verify(x => x.Post(It.IsAny<string>(), It.IsAny<Event>()), Times.Never());

        }

        [TestMethod]
        public void PostResponseIfEventStatusIsPROVIDER_ACCEPTED()
        {
            // Arrange
            var json = "{\"corrId\":\"c978c986-8d50-496f-8afd-8d27bd68049b\",\"action\":\"health\",\"status\":\"NEW\",\"time\":1481116509260,\"orgId\":\"rogfk.no\",\"source\":\"source\",\"client\":\"client\",\"message\":null,\"data\": \"\"}";
            var evtObj = EventUtil.ToEvent(json);

            var configService = new Mock<IConfigService>();
            configService.Setup(x => x.ResponseEndpoint).Returns("https://example.com/api/response");
            configService.Setup(x => x.StatusEndpoint).Returns("https://example.com/api/status");

            var statusService = new Mock<IEventStatusService>();
            statusService.Setup(s => s.VerifyEvent(It.IsAny<Event>())).Returns(() =>
            {
                evtObj.Status = Status.PROVIDER_ACCEPTED;
                return evtObj;
            });

            var httpService = new Mock<IHttpService>();
            httpService.Setup(x => x.Post(It.IsAny<string>(), It.IsAny<Event>()));
            var handlerService = new EventHandlerService(statusService.Object, httpService.Object, configService.Object);
            // Act
            handlerService.HandleEvent(evtObj);
            // Verify
            httpService.Verify(x => x.Post(It.IsAny<string>(), It.IsAny<Event>()), Times.Once());

        }
    }
}

