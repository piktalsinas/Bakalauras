using Newtonsoft.Json;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Bakalauras.App.Services
{
    public class LanguageService
    {
        private readonly ILogger<LanguageService> _logger;
        private readonly string _pageAccessToken = "EAAM60zgkJVoBOxQgRZBe6qsdrsatspnVzAtC2qZATUJt4JHnoPtcErgixfSwHKHgTH2Mv0Gf0E7UmJTJTpO3TyZBoaClcgMeYq6omT4uYU8IX8ZCrUKrZB4kyT9qpZBA3OhRi8xOZCIJhHZCdHv44lo9Bp3J55ALOLHdgjDpqzmHdh6QK5trmvZB2aZCacAtbJvqd1owZDZD";
        private readonly string _baseUrl = "https://98ec-78-57-195-217.ngrok-free.app";
        private static readonly Dictionary<string, string> _userLanguages = new();

        public LanguageService(ILogger<LanguageService> logger)
        {
            _logger = logger;
        }

        public void SetUserLanguage(string userId, string language) => _userLanguages[userId] = language;

        public string Translate(string key, string userId)
        {
            var lang = _userLanguages.ContainsKey(userId) ? _userLanguages[userId] : "en";

            var translations = new Dictionary<string, (string en, string lt)>
            {
                ["welcome"] = (
                    "👋 Welcome back! I'm your navigation assistant.\n\n" +
                    "• Type: get rooms – list all auditoriums\n" +
                    "• Type: shortest path from Building1 Room1 to Building2 Room2 – find a route\n" +
                    "• Type: Building_Room – get a photo",
                    "👋 Sveiki sugrįžę! Aš esu jūsų navigacijos asistentas.\n\n" +
                    "• Įveskite: gauti auditorijas – rodyti visas auditorijas\n" +
                    "• Įveskite: trumpiausias kelias nuo Pastatas1 Auditorija1 iki Pastatas2 Auditorija2 – rasti maršrutą\n" +
                    "• Įveskite: Pastatas_Auditorija – gauti nuotrauką"
                ),
                ["prompt_building"] = (
                    "Please enter the building name to see its rooms (e.g., S1, S2).",
                    "Įveskite pastato pavadinimą, kad pamatytumėte jo auditorijas (pvz., S1, S2)."
                ),
                ["nodes_list"] = (
                    "Here are your rooms:",
                    "Štai jūsų auditorijos:"
                ),
                ["no_nodes"] = (
                    "No rooms found.",
                    "Auditorijų nerasta."
                ),
                ["nodes_not_found"] = (
                    "One or both rooms were not found.",
                    "Viena arba abi auditorijos nerastos."
                ),
                ["no_path_found"] = (
                    "No path found between the specified rooms.",
                    "Kelias tarp nurodytų auditorijų nerastas."
                ),
                ["path_start"] = (
                    "{0} to {1}:",
                    "{0} iki {1}:"
                ),
                ["prompt_path"] = (
                    "Please type the path in format:\nshortest path from Building1 Room1 to Building2 Room2 (eg. S1_227 to S1_435)",
                    "Prašome įvesti kelią tokiu formatu:\ntrumpiausias kelias nuo Pastatas1 Auditorija1 iki Pastatas2 Auditorija2 (pvz S1_227 iki S1_435)"
                ),

            };

            if (translations.TryGetValue(key, out var translation))
            {
                return lang == "lt" ? translation.lt : translation.en;
            }

            // Fallback
            return lang == "lt" ? "Nežinomas raktas vertimui." : "Unknown translation key.";
        }


        public async Task SetUserMenuAsync(string userId, string language)
        {
            var menuItems = language == "lt"
                ? new[]
                {
            new { type = "postback", title = "Paleisti iš naujo 🔁", payload = "RESTART_BOT" },
            new { type = "postback", title = "Rasti kelią 📍", payload = "FIND_PATH" },
            new { type = "postback", title = "Pakeisti į EN", payload = "LANGUAGE_EN" },
            new { type = "postback", title = "Gauti auditorijas 🏫", payload = "GET_ROOMS" }
                }
                : new[]
                {
            new { type = "postback", title = "Restart 🔁", payload = "RESTART_BOT" },
            new { type = "postback", title = "Find Path 📍", payload = "FIND_PATH" },
            new { type = "postback", title = "Switch to LT", payload = "LANGUAGE_LT" },
            new { type = "postback", title = "Rooms by Building 🏫", payload = "GET_ROOMS" }
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
