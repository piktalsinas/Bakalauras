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
        private readonly WitAiService _witAiService;

        public FacebookMessageService(IOptions<AppSettings> appSettings, ILogger<LanguageService> logger, WitAiService witAiService)
        {
            _pageAccessToken = appSettings.Value.PageAccessToken;
            _baseUrl = appSettings.Value.BaseUrl;
            _httpClient = new HttpClient();
            _logger = logger;
            _witAiService = witAiService;
        }

        private string ImageBaseUrl => $"{_baseUrl}/images";

        public async Task SendTextAsync(string recipientId, string text, dynamic quickReplies = null)
        {
            if (string.IsNullOrEmpty(text))
            {
                text = "Sorry, I didn't understand that. Could you try again?";
            }

            var url = $"https://graph.facebook.com/v10.0/me/messages?access_token={_pageAccessToken}";
            var payload = new
            {
                recipient = new { id = recipientId },
                message = new { text, quick_replies = quickReplies }
            };
            var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");

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

            int retries = 3;
            while (retries > 0)
            {
                try
                {
                    var response = await _httpClient.PostAsync(url, content);
                    var responseBody = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation($"Response: {responseBody}");

                    if (response.IsSuccessStatusCode)
                    {
                        break;
                    }
                    else
                    {
                        _logger.LogError($"Failed to send image: {responseBody}");
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



        public async Task SendClarificationMessageAsync(string recipientId, string language)
        {
            string messageText = language == "lt"
                ? "Ar galite perfrazuoti?"  
                : "Could you repeat?";      

            await SendTextAsync(recipientId, messageText);
        }
     /*   public async Task HandleUserInputAsync(string recipientId, string userInput, string language)
        {
            var intent = await _witAiService.GetIntentAsync(userInput); 

            if (intent == "welcome")
            {
                _logger.LogInformation("Welcome Intent Detected! Sending response...");

                var welcomeMessage = language == "en"
                    ? "Hello! I am your navigation chatbot, I would like to help you. You can use the menu on the right side for more information."
                    : "Sveiki! Aš esu jūsų navigacijos pokalbių robotas. Noriu jums padėti. Galite naudotis meniu dešinėje pusėje, kad gautumėte daugiau informacijos.";

                await SendTextAsync(recipientId, welcomeMessage);
            }
            else
            {
                // If it's not a recognized intent, proceed with translation and normal handling
                var response = ProcessUserInput(userInput);

                if (response == null)
                {
                    await SendClarificationMessageAsync(recipientId, language);
                }
                else
                {
                    await SendTextAsync(recipientId, response, null);
                }
            }
        }*/




        /*private string ProcessUserInput(string input)
        {
            if (input.Equals("INFO", StringComparison.OrdinalIgnoreCase))
            {
                return "Here is some information about the university.";
            }
            else if (input.Equals("FIND_PATH", StringComparison.OrdinalIgnoreCase))
            {
                return "Please provide the starting and ending points to find a path.";
            }
            else if (input.Equals("GET_ROOMS", StringComparison.OrdinalIgnoreCase))
            {
                return "Please provide the building name to get a list of rooms.";
            }

            return null;
        }*/


        public async Task SendImageByNameAsync(string recipientId, string name)
        {
            var imageUrl = $"{ImageBaseUrl}/{name}.jpg";
            await SendImageAsync(recipientId, imageUrl);
        }
    }

}
