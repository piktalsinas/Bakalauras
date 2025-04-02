using Newtonsoft.Json;
using System.Text;


namespace Bakalauras.App.Services
{
    public class FacebookMessageService
    {
        private readonly string _pageAccessToken = "EAAM60zgkJVoBOxQgRZBe6qsdrsatspnVzAtC2qZATUJt4JHnoPtcErgixfSwHKHgTH2Mv0Gf0E7UmJTJTpO3TyZBoaClcgMeYq6omT4uYU8IX8ZCrUKrZB4kyT9qpZBA3OhRi8xOZCIJhHZCdHv44lo9Bp3J55ALOLHdgjDpqzmHdh6QK5trmvZB2aZCacAtbJvqd1owZDZD";
        private readonly string _baseUrl = "https://7bd8-78-57-195-217.ngrok-free.app";
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
