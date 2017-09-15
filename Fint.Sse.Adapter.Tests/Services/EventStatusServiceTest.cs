using Fint.Event.Model;
using Fint.Sse.Adapter.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Fint.Sse.Adapter.Tests.Services
{
    public class EventStatusServiceTest
    {
        public EventStatusServiceTest()
        {
            _httpService = new Mock<IHttpService>();
            _logger = new Mock<ILogger<HttpService>>();
            _appSettingsMock = new Mock<IOptions<AppSettings>>();
            _appSettingsMock.Setup(ap => ap.Value).Returns(new AppSettings
            {
                ResponseEndpoint = "https://example.com/api/response",
                StatusEndpoint = "https://example.com/api/status"
            });
        }

        [Fact]
        public void Given_VerifiedAction_Should_Accept()
        {
            // Arrange
            _httpService.Setup(x => x.Post(It.IsAny<string>(), It.IsAny<Event<object>>()));

            // Act
            var statusService = new EventStatusService(_logger.Object, _httpService.Object, _appSettingsMock.Object);
            var evt = statusService.VerifyEvent(new Event<object> {Action = "health"});

            // Verify
            Assert.Equal(Status.ADAPTER_ACCEPTED, evt.Status);
            _httpService.Verify(x => x.Post(It.IsAny<string>(), It.IsAny<Event<object>>()), Times.Once());
        }

        [Fact]
        public void Given_UndefinedEvent_Should_Reject()
        {
            // Arrange
            _httpService.Setup(x => x.Post(It.IsAny<string>(), It.IsAny<Event<object>>()));

            // Act
            var statusService = new EventStatusService(_logger.Object, _httpService.Object, _appSettingsMock.Object);
            var evt = statusService.VerifyEvent(new Event<object> {Action = "SomeUndefinedAction"});

            // Verify
            Assert.Equal(Status.ADAPTER_REJECTED, evt.Status);
            _httpService.Verify(x => x.Post(It.IsAny<string>(), It.IsAny<Event<object>>()), Times.Once());
        }

        private readonly Mock<IOptions<AppSettings>> _appSettingsMock;
        private readonly Mock<IHttpService> _httpService;
        private readonly Mock<ILogger<HttpService>> _logger;
    }
}