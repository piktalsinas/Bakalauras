using Bakalauras.App.Services;
using Bakalauras.Domain.Models;
using Bakalauras.Persistence.Repositories;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Bakalauras.API.Controllers
{
    [ApiController]
    [Route("api/webhook")]
    public class WebhookController : ControllerBase
    {
        private readonly ILogger<WebhookController> _logger;
        private readonly string _verifyToken = "navigacija123";
        private readonly INodeRepository _nodeRepository;
        private readonly NodeService _nodeService;
        private readonly DijkstraService _dijkstraService;
        private readonly INodeNameSevice _nodeNameService;
        private readonly NodeConnectionService _nodeConnectionService;
        private readonly string _pageAccessToken = "EAAM60zgkJVoBOxQgRZBe6qsdrsatspnVzAtC2qZATUJt4JHnoPtcErgixfSwHKHgTH2Mv0Gf0E7UmJTJTpO3TyZBoaClcgMeYq6omT4uYU8IX8ZCrUKrZB4kyT9qpZBA3OhRi8xOZCIJhHZCdHv44lo9Bp3J55ALOLHdgjDpqzmHdh6QK5trmvZB2aZCacAtbJvqd1owZDZD";
        private readonly string _baseUrl = "https://d828-78-57-195-217.ngrok-free.app";
        private string ImageBaseUrl => $"{_baseUrl}/api/images";

        public WebhookController(ILogger<WebhookController> logger, INodeRepository nodeRepository, NodeService nodeService, DijkstraService dijkstraService, INodeNameSevice nodeNameService, NodeConnectionService nodeConnectionService)
        {
            _logger = logger;
            _nodeRepository = nodeRepository;
            _nodeService = nodeService;
            _dijkstraService = dijkstraService;
            _nodeNameService = nodeNameService;
            _nodeConnectionService = nodeConnectionService;
        }

        [HttpGet]
        public IActionResult Get(
            [FromQuery(Name = "hub.mode")] string hubMode,
            [FromQuery(Name = "hub.verify_token")] string hubVerifyToken,
            [FromQuery(Name = "hub.challenge")] string hubChallenge)
        {
            if (hubMode == "subscribe" && hubVerifyToken == _verifyToken)
            {
                return Content(hubChallenge, "text/plain");
            }
            return Unauthorized();
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] object payload)
        {
            try
            {
                var jsonPayload = JsonConvert.DeserializeObject<FacebookWebhookPayload>(payload.ToString());
                var messageText = jsonPayload?.Entry?[0]?.Messaging?[0]?.Message?.Text;
                var senderId = jsonPayload?.Entry?[0]?.Messaging?[0]?.Sender?.Id;
                var postbackPayload = jsonPayload?.Entry?[0]?.Messaging?[0]?.Postback?.Payload;

                if (!string.IsNullOrEmpty(postbackPayload))
                {
                    switch (postbackPayload)
                    {
                        case "RESTART_BOT":
                            await SendTextToFacebook(senderId, "👋 Welcome back! I'm your navigation assistant.\n\n👋 Sveiki sugrįžę! Aš esu jūsų navigacijos asistentas.\n\n• Type / Įveskite: get nodes – list all nodes / rodyti visus taškus\n• Type / Įveskite: shortest path from A to B – find a route / rasti trumpiausią kelią\n• Type / Įveskite: Floor1_Entrance – get an image / gauti paveikslėlį\n\n👇 Or tap a menu item below / Arba spauskite meniu mygtuką žemiau");
                            break;
                        case "FIND_PATH":
                            await SendTextToFacebook(senderId, "Please type the path in format:\nshortest path from Node1 to Node2\n\nPrašome įvesti kelią tokiu formatu:\nshortest path from Node1 to Node2");
                            break;
                    }
                    return Ok("Postback handled");
                }

                if (!string.IsNullOrEmpty(messageText))
                {
                    var lowerText = messageText.ToLower();

                    if (lowerText == "get nodes")
                    {
                        var nodes = await GetAllNodesFromApi();
                        await SendMessageToFacebook(senderId, nodes);
                        return Ok("Message sent.");
                    }

                    if (lowerText.StartsWith("get nodes by base node"))
                    {
                        var baseNodeName = messageText.Substring("get nodes by base node".Length).Trim();
                        var nodes = await GetNodesByBaseNode(baseNodeName);
                        await SendMessageToFacebook(senderId, nodes.Any() ? nodes : new List<Node> { new Node { Name = "No nodes found / Nėra rastų taškų." } });
                        return Ok("Nodes sent.");
                    }

                    if (lowerText.StartsWith("shortest path from") && lowerText.Contains("to"))
                    {
                        var parts = messageText.Substring("shortest path from".Length).Split("to", StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length == 2)
                        {
                            var from = parts[0].Trim();
                            var to = parts[1].Trim();
                            await SendShortestPathWithImages(senderId, from, to);
                            return Ok("Path sent.");
                        }
                    }

                    if (Regex.IsMatch(messageText.Trim(), @"^[\w\-]+_[\w\-]+$", RegexOptions.IgnoreCase))
                    {
                        var imageFileName = messageText.Trim() + ".jpg";
                        var imageUrl = $"{ImageBaseUrl}/{imageFileName}";
                        await SendImageToFacebook(senderId, imageUrl);
                        return Ok("Image sent.");
                    }
                }

                return Ok("EVENT_RECEIVED");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error processing the payload: {ex.Message}");
                return StatusCode(500, "Internal server error.");
            }
        }

        private async Task<IEnumerable<Node>> GetNodesByBaseNode(string baseNodeName) => await _nodeRepository.GetNodesByBaseNodeAsync(baseNodeName);

        private async Task<IEnumerable<Node>> GetAllNodesFromApi()
        {
            var httpClient = new HttpClient();
            var response = await httpClient.GetAsync($"{_baseUrl}/node");
            if (!response.IsSuccessStatusCode) return new List<Node>();
            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<IEnumerable<Node>>(content);
        }

        private async Task SendMessageToFacebook(string recipientId, IEnumerable<Node> nodes)
        {
            var message = $"Here are your nodes / Štai jūsų taškai: {string.Join(", ", nodes.Select(n => n.Name))}";
            await SendTextToFacebook(recipientId, message);
        }

        private async Task SendTextToFacebook(string recipientId, string text)
        {
            var url = $"https://graph.facebook.com/v10.0/me/messages?access_token={_pageAccessToken}";
            var payload = new { recipient = new { id = recipientId }, message = new { text = text } };
            var httpClient = new HttpClient();
            var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");
            await httpClient.PostAsync(url, content);
        }

        private async Task SendImageToFacebook(string recipientId, string imageUrl)
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
            var httpClient = new HttpClient();
            var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");
            await httpClient.PostAsync(url, content);
        }

        private async Task SendShortestPathWithImages(string recipientId, string fullName1, string fullName2)
        {
            var startNodeId = await _nodeNameService.GetNodeIdByFullName(fullName1);
            var endNodeId = await _nodeNameService.GetNodeIdByFullName(fullName2);

            if (startNodeId == null || endNodeId == null)
            {
                await SendTextToFacebook(recipientId, "One or both nodes not found / Vienas arba abu taškai nerasti.");
                return;
            }

            var path = await _dijkstraService.FindShortestPathAsync(startNodeId.Value, endNodeId.Value);
            if (path == null || path.Count < 2)
            {
                await SendTextToFacebook(recipientId, "No path found between the specified nodes / Kelias tarp nurodytų taškų nerastas.");
                return;
            }

            await _nodeConnectionService.CopyPathImagesAsync(path);
            await SendTextToFacebook(recipientId, $"Shortest path from {fullName1} to {fullName2} / Trumpiausias kelias nuo {fullName1} iki {fullName2}:");

            for (int i = 0; i < path.Count - 1; i++)
            {
                var from = path[i];
                var to = path[i + 1];
                var fromParent = from.ParentName ?? "NoParent";
                var toParent = to.ParentName ?? "NoParent";
                var connectionName = $"{fromParent}_{from.Name}_{toParent}_{to.Name}";
                var imageUrl = $"{ImageBaseUrl}/{connectionName}.jpg";
                await SendImageToFacebook(recipientId, imageUrl);
            }
        }

        public class FacebookWebhookPayload
        {
            public string Object { get; set; }
            public List<FacebookEntry> Entry { get; set; }
        }
        public class FacebookEntry
        {
            public long Time { get; set; }
            public string Id { get; set; }
            public List<FacebookMessaging> Messaging { get; set; }
        }
        public class FacebookMessaging
        {
            public FacebookSender Sender { get; set; }
            public FacebookRecipient Recipient { get; set; }
            public long Timestamp { get; set; }
            public FacebookMessage Message { get; set; }
            public FacebookPostback Postback { get; set; }
        }
        public class FacebookSender { public string Id { get; set; } }
        public class FacebookRecipient { public string Id { get; set; } }
        public class FacebookMessage { public string Mid { get; set; } public string Text { get; set; } }
        public class FacebookPostback { public string Payload { get; set; } }
    }
}
