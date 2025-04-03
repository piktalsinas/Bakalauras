using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;
using Bakalauras.Domain.Models.Facebook;



namespace Bakalauras.App.Services
{
    public class FacebookPayloadHandler
    {
        private readonly FacebookMessageService _messageService;
        private readonly LanguageService _languageService;
        private readonly NavigationService _navigationService;
        private readonly ILogger<FacebookPayloadHandler> _logger;
        private readonly WitAiService _witAiService;

        public FacebookPayloadHandler(
            FacebookMessageService messageService,
            LanguageService languageService,
            NavigationService navigationService,
            WitAiService witAiService,
            ILogger<FacebookPayloadHandler> logger)
        {
            _messageService = messageService;
            _languageService = languageService;
            _navigationService = navigationService;
            _witAiService = witAiService;
            _logger = logger;
        }

        public async Task HandlePayloadAsync(FacebookWebhookPayload payload)
        {
            var messaging = payload?.Entry?.FirstOrDefault()?.Messaging?.FirstOrDefault();
            if (messaging == null) return;

            var senderId = messaging.Sender?.Id;
            var text = messaging.Message?.Text;
            var postback = messaging.Postback?.Payload;

            if (!string.IsNullOrEmpty(postback))
            {
                await HandlePostbackAsync(senderId, postback);
                return;
            }

            if (!string.IsNullOrEmpty(text))
            {
                await HandleMessageAsync(senderId, text);
            }
        }

        private async Task HandlePostbackAsync(string senderId, string payload)
        {
            switch (payload)
            {
                case "LANGUAGE_EN":
                    _languageService.SetUserLanguage(senderId, "en");
                    await _languageService.SetUserMenuAsync(senderId, "en");
                    await _messageService.SendTextAsync(senderId, "Language set to English ✅", null);
                    break;
                case "LANGUAGE_LT":
                    _languageService.SetUserLanguage(senderId, "lt");
                    await _languageService.SetUserMenuAsync(senderId, "lt");
                    await _messageService.SendTextAsync(senderId, "Kalba nustatyta į Lietuvių ✅", null);
                    break;
                case "INFO":
                    var infoText = _languageService.Translate("info", senderId).text;
                    await _messageService.SendTextAsync(senderId, infoText, null);
                    break;
                case "FIND_PATH":
                    var (promptText, quickReplies) = _languageService.Translate("prompt_path", senderId);
                    await _messageService.SendTextAsync(senderId, promptText, null); 
                    break;
                case "GET_ROOMS":
                    var buildingPrompt = _languageService.Translate("prompt_building", senderId).text;
                    var (roomsPrompt, quickRepliesForRooms) = _languageService.Translate("nodes_list", senderId); 
                    await _messageService.SendTextAsync(senderId, buildingPrompt, quickRepliesForRooms);  
                    break;
            }
        }




        private async Task HandleMessageAsync(string senderId, string text)
        {
            var lowerText = text.ToLower();

            var nlpResponse = await _witAiService.GetIntentAsync(text);

            if (nlpResponse == "prompt_path")
            {
                Console.WriteLine("Prompt Path Detected! Sending response...");
                var (promptText, quickReplies) = _languageService.Translate("prompt_path", senderId);
                await _messageService.SendTextAsync(senderId, promptText, null);
                return;
            }

            if (Regex.IsMatch(lowerText, @"^s\d+$"))
            {
                await _navigationService.SendNodesByBaseAsync(senderId, text.Trim());
            }

            if (Regex.IsMatch(text, @"^\s*[\w\-]+(?:_[\w\-]+)?\s+to\s+[\w\-]+(?:_[\w\-]+)?\s*$", RegexOptions.IgnoreCase))
            {
                await _navigationService.SendShortestPathAsync(senderId, text);
                return;
            }

            if (Regex.IsMatch(text.Trim(), @"^[\w\-]+_[\w\-]+$", RegexOptions.IgnoreCase))
            {
                await _messageService.SendImageByNameAsync(senderId, text.Trim());
            }
        }
    }
}
