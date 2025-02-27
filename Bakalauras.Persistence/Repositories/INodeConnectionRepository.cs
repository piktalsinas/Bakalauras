using Bakalauras.Domain.Models;

namespace Bakalauras.Persistence.Repositories;

public interface INodeConnectionRepository
{
	Task<IEnumerable<NodeConnection>> GetAllAsync();
	Task<IEnumerable<NodeConnection>> GetAllConnectionsByNodeIdAsync(Guid id);
	Task<NodeConnection?> GetByIdAsync(Guid id);
	Task<NodeConnection?> AddAsync(NodeConnection nodeConnection);
	Task<NodeConnection?> UpdateAsync(NodeConnection nodeConnection);
	Task DeleteAsync(Guid id);
}
