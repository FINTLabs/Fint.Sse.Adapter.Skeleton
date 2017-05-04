using Fint.SSE.Adapter;
using Fint.SSE.Adapter.service;
using FintEventModel.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Fint.SSE.Test.Adapter.sse
{
    [TestClass]
    public class EventStatusServiceTest
    {
        [TestMethod]
        public void VerifyDefinedEvent()
        {
            // Arrange
            var configService = new Mock<IConfigService>();
            configService.Setup(x => x.ResponseEndpoint).Returns("https://example.com/api/response");
            configService.Setup(x => x.StatusEndpoint).Returns("https://example.com/api/status");
            var httpService = new Mock<IHttpService>();
            httpService.Setup(x => x.Post(It.IsAny<string>(), It.IsAny<Event>()));

            // Act
            var statusService = new EventStatusService(httpService.Object, configService.Object);
            var evt = statusService.VerifyEvent(new Event { Action = "health" });

            // Verify
            Assert.AreEqual(Status.PROVIDER_ACCEPTED, evt.Status);
            httpService.Verify(x => x.Post(It.IsAny<string>(), It.IsAny<Event>()), Times.Once());
        }

        [TestMethod]
        public void RejectUndefinedEvent()
        {
            // Arrange
            var configService = new Mock<IConfigService>();
            configService.Setup(x => x.ResponseEndpoint).Returns("https://example.com/api/response");
            configService.Setup(x => x.StatusEndpoint).Returns("https://example.com/api/status");
            var httpService = new Mock<IHttpService>();
            httpService.Setup(x => x.Post(It.IsAny<string>(), It.IsAny<Event>()));

            // Act
            var statusService = new EventStatusService(httpService.Object, configService.Object);
            var evt = statusService.VerifyEvent(new Event { Action = "SomeUndefinedAction" });

            // Verify
            Assert.AreEqual(Status.PROVIDER_REJECTED, evt.Status);
            httpService.Verify(x => x.Post(It.IsAny<string>(), It.IsAny<Event>()), Times.Once());
        }
    }
}
