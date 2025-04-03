using Bakalauras.Domain.Models;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Text;


namespace Bakalauras.App.Services
{
    public class FacebookMessageService
    {
        private readonly string _pageAccessToken;
        private readonly string _baseUrl;
        public FacebookMessageService(IOptions<AppSettings> appSettings)
        {
            _pageAccessToken = appSettings.Value.PageAccessToken;
            _baseUrl = appSettings.Value.BaseUrl;
        }
        private string ImageBaseUrl => $"{_baseUrl}/images";

        public async Task SendTextAsync(string recipientId, string text, dynamic quickReplies = null)
        {
            var url = $"https://graph.facebook.com/v10.0/me/messages?access_token={_pageAccessToken}";
            var payload = new
            {
                recipient = new { id = recipientId },
                message = new
                {
                    text,
                    quick_replies = quickReplies // Include quick replies if provided
                }
            };
            var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");
            using var httpClient = new HttpClient();
            await httpClient.PostAsync(url, content);
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
            using var httpClient = new HttpClient();
            await httpClient.PostAsync(url, content);
        }

        public async Task SendImageByNameAsync(string recipientId, string name)
        {
            var imageUrl = $"{ImageBaseUrl}/{name}.jpg";
            await SendImageAsync(recipientId, imageUrl);
        }
    }
}
