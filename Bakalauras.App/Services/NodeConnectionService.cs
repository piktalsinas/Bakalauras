using Bakalauras.Domain.Models;
using Bakalauras.Persistence.Repositories;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Bakalauras.App.Services
{
    public class NodeConnectionService
    {
        private readonly INodeConnectionRepository _nodeConnectionRepository;
        private readonly INodeRepository _nodeRepository;
        private readonly string _baseImageFolder;
        private readonly string _pathFolder;
        private readonly ILogger<NodeConnectionService> _logger;

        public NodeConnectionService(INodeConnectionRepository nodeConnectionRepository, INodeRepository nodeRepository, ILogger<NodeConnectionService> logger)
        {
            _nodeConnectionRepository = nodeConnectionRepository;
            _nodeRepository = nodeRepository;
            _logger = logger;

            _baseImageFolder = Path.Combine(Directory.GetCurrentDirectory(), "C:\\Users\\picom\\Documents\\BAKALAURAS\\ConnectionPhotos");
            _pathFolder = Path.Combine(Directory.GetCurrentDirectory(), "C:\\Users\\picom\\Documents\\BAKALAURAS\\ConnectionPath");
        }

        public async Task<bool> MoveImageToPathFolderAsync(Guid nodeConnectionId)
        {
            var nodeConnection = await _nodeConnectionRepository.GetByIdAsync(nodeConnectionId);
            if (nodeConnection == null)
            {
                return false; // NodeConnection not found
            }

            // Get the names of the nodes connected by this connection
            var fromNode = await _nodeRepository.GetByIdAsync(nodeConnection.FromNodeId);
            var toNode = await _nodeRepository.GetByIdAsync(nodeConnection.ToNodeId);

            if (fromNode == null || toNode == null)
            {
                return false; // One of the connected nodes is not found
            }

            try
            {
                if (!Directory.Exists(_pathFolder))
                {
                    Directory.CreateDirectory(_pathFolder);
                }

                // Create the image name from the node connection
                var connectionName = $"{fromNode.Name}_{toNode.Name}";
                var sourceImagePath = Path.Combine(_baseImageFolder, $"{connectionName}.jpg");
                var destinationImagePath = Path.Combine(_pathFolder, $"{connectionName}.jpg");

                if (File.Exists(sourceImagePath))
                {
                    File.Copy(sourceImagePath, destinationImagePath, overwrite: true);
                    _logger.LogInformation("Image file copied successfully from {Source} to {Destination}.", sourceImagePath, destinationImagePath);
                    return true;
                }

                return false; // Source file does not exist
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while moving image for NodeConnection {NodeConnectionId}.", nodeConnectionId);
                return false; // An error occurred
            }
        }
    }
}
