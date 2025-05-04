using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Bakalauras.Domain.Models;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Bakalauras.App.Services
{
    public class WitAiService
    {
        private readonly string _accessToken;

        public WitAiService(IOptions<AppSettings> appSettings)
        {
            _accessToken = appSettings.Value.WitAiAccessToken;
        }

        public async Task<string> GetIntentAsync(string text)
        {
            var url = "https://api.wit.ai/message?v=20230408&q=" + Uri.EscapeDataString(text);

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_accessToken}");
                var response = await client.GetStringAsync(url);

                var witResponse = JsonConvert.DeserializeObject<WitResponse>(response);

                Console.WriteLine("Wit.ai Response: " + JsonConvert.SerializeObject(witResponse, Formatting.Indented));

                if (witResponse?.Entities is Dictionary<string, object> entities)
                {
                    if (entities.ContainsKey("path:path"))
                    {
                        return "prompt_path";
                    }
                    if (entities.ContainsKey("questions:questions"))
                    {
                        return "questions";
                    }
                    if (entities.ContainsKey("welcome:welcome"))
                    {
                        return "welcome";
                    }
                }
                

                return witResponse?.Text ?? "Sorry, I couldn't understand that.";
            }
        }

        public string GetTimetableInEnglish()
        {
            return "Here is the timetable for lecture times:\n" +
                   "*1st Lecture*: 8:30 - 10:05\n" +
                   "*2nd Lecture*: 10:20 - 11:55\n" +
                   "*3rd Lecture*: 12:10 - 13:45\n" +
                   "*Lunch break*: 13:45 - 14:30\n" +
                   "*4th Lecture*: 14:30 - 16:05\n" +
                   "*5th Lecture*: 16:20 - 17:55\n" +
                   "*6th Lecture*: 18:10 - 19:45\n" +
                   "*7th Lecture*: 19:55 - 21:30";

        }

        
        public string GetTimetableInLithuanian()
        {
            return "Štai paskaitų laikų tvarkaraštis:\n" +
                   "*1 paskaita*: 8:30 - 10:05\n" +
                   "*2 paskaita*: 10:20 - 11:55\n" +
                   "*3 paskaita*: 12:10 - 13:45\n" +
                   "*Pietų pertrauka*: 13:45 - 14:30\n" +
                   "*4 paskaita*: 14:30 - 16:05\n" +
                   "*5 paskaita*: 16:20 - 17:55\n" +
                   "*6 paskaita*: 18:10 - 19:45\n" +
                   "*7 paskaita*: 19:55 - 21:30";
        }


        public class WitResponse
        {
            [JsonProperty("text")]
            public string Text { get; set; }
            [JsonProperty("entities")]
            public Dictionary<string, object> Entities { get; set; }
        }
    }
}
