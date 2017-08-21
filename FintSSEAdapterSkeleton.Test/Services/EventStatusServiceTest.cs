using Fint.Event.Model;
using Fint.Sse.Adapter.Models;
using Fint.Sse.Adapter.Services;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Fint.Sse.Adapter.Test.Services
{
    [TestClass]
    public class EventStatusServiceTest
    {
        [TestMethod]
        public void VerifyDefinedEvent()
        {
            // Arrange
            var appSettings = new AppSettings
            {
                ResponseEndpoint = "https://example.com/api/response",
                StatusEndpoint = "https://example.com/api/status"
            };
            var configServiceMock = new Mock<IOptions<AppSettings>>();
            configServiceMock.Setup(ap => ap.Value).Returns(appSettings);

            var httpService = new Mock<IHttpService>();
            httpService.Setup(x => x.Post(It.IsAny<string>(), It.IsAny<Event<object>>()));

            // Act
            var statusService = new EventStatusService(httpService.Object, configServiceMock.Object);
            var evt = statusService.VerifyEvent(new Event<object> { Action = "health" });

            // Verify
            Assert.AreEqual(Status.PROVIDER_ACCEPTED, evt.Status);
            httpService.Verify(x => x.Post(It.IsAny<string>(), It.IsAny<Event<object>>()), Times.Once());
        }

        [TestMethod]
        public void RejectUndefinedEvent()
        {
            // Arrange
            var appSettings = new AppSettings
            {
                ResponseEndpoint = "https://example.com/api/response",
                StatusEndpoint = "https://example.com/api/status"
            };
            var configServiceMock = new Mock<IOptions<AppSettings>>();
            configServiceMock.Setup(ap => ap.Value).Returns(appSettings);

            var httpService = new Mock<IHttpService>();
            httpService.Setup(x => x.Post(It.IsAny<string>(), It.IsAny<Event<object>>()));

            // Act
            var statusService = new EventStatusService(httpService.Object, configServiceMock.Object);
            var evt = statusService.VerifyEvent(new Event<object> { Action = "SomeUndefinedAction" });

            // Verify
            Assert.AreEqual(Status.PROVIDER_REJECTED, evt.Status);
            httpService.Verify(x => x.Post(It.IsAny<string>(), It.IsAny<Event<object>>()), Times.Once());
        }
    }
}
