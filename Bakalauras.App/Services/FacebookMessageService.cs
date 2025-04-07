using Bakalauras.Domain.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Text;


namespace Bakalauras.App.Services
{

    public class FacebookMessageService
    {
        private readonly string _pageAccessToken;
        private readonly string _baseUrl;
        private readonly HttpClient _httpClient;
        private readonly ILogger<LanguageService> _logger;

        public FacebookMessageService(IOptions<AppSettings> appSettings, ILogger<LanguageService> logger)
        {
            _pageAccessToken = appSettings.Value.PageAccessToken;
            _baseUrl = appSettings.Value.BaseUrl;
            _httpClient = new HttpClient();
            _logger = logger;
        }

        private string ImageBaseUrl => $"{_baseUrl}/images";

        public async Task SendTextAsync(string recipientId, string text, dynamic quickReplies = null)
        {
            var url = $"https://graph.facebook.com/v10.0/me/messages?access_token={_pageAccessToken}";
            var payload = new
            {
                recipient = new { id = recipientId },
                message = new { text, quick_replies = quickReplies }
            };
            var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");

            // Retry logic for better reliability
            int retries = 3;
            while (retries > 0)
            {
                try
                {
                    var response = await _httpClient.PostAsync(url, content);
                    if (response.IsSuccessStatusCode)
                    {
                        break;
                    }
                }
                catch (Exception ex)
                {
                    // Log the exception
                    _logger.LogError($"Error sending message: {ex.Message}");
                }
                retries--;
                if (retries == 0)
                {
                    throw new Exception("Failed to send message after 3 attempts");
                }
            }
        }

        public async Task SendImageAsync(string recipientId, string imageUrl)
        {
            var url = $"https://graph.facebook.com/v10.0/me/messages?access_token={_pageAccessToken}";
            var payload = new
            {
                recipient = new { id = recipientId },
                message = new
                {
                    attachment = new
                    {
                        type = "image",
                        payload = new { url = imageUrl, is_reusable = false }
                    }
                }
            };
            var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");

            // Retry logic for better reliability
            int retries = 3;
            while (retries > 0)
            {
                try
                {
                    var response = await _httpClient.PostAsync(url, content);
                    if (response.IsSuccessStatusCode)
                    {
                        break;
                    }
                }
                catch (Exception ex)
                {
                    // Log the exception
                    _logger.LogError($"Error sending image: {ex.Message}");
                }
                retries--;
                if (retries == 0)
                {
                    throw new Exception("Failed to send image after 3 attempts");
                }
            }
        }

        public async Task SendImageByNameAsync(string recipientId, string name)
        {
            var imageUrl = $"{ImageBaseUrl}/{name}.jpg";
            await SendImageAsync(recipientId, imageUrl);
        }
    }

}
