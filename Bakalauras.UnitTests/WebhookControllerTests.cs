using Bakalauras.API.Controllers;
using Bakalauras.App.Services;
using Bakalauras.Domain.Models.Facebook;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;

namespace Bakalauras.UnitTests
{

    public class WebhookControllerTests
    {
        private WebhookController _controller;
        private Mock<ILogger<WebhookController>> _loggerMock;
        private Mock<IFacebookPayloadHandler> _payloadHandlerMock;

        [SetUp]
        public void Setup()
        {
            _loggerMock = new Mock<ILogger<WebhookController>>();
            _payloadHandlerMock = new Mock<IFacebookPayloadHandler>();
            _controller = new WebhookController(_loggerMock.Object, _payloadHandlerMock.Object);
        }

        [Test]
        public void Get_Returns_Challenge_On_Valid_Subscription()
        {
            var mode = "subscribe";
            var token = "navigacija123";
            var challenge = "mockchallenge123";

            var result = _controller.Get(mode, token, challenge) as ContentResult;

            Assert.IsNotNull(result);
            Assert.AreEqual("text/plain", result.ContentType);
            Assert.AreEqual(challenge, result.Content);
        }

        [Test]
        public void Get_Returns_Unauthorized_On_Invalid_Subscription()
        {
            var mode = "invalidmode";
            var token = "navigacija123";
            var challenge = "mockchallenge123";

            var result = _controller.Get(mode, token, challenge);

            Assert.IsInstanceOf<UnauthorizedResult>(result);
        }

        [Test]
        public async Task Post_Returns_OK_On_Successful_Payload_Handling()
        {
            var mockPayload = new FacebookWebhookPayload { /* mock payload properties */ };
            var jsonPayload = JsonConvert.SerializeObject(mockPayload);

            _payloadHandlerMock.Setup(ph => ph.HandlePayloadAsync(It.IsAny<FacebookWebhookPayload>())).Returns(Task.CompletedTask);

            var result = await _controller.Post(jsonPayload);

            Assert.IsInstanceOf<OkObjectResult>(result);
            var okResult = result as OkObjectResult;
            Assert.AreEqual("EVENT_RECEIVED", okResult.Value);
        }

    }



}
