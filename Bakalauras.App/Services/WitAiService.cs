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
                }

                return witResponse?.Text ?? "Sorry, I couldn't understand that.";
            }
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
