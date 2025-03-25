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

        // Modify this method to return all nodes with the same name
        public async Task<IEnumerable<Node>> GetByNameAsync(string name)
        {
            return await _context.Nodes
                .Where(n => n.Name == name)
                .ToListAsync(); // Now returns all nodes with that name
        }

        public async Task<Node> AddAsync(Node node)
        {
            _context.Nodes.Add(node);
            await _context.SaveChangesAsync();
            return node;
        }

        public async Task<Node?> UpdateAsync(Node node)
        {
            if (node.ParentId.HasValue)
            {
                // Fetch the parent node by ParentId
                var parentNode = await _context.BaseNodes.FirstOrDefaultAsync(bn => bn.Id == node.ParentId.Value);
                if (parentNode != null)
                {
                    node.ParentName = parentNode.Name;
                }
            }

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
    
         public async Task<BaseNode?> GetParentByIdAsync(Guid parentId)
        {
            // Fetch the parent node from the BaseNode table by its Id
            return await _context.BaseNodes.FirstOrDefaultAsync(bn => bn.Id == parentId);
        }

        public async Task<Node?> GetByNameAndParentAsync(string name, string parentName)
        {
            return await _context.Nodes
                .Where(n => n.Name == name && n.ParentName == parentName)
                .FirstOrDefaultAsync();
        }



    }
}
