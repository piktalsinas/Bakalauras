using Bakalauras.Domain.Models;
using Bakalauras.Persistence.Repositories;
using Bakalauras.Persistence;
using Microsoft.EntityFrameworkCore;

public class BaseNodeRepository : IBaseNodeRepository
{
    private readonly AppDbContext _context;

    public BaseNodeRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<BaseNode?> GetByIdAsync(Guid id)
    {
        return await _context.BaseNodes.FindAsync(id);
    }

    public async Task<BaseNode?> GetByNameAsync(string name)
    {
        return await _context.BaseNodes
            .Where(n => n.Name == name)
            .FirstOrDefaultAsync();
    }

    public async Task<BaseNode> AddAsync(BaseNode baseNode)
    {
        _context.BaseNodes.Add(baseNode);
        await _context.SaveChangesAsync();
        return baseNode;
    }

    public async Task<BaseNode> UpdateAsync(BaseNode baseNode)
    {
        _context.BaseNodes.Update(baseNode);
        await _context.SaveChangesAsync();
        return baseNode;
    }

    public async Task DeleteAsync(Guid id)
    {
        var baseNode = await _context.BaseNodes.FindAsync(id);
        if (baseNode != null)
        {
            _context.BaseNodes.Remove(baseNode);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<BaseNode>> GetAllAsync()
    {
        return await _context.BaseNodes.ToListAsync();
    }
}