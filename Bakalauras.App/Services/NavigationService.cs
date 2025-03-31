using Newtonsoft.Json;
using Bakalauras.Domain.Models;
using Bakalauras.Persistence.Repositories;

namespace Bakalauras.App.Services
{
    public class NavigationService
    {
        private readonly INodeRepository _nodeRepository;
        private readonly INodeNameSevice _nodeNameService;
        private readonly DijkstraService _dijkstraService;
        private readonly NodeConnectionService _nodeConnectionService;
        private readonly FacebookMessageService _messageService;
        private readonly LanguageService _languageService;
        private readonly string _baseUrl = "https://98ec-78-57-195-217.ngrok-free.app";
        private string ImageBaseUrl => $"{_baseUrl}/images";

        public NavigationService(
            INodeRepository nodeRepository,
            INodeNameSevice nodeNameService,
            DijkstraService dijkstraService,
            NodeConnectionService nodeConnectionService,
            FacebookMessageService messageService,
            LanguageService languageService)
        {
            _nodeRepository = nodeRepository;
            _nodeNameService = nodeNameService;
            _dijkstraService = dijkstraService;
            _nodeConnectionService = nodeConnectionService;
            _messageService = messageService;
            _languageService = languageService;
        }

        public async Task SendAllNodesAsync(string recipientId)
        {
            var nodes = await GetAllNodesFromApi();
            var message = _languageService.Translate("nodes_list", recipientId) + string.Join(", ", nodes.Select(n => n.Name));
            await _messageService.SendTextAsync(recipientId, message);
        }

        public async Task SendNodesByBaseAsync(string recipientId, string messageText)
        {
            var baseNodeName = messageText.Substring("get nodes by base node".Length).Trim();
            var nodes = await _nodeRepository.GetNodesByBaseNodeAsync(baseNodeName);
            if (!nodes.Any())
            {
                await _messageService.SendTextAsync(recipientId, _languageService.Translate("no_nodes", recipientId));
                return;
            }
            var message = _languageService.Translate("nodes_list", recipientId) + string.Join(", ", nodes.Select(n => n.Name));
            await _messageService.SendTextAsync(recipientId, message);
        }

        public async Task SendShortestPathAsync(string recipientId, string messageText)
        {
            // Expecting input like "S1_Exit to S1_Room3"
            var parts = messageText.Split("to", StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2)
            {
                await _messageService.SendTextAsync(recipientId, _languageService.Translate("nodes_not_found", recipientId));
                return;
            }

            var from = parts[0].Trim();
            var to = parts[1].Trim();

            var startNodeId = await _nodeNameService.GetNodeIdByFullName(from);
            var endNodeId = await _nodeNameService.GetNodeIdByFullName(to);

            if (startNodeId == null || endNodeId == null)
            {
                await _messageService.SendTextAsync(recipientId, _languageService.Translate("nodes_not_found", recipientId));
                return;
            }

            var path = await _dijkstraService.FindShortestPathAsync(startNodeId.Value, endNodeId.Value);
            if (path == null || path.Count < 2)
            {
                await _messageService.SendTextAsync(recipientId, _languageService.Translate("no_path_found", recipientId));
                return;
            }

            await _nodeConnectionService.CopyPathImagesAsync(path);

            var intro = string.Format(_languageService.Translate("path_start", recipientId), from, to);
            await _messageService.SendTextAsync(recipientId, intro);

            for (int i = 0; i < path.Count - 1; i++)
            {
                var fromNode = path[i];
                var toNode = path[i + 1];
                var connectionName = $"{fromNode.ParentName ?? "NoParent"}_{fromNode.Name}_{toNode.ParentName ?? "NoParent"}_{toNode.Name}";
                var imageUrl = $"{ImageBaseUrl}/{connectionName}.jpg";
                await _messageService.SendImageAsync(recipientId, imageUrl);
            }
        }


        private async Task<IEnumerable<Node>> GetAllNodesFromApi()
        {
            using var httpClient = new HttpClient();
            var response = await httpClient.GetAsync($"{_baseUrl}/node");
            if (!response.IsSuccessStatusCode) return new List<Node>();
            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<IEnumerable<Node>>(content);
        }
    }
}
