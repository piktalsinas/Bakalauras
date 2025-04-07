using Newtonsoft.Json;
using System.Text;
using Microsoft.Extensions.Logging;
using Bakalauras.Domain.Models;
using Microsoft.Extensions.Options;

namespace Bakalauras.App.Services
{
    public class LanguageService
    {
        private readonly ILogger<LanguageService> _logger;
        private readonly string _pageAccessToken;
        private readonly string _baseUrl;
        public LanguageService(ILogger<LanguageService> logger,IOptions<AppSettings> appSettings)
        {
            _logger = logger;
            _pageAccessToken = appSettings.Value.PageAccessToken;
            _baseUrl = appSettings.Value.BaseUrl;
        }
        private static readonly Dictionary<string, string> _userLanguages = new();

        public void SetUserLanguage(string userId, string language) => _userLanguages[userId] = language;

        public (string text, dynamic quickReplies) Translate(string key, string userId)
        {
            var lang = _userLanguages.ContainsKey(userId) ? _userLanguages[userId] : "en";

            var translations = new Dictionary<string, (string en, string lt, dynamic quickReplies)>
            {
                ["info"] = (
                    "👋 Welcome! I'm your navigation assistant at Vilnius Tech University.\n\n" +
                    "My main purpose is to:\n" +
                    "• Help students find the shortest path to their desired auditorium\n" +
                    "• View a photo of your desired auditorium\n" +
                    "• List rooms by building\n\n" +
                    "Feel free to use these features to make your campus navigation easier!",
                    "👋 Sveiki! Aš esu jūsų navigacijos asistentas Vilnius Tech universitete.\n\n" +
                    "Mano pagrindinė paskirtis:\n" +
                    "• Padėti studentams rasti trumpiausią kelią iki norimos auditorijos\n" +
                    "• Peržiūrėti norimos auditorijos nuotrauką\n" +
                    "• Gauti auditorijų sąrašą pagal pastatą\n\n" +
                    "Drąsiai naudokitės šiais įrankiais, kad lengviau orientuotumėtės universiteto teritorijoje!",
                    null
                ),
                ["prompt_building"] = (
                    "Please enter the building name to see its rooms (e.g., S1, S2).",
                    "Įveskite pastato pavadinimą, kad pamatytumėte jo auditorijas (pvz., S1, S2).",
                    null
                ),
                ["nodes_list"] = (
                    "Here are your rooms:",
                    "Štai jūsų auditorijos:",
                    new[]
                    {
                new { content_type = "text", title = "S1", payload = "S1" },
                new { content_type = "text", title = "S2", payload = "S2" },
                new { content_type = "text", title = "S3", payload = "S3" }
                    }
                ),
                ["no_nodes"] = (
                    "No rooms found.",
                    "Auditorijų nerasta.",
                    null
                ),
                ["nodes_not_found"] = (
                    "One or both rooms were not found.",
                    "Viena arba abi auditorijos nerastos.",
                    null
                ),
                ["no_path_found"] = (
                    "No path found between the specified rooms.",
                    "Kelias tarp nurodytų auditorijų nerastas.",
                    null
                ),
                ["path_start"] = (
                    "{0} to {1}:",
                    "{0} iki {1}:",
                    null
                ),
                ["prompt_path"] = (
                    "Please type the path in format:\nBuilding1 Room1 to Building2 Room2 (e.g., S1_227 to S1_435)",
                    "Prašome įvesti kelią tokiu formatu:\nPastatas1_Auditorija1 iki Pastatas2_Auditorija2 (pvz S1_227 iki S1_435)",
                    null
                ),
            };

            if (translations.TryGetValue(key, out var translation))
            {
                if (lang == "lt")
                    return (translation.lt, translation.quickReplies);
                else
                    return (translation.en, translation.quickReplies);
            }

            return lang == "lt" ? ("Nežinomas raktas vertimui.", null) : ("Unknown translation key.", null);
        }


        public string GetUserLanguage(string userId)
        {
            return _userLanguages.ContainsKey(userId) ? _userLanguages[userId] : "en";
        }


        public async Task SetUserMenuAsync(string userId, string language)
        {
            var menuItems = language == "lt"
                ? new[]
                {
            new { type = "postback", title = "INFO", payload = "INFO" },
            new { type = "postback", title = "Rasti kelią 📍", payload = "FIND_PATH" },
            new { type = "postback", title = "Gauti auditorijas 🏫", payload = "GET_ROOMS" },
            new { type = "postback", title = "Pakeisti į EN", payload = "LANGUAGE_EN" }
                }
                : new[]
                {
            new { type = "postback", title = "INFO", payload = "INFO" },
            new { type = "postback", title = "Find Path 📍", payload = "FIND_PATH" },
            new { type = "postback", title = "Rooms by Building 🏫", payload = "GET_ROOMS" },
            new { type = "postback", title = "Switch to LT", payload = "LANGUAGE_LT" }
                };

            var payload = new
            {
                psid = userId,
                persistent_menu = new[]
                {
            new
            {
                locale = "default",
                composer_input_disabled = false,
                call_to_actions = menuItems
            }
        }
            };

            var deleteUrl = $"https://graph.facebook.com/v17.0/me/custom_user_settings?psid={userId}&access_token={_pageAccessToken}";
            var postUrl = $"https://graph.facebook.com/v17.0/me/custom_user_settings?access_token={_pageAccessToken}";

            using var httpClient = new HttpClient();

            var deleteResponse = await httpClient.DeleteAsync(deleteUrl);
            var deleteBody = await deleteResponse.Content.ReadAsStringAsync();
            _logger.LogInformation($"Deleted menu for {userId}: {deleteResponse.StatusCode} - {deleteBody}");


            var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync(postUrl, content);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                _logger.LogError($"Failed to set menu for user {userId}: {response.StatusCode} {responseBody}");
            else
                _logger.LogInformation($"Successfully updated menu for {userId}: {response.StatusCode} {responseBody}");
        }

    }
}
