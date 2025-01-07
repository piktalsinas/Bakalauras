using Bakalauras.Domain.Models;

namespace Bakalauras.Persistence.Repositories;

public interface INodeRepository
{
	Task<IEnumerable<Node>> GetAllAsync();
	Task<Node?> GetByIdAsync(Guid id);
    Task<Node?> GetByNameAsync(string name);
    Task<Node?> AddAsync(Node node);
	Task<Node?> UpdateAsync(Node node);
	Task DeleteAsync(Guid id);
}
