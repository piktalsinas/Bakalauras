using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;
using Bakalauras.Domain.Models.Facebook;



namespace Bakalauras.App.Services
{
    public class FacebookPayloadHandler : IFacebookPayloadHandler
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
            if (payload?.Entry == null || payload.Entry.Count == 0)
            {
                _logger.LogWarning("Received empty payload or no entries.");
                return;
            }

            var messaging = payload.Entry.FirstOrDefault()?.Messaging?.FirstOrDefault();
            if (messaging == null)
            {
                _logger.LogWarning("No messaging found in the payload.");
                return;
            }

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
                    var (infoText, _) = await _languageService.TranslateAsync("info", senderId);
                    await _messageService.SendTextAsync(senderId, infoText, null);
                    break;

                case "FIND_PATH":
                    var (promptText, quickReplies) = await _languageService.TranslateAsync("prompt_path", senderId);
                    await _messageService.SendTextAsync(senderId, promptText, null);
                    break;

                case "GET_ROOMS":
                    var (buildingPrompt, _) = await _languageService.TranslateAsync("prompt_building", senderId);
                    (string roomsPrompt, dynamic quickRepliesForRooms) = await _languageService.TranslateAsync("nodes_list", senderId);
                    await _messageService.SendTextAsync(senderId, buildingPrompt, quickRepliesForRooms);
                    break;

                case "GET_TIMETABLE":
                    var userLanguage = _languageService.GetUserLanguage(senderId);
                    string timetable = userLanguage == "en"
                        ? _witAiService.GetTimetableInEnglish()
                        : _witAiService.GetTimetableInLithuanian();
                    await _messageService.SendTextAsync(senderId, timetable, null);
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
                var (promptText, quickReplies) = await _languageService.TranslateAsync("prompt_path", senderId);
                await _messageService.SendTextAsync(senderId, promptText, null);
                return;
            }
            if (nlpResponse == "welcome")
            {
                Console.WriteLine("Welcome Detected! Sending response...");
                var (promptText, quickReplies) = await _languageService.TranslateAsync("welcome", senderId);
                await _messageService.SendTextAsync(senderId, promptText, null);
                return;
            }

            if (nlpResponse == "questions")
            {
                Console.WriteLine("Questions Detected! Sending timetable...");

                var userLanguage = _languageService.GetUserLanguage(senderId);

                string timetable = string.Empty;
                if (userLanguage == "en")
                {
                    timetable = _witAiService.GetTimetableInEnglish();
                }
                else if (userLanguage == "lt")
                {
                    timetable = _witAiService.GetTimetableInLithuanian();
                }
                await _messageService.SendTextAsync(senderId, timetable, null);
                return;
            }
            if (nlpResponse == "welcome")
            {
                Console.WriteLine("Welcome Detected! Sending response...");
                var userLanguage = _languageService.GetUserLanguage(senderId);

                var (welcomeText, quickReplies) = await _languageService.TranslateAsync("welcome", senderId);

                if (string.IsNullOrEmpty(welcomeText))
                {
                    welcomeText = userLanguage == "en"
                        ? "Hello! I am your navigation chatbot, I would like to help you. You can use the menu on the right side for more information."
                        : "Sveiki! Aš esu jūsų navigacijos pokalbių robotas. Noriu jums padėti. Galite naudotis meniu dešinėje pusėje, kad gautumėte daugiau informacijos.";
                }

                await _messageService.SendTextAsync(senderId, welcomeText);
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

            if (Regex.IsMatch(text, @"^\s*[\w\-]+(?:_[\w\-]+)?\s+iki\s+[\w\-]+(?:_[\w\-]+)?\s*$", RegexOptions.IgnoreCase))
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

    public interface IFacebookPayloadHandler
    {
        Task HandlePayloadAsync(FacebookWebhookPayload payload);
    }


}
