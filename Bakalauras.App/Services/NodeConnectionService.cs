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
                _logger.LogWarning("NodeConnection with ID {NodeConnectionId} not found.", nodeConnectionId);
                return false; // NodeConnection not found
            }

            var fromNode = await _nodeRepository.GetByIdAsync(nodeConnection.FromNodeId);
            var toNode = await _nodeRepository.GetByIdAsync(nodeConnection.ToNodeId);

            if (fromNode == null || toNode == null)
            {
                _logger.LogWarning("One or both nodes not found for NodeConnection {NodeConnectionId}. FromNodeId: {FromNodeId}, ToNodeId: {ToNodeId}",
                                    nodeConnectionId, nodeConnection.FromNodeId, nodeConnection.ToNodeId);
                return false; // One of the connected nodes is not found
            }

            try
            {
                if (!Directory.Exists(_pathFolder))
                {
                    Directory.CreateDirectory(_pathFolder);
                    _logger.LogInformation("Created directory: {PathFolder}", _pathFolder);
                }

                // Modify the logic to use ParentName and Node.Name for the connectionName
                var fromParentName = fromNode.ParentName ?? "NoParent";
                var toParentName = toNode.ParentName ?? "NoParent";
                var connectionName = $"{fromParentName}_{fromNode.Name}_{toParentName}_{toNode.Name}";

                var sourceImagePath = Path.Combine(_baseImageFolder, $"{connectionName}.jpg");
                var destinationImagePath = Path.Combine(_pathFolder, $"{connectionName}.jpg");

                _logger.LogInformation("Source image path: {SourceImagePath}, Destination path: {DestinationImagePath}", sourceImagePath, destinationImagePath);

                if (File.Exists(sourceImagePath))
                {
                    File.Copy(sourceImagePath, destinationImagePath, overwrite: true);
                    _logger.LogInformation("Image file copied successfully from {Source} to {Destination}.", sourceImagePath, destinationImagePath);
                    return true;
                }

                _logger.LogWarning("Source image file does not exist: {SourceImagePath}", sourceImagePath);
                return false; // Source file does not exist
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while moving image for NodeConnection {NodeConnectionId}.", nodeConnectionId);
                return false; // An error occurred
            }
        }

        public async Task<bool> CopyPathImagesAsync(List<Node> path)
        {
            if (path == null || path.Count < 2)
            {
                _logger.LogWarning("Path must contain at least two nodes to copy images.");
                return false;
            }

            try
            {
                if (!Directory.Exists(_pathFolder))
                {
                    Directory.CreateDirectory(_pathFolder);
                    _logger.LogInformation("Created path directory: {PathFolder}", _pathFolder);
                }

                for (int i = 0; i < path.Count - 1; i++) // Ensure first connection is included
                {
                    var fromNode = path[i];
                    var toNode = path[i + 1];

                    var fromParentName = fromNode.ParentName ?? "NoParent";
                    var toParentName = toNode.ParentName ?? "NoParent";
                    var connectionName = $"{fromParentName}_{fromNode.Name}_{toParentName}_{toNode.Name}";

                    var sourceImagePath = Path.Combine(_baseImageFolder, $"{connectionName}.jpg");
                    var destinationImagePath = Path.Combine(_pathFolder, $"{connectionName}.jpg");

                    if (File.Exists(sourceImagePath))
                    {
                        File.Copy(sourceImagePath, destinationImagePath, overwrite: true);
                        _logger.LogInformation("Copied image from {Source} to {Destination}.", sourceImagePath, destinationImagePath);
                    }
                    else
                    {
                        _logger.LogWarning("Image not found for connection: {ConnectionName}", connectionName);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while copying shortest path images.");
                return false;
            }
        }


    }
}
