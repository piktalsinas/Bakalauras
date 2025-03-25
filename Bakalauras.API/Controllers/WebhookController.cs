using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Bakalauras.API.Controllers
{
    [ApiController]
    [Route("webhook")]
    public class WebhookController : ControllerBase
    {
        private readonly ILogger<WebhookController> _logger;

        public WebhookController(ILogger<WebhookController> logger)
        {
            _logger = logger;
        }

        private const string VERIFY_TOKEN = "navigacija123"; // Change this to your actual verification token

        // Step 1: Verification Endpoint
        [HttpGet]
        public IActionResult VerifyWebhook([FromQuery] string hub_mode, [FromQuery] string hub_challenge, [FromQuery] string hub_verify_token)
        {
            if (hub_mode == "subscribe" && hub_verify_token == VERIFY_TOKEN)
            {
                return Ok(hub_challenge);
            }
            return Unauthorized();
        }

        [HttpPost]
        public async Task<IActionResult> ReceiveWebhook([FromBody] JObject requestBody)
        {
            try
            {
                _logger.LogInformation($"Received POST request body: {requestBody.ToString()}");

                var entry = requestBody["entry"]?.First;
                var messaging = entry?["messaging"]?.First;

                if (messaging != null)
                {
                    var senderId = messaging["sender"]?["id"]?.ToString();
                    var messageText = messaging["message"]?["text"]?.ToString();

                    if (!string.IsNullOrEmpty(senderId) && !string.IsNullOrEmpty(messageText))
                    {
                        await HandleMessageAsync(senderId, messageText);
                    }
                }

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error processing webhook: {ex.Message}");
                return StatusCode(500, "Internal Server Error");
            }
        }


        private async Task HandleMessageAsync(string senderId, string messageText)
        {
            // Here you can integrate API logic based on the received message
            _logger.LogInformation($"Received message: {messageText} from {senderId}");

            // Example: Reply with a simple message
            await SendMessageAsync(senderId, $"You said: {messageText}");
        }

        private async Task SendMessageAsync(string recipientId, string messageText)
        {
            var payload = new
            {
                recipient = new { id = recipientId },
                message = new { text = messageText }
            };

            // Send request to Facebook Messenger API (replace with your own logic)
            _logger.LogInformation($"Sending message to {recipientId}: {messageText}");
        }
    }
}
