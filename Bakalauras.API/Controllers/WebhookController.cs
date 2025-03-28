using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Bakalauras.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WebhookController : ControllerBase
    {
        private readonly ILogger<WebhookController> _logger;
        private readonly string _verifyToken = "navigacija123"; // Same token as you put in Meta dashboard

        public WebhookController(ILogger<WebhookController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Get(
            [FromQuery(Name = "hub.mode")] string hubMode,
            [FromQuery(Name = "hub.verify_token")] string hubVerifyToken,
            [FromQuery(Name = "hub.challenge")] string hubChallenge)
        {
            _logger.LogInformation($"Verification request: mode={hubMode}, token={hubVerifyToken}, challenge={hubChallenge}");

            if (hubMode == "subscribe" && hubVerifyToken == _verifyToken)
            {
                return Content(hubChallenge, "text/plain"); // IMPORTANT: must return raw text
            }

            return Unauthorized();
        }

        [HttpPost]
        public IActionResult Post([FromBody] object payload)
        {
            _logger.LogInformation("Received Facebook event:");
            _logger.LogInformation(payload.ToString());

            return Ok("EVENT_RECEIVED");
        }
    }
}
