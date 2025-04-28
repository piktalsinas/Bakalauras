using Newtonsoft.Json;
using System.Text;
using Microsoft.Extensions.Logging;
using Bakalauras.Domain.Models;
using Microsoft.Extensions.Options;
using Bakalauras.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Bakalauras.App.Services
{
    public class LanguageService
    {
        private readonly ILogger<LanguageService> _logger;
        private readonly string _pageAccessToken;
        private readonly string _baseUrl;
        private readonly AppDbContext _dbContext;
        public LanguageService(ILogger<LanguageService> logger,IOptions<AppSettings> appSettings, AppDbContext dbContext)
        {
            _logger = logger;
            _pageAccessToken = appSettings.Value.PageAccessToken;
            _baseUrl = appSettings.Value.BaseUrl;
            _dbContext = dbContext;
        }
        private static readonly Dictionary<string, string> _userLanguages = new();

        public void SetUserLanguage(string userId, string language) => _userLanguages[userId] = language;



        public async Task<(string text, dynamic quickReplies)> TranslateAsync(string key, string userId)
        {
            var lang = _userLanguages.ContainsKey(userId) ? _userLanguages[userId] : "en";


            var translations = new Dictionary<string, (string en, string lt, dynamic quickReplies)>
            {
                ["info"] = (
                    "👋 Welcome! I'm your navigation assistant at Vilnius Tech University.\n\n" +
                    "My main purpose is to:\n" +
                    "• Help students find *the shortest path* to their desired auditorium\n" +
                    "• View a photo of your *desired building*\n" +
                    "• *List rooms by building*\n" +
                    "• Get all lectures *timetable*\n\n" +
                    "Feel free to use these features to make your campus navigation easier!",
                    "👋 Sveiki! Aš esu jūsų navigacijos asistentas Vilnius Tech universitete.\n\n" +
                    "Mano pagrindinė paskirtis:\n" +
                    "• Padėti studentams rasti *trumpiausią kelią* iki norimos auditorijos\n" +
                    "• Peržiūrėti norimo *pastato nuotrauką*\n" +
                    "• Gauti *auditorijų sąrašą pagal pastatą*\n" +
                    "• Gauti visų paskaitų *tvarkaraštį*\n\n" +
                    "Drąsiai naudokitės šiais įrankiais, kad lengviau orientuotumėtės universiteto teritorijoje!",
                    null
                ),
                ["prompt_building"] = (
                    "Please enter the building name to see its rooms *(e.g., S1, S2)*.",
                    "Įveskite pastato pavadinimą, kad pamatytumėte jo auditorijas *(pvz., S1, S2)*.",
                    null
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
                    "Please type the path in format:\n\nBuilding1_Room1 to Building2_Room2 (e.g., *S5_227 to S5_435*)\n *or*\nBuilding1_Entrance to Building2_Room2 (e.g. *S2_Start to S6_427*)",
                    "Prašome įvesti kelią tokiu formatu:\n\nPastatas1_Auditorija1 iki Pastatas2_Auditorija2 (pvz *S5_227 iki S5_435*)\n *arba*\nPastatas1_Įėjimas iki Pastatas2_Auditorija2  (e.g. *S2_Start iki S6_427*)",
                    null
                ),
                ["welcome"] = (
                    "Hello! I'd be happy to assist you :)\n For more information, feel free to use *the menu on the right side*.",
                    "Sveiki! Norėčiau jums padėti :)\n Daugiau informacijos rasite naudodamiesi *meniu dešinėje pusėje*.",
                    null
                )
            };

            if (key == "nodes_list")
            {
                var baseNodes = await _dbContext.BaseNodes.Select(b => b.Name).Distinct().ToListAsync();

                var quickReplies = baseNodes.Select(name => new
                {
                    content_type = "text",
                    title = name,
                    payload = name
                }).ToArray();

                var text = lang == "lt"
                    ? "Štai norimas pastatas ir jo auditorijos:"
                    : "Here is a picture of building and it's rooms:";

                return (text, quickReplies);
            }

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
            new { type = "postback", title = "Pastatas ir Auditorijos 🏫", payload = "GET_ROOMS" },
            new { type = "postback", title = "Tvarkaraštis 🕒", payload = "GET_TIMETABLE" },
            new { type = "postback", title = "Pakeisti į EN", payload = "LANGUAGE_EN" }
                }
                : new[]
                {
            new { type = "postback", title = "INFO", payload = "INFO" },
            new { type = "postback", title = "Find Path 📍", payload = "FIND_PATH" },
            new { type = "postback", title = "Building and Rooms 🏫", payload = "GET_ROOMS" },
            new { type = "postback", title = "Timetable 🕒", payload = "GET_TIMETABLE" },
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
