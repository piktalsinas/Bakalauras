using Bakalauras.App.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Bakalauras.Domain.Models.Facebook;

namespace Bakalauras.API.Controllers
{
    [ApiController]
    [Route("webhook")]
    public class WebhookController : ControllerBase
    {
        private readonly ILogger<WebhookController> _logger;
        private readonly IFacebookPayloadHandler _payloadHandler;

        public WebhookController(
            ILogger<WebhookController> logger,
            IFacebookPayloadHandler payloadHandler)
        {
            _logger = logger;
            _payloadHandler = payloadHandler;
        }

        [HttpGet]
        public IActionResult Get([FromQuery(Name = "hub.mode")] string mode,
                                  [FromQuery(Name = "hub.verify_token")] string token,
                                  [FromQuery(Name = "hub.challenge")] string challenge)
        {
            if (mode == "subscribe" && token == "navigacija123")
                return Content(challenge, "text/plain");
            return Unauthorized();
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] object payload)
        {
            try
            {

                var jsonPayload = JsonConvert.DeserializeObject<FacebookWebhookPayload>(payload.ToString());
                await _payloadHandler.HandlePayloadAsync(jsonPayload);
                return Ok("EVENT_RECEIVED");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing webhook payload");
                return StatusCode(500, "Internal Server Error");
            }
        }
    }
}
