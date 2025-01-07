using Bakalauras.Domain.Models;
using Bakalauras.Persistence.Repositories;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Bakalauras.App.Services
{
    public class NodeService
    {
        private readonly INodeRepository _nodeRepository;
        private readonly string _baseImageFolder;
        private readonly string _answerPicFolder;

        public NodeService(INodeRepository nodeRepository)
        {
            _nodeRepository = nodeRepository;
            // Define the paths for the base image folder and AnswerPic folder
            _baseImageFolder = Path.Combine(Directory.GetCurrentDirectory(), "C:\\Users\\picom\\Documents\\BAKALAURAS\\photos");
            _answerPicFolder = Path.Combine(Directory.GetCurrentDirectory(), "C:\\Users\\picom\\Documents\\BAKALAURAS\\AnswerPic");
        }

        // Method to add nodes from images in a folder
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
                var nodeName = Path.GetFileNameWithoutExtension(file); // Get the image name without extension

                // Check if the node already exists by name
                var existingNode = await _nodeRepository.GetByNameAsync(nodeName);
                if (existingNode == null)
                {
                    Node node = new Node { Name = nodeName };
                    await _nodeRepository.AddAsync(node);
                }
            }
        }

        // Method to move image to the AnswerPic folder
        public async Task<bool> MoveImageToAnswerPicFolderAsync(Guid nodeId)
        {
            var node = await _nodeRepository.GetByIdAsync(nodeId);
            if (node == null)
            {
                return false; // Node not found
            }

            try
            {
                // Ensure the AnswerPic folder exists
                if (!Directory.Exists(_answerPicFolder))
                {
                    Directory.CreateDirectory(_answerPicFolder);
                }

                // Construct the source and destination paths
                var sourceImagePath = Path.Combine(_baseImageFolder, node.Name);
                var destinationImagePath = Path.Combine(_answerPicFolder, node.Name);

                // Check if the file exists
                if (File.Exists(sourceImagePath))
                {
                    // Move the file to the AnswerPic folder
                    File.Move(sourceImagePath, destinationImagePath);
                    return true;
                }

                return false; // Image not found
            }
            catch (Exception ex)
            {
                // Log exception if needed
                return false; // Failure to move image
            }
        }
    }
}
