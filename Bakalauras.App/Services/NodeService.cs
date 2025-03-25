using Bakalauras.Domain.Models;
using Bakalauras.Persistence.Repositories;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Bakalauras.App.Services
{
    public class NodeService
    {
        private readonly INodeRepository _nodeRepository;
        private readonly string _baseImageFolder;
        private readonly string _PathFolder;
        private readonly ILogger<NodeService> _logger;

        public NodeService(INodeRepository nodeRepository, ILogger<NodeService> logger)
        {
            _nodeRepository = nodeRepository;
            _logger = logger;  // Initialize logger

            _baseImageFolder = Path.Combine(Directory.GetCurrentDirectory(), "C:\\home\\Nodes");
            _PathFolder = Path.Combine(Directory.GetCurrentDirectory(), "C:\\home\\NodesPath");
        }

        public async Task AddNodesFromImagesAsync(string folderPath)
        {
            if (!Directory.Exists(folderPath))
            {
                throw new DirectoryNotFoundException($"The directory {folderPath} does not exist.");
            }

            var files = Directory.GetFiles(folderPath, "*.*", SearchOption.TopDirectoryOnly)
                .Where(file => file.EndsWith(".jpg") || file.EndsWith(".png") || file.EndsWith(".jpeg"));

            foreach (var file in files)
            {
                var nodeName = Path.GetFileNameWithoutExtension(file);

                // Get the first existing node with the given name
               // var existingNode = (await _nodeRepository.GetByNameAsync(nodeName)).FirstOrDefault();
               var existingNode = (await _nodeRepository.GetByNameAsync(nodeName)).FirstOrDefault();

                if (existingNode == null)
                {
                    Node node = new Node { Name = nodeName };
                    var addedNode = await _nodeRepository.AddAsync(node);

                    if (addedNode != null)
                    {
                        _logger.LogInformation("Added Node - Id: {NodeId}, Name: {NodeName}", addedNode.Id, addedNode.Name);
                    }
                }
            }
        }

        public async Task<bool> MoveImageToPathFolderAsync(Guid nodeId)
        {
            var node = await _nodeRepository.GetByIdAsync(nodeId);
            if (node == null)
            {
                return false; // Node not found
            }

            try
            {
                if (!Directory.Exists(_PathFolder))
                {
                    Directory.CreateDirectory(_PathFolder);
                }

                var sourceImagePath = Path.Combine(_baseImageFolder, node.Name);
                var destinationImagePath = Path.Combine(_PathFolder, node.Name);

                if (File.Exists(sourceImagePath))
                {
                    File.Move(sourceImagePath, destinationImagePath);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while moving image for Node ID {NodeId}.", nodeId);
                return false;
            }
        }
    }
}
