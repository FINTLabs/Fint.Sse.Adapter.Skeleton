using Fint.Event.Model;
using Fint.Sse.Adapter.Models;
using Fint.Sse.Adapter.Services;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Fint.Sse.Adapter.Tests.Services
{
    public class EventStatusServiceTest
    {
        [Fact]
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
            Assert.Equal(Status.PROVIDER_ACCEPTED, evt.Status);
            httpService.Verify(x => x.Post(It.IsAny<string>(), It.IsAny<Event<object>>()), Times.Once());
        }

        [Fact]
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
            Assert.Equal(Status.PROVIDER_REJECTED, evt.Status);
            httpService.Verify(x => x.Post(It.IsAny<string>(), It.IsAny<Event<object>>()), Times.Once());
        }
    }
}
