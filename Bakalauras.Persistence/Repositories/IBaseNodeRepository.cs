using Bakalauras.Domain.Models;

namespace Bakalauras.Persistence.Repositories;

public interface IBaseNodeRepository
{
    Task<BaseNode?> GetByIdAsync(Guid id);
    Task<BaseNode?> GetByNameAsync(string name);
    Task<BaseNode> AddAsync(BaseNode baseNode);
    Task<BaseNode> UpdateAsync(BaseNode baseNode);
    Task DeleteAsync(Guid id);
    Task<IEnumerable<BaseNode>> GetAllAsync();
}