using Bakalauras.Domain.Models;
using Bakalauras.Persistence.Repositories;
using Microsoft.Extensions.Logging;

public class BaseNodeService
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
}
