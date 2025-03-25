using Bakalauras.Domain.Models;

namespace Bakalauras.Persistence.Repositories;

public interface INodeRepository
{
    Task<Node?> GetByIdAsync(Guid id);
    Task<IEnumerable<Node>> GetByNameAsync(string name);  // Updated to return IEnumerable<Node>
    Task<Node> AddAsync(Node node);
    Task<Node> UpdateAsync(Node node);
    Task DeleteAsync(Guid id);
    Task<IEnumerable<Node>> GetAllAsync();
    Task<BaseNode?> GetParentByIdAsync(Guid parentId);
    Task<Node?> GetByNameAndParentAsync(string name, string parentName);

}
