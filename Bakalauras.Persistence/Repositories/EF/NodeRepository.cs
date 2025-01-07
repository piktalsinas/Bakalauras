using Bakalauras.Domain.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Bakalauras.Persistence.Repositories
{
    public class NodeRepository : INodeRepository
    {
        private readonly AppDbContext _context;

        public NodeRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Node?> GetByIdAsync(Guid id)
        {
            return await _context.Nodes.FindAsync(id);
        }

        public async Task<Node?> GetByNameAsync(string name)
        {
            return await _context.Nodes
                .Where(n => n.Name == name)
                .FirstOrDefaultAsync();
        }

        public async Task<Node> AddAsync(Node node)
        {
            _context.Nodes.Add(node);
            await _context.SaveChangesAsync();
            return node;
        }

        public async Task<Node> UpdateAsync(Node node)
        {
            _context.Nodes.Update(node);
            await _context.SaveChangesAsync();
            return node;
        }

        public async Task DeleteAsync(Guid id)
        {
            var node = await _context.Nodes.FindAsync(id);
            if (node != null)
            {
                _context.Nodes.Remove(node);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Node>> GetAllAsync()
        {
            return await _context.Nodes.ToListAsync();
        }
    }
}
