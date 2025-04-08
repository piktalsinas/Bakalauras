using Bakalauras.Domain.Models;
using Bakalauras.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
namespace Bakalauras.App.Services
{
    public partial class BaseNodeService
    {
        private readonly IBaseNodeRepository _baseNodeRepository;
        private readonly ILogger<BaseNodeService> _logger;

        public BaseNodeService(IBaseNodeRepository baseNodeRepository, ILogger<BaseNodeService> logger)
        {
            _baseNodeRepository = baseNodeRepository;
            _logger = logger;
        }
       


        public async Task<BaseNode?> AddBaseNodeAsync(string name)
        {
            var existingBaseNode = await _baseNodeRepository.GetByNameAsync(name);
            if (existingBaseNode != null)
            {
                return null; // BaseNode with the same name already exists
            }

            BaseNode baseNode = new() { Name = name };
            return await _baseNodeRepository.AddAsync(baseNode);
        }
        public async Task AddBaseNodesFromImagesAsync(string folderPath)
        {
            if (!Directory.Exists(folderPath))
            {
                throw new DirectoryNotFoundException($"The directory {folderPath} does not exist.");
            }

            var files = Directory.GetFiles(folderPath, "*.*", SearchOption.TopDirectoryOnly)
                .Where(file => file.EndsWith(".jpg") || file.EndsWith(".png") || file.EndsWith(".jpeg"));

            foreach (var file in files)
            {
                var baseNodeName = Path.GetFileNameWithoutExtension(file);

                var existingBaseNode = await _baseNodeRepository.GetByNameAsync(baseNodeName);
                if (existingBaseNode == null)
                {
                    BaseNode baseNode = new BaseNode { Name = baseNodeName };
                    var addedBaseNode = await _baseNodeRepository.AddAsync(baseNode);

                    if (addedBaseNode != null)
                    {
                        _logger.LogInformation("Added BaseNode - Id: {BaseNodeId}, Name: {BaseNodeName}", addedBaseNode.Id, addedBaseNode.Name);
                    }
                }
            }
        }

    }
}
