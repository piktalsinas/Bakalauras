using Bakalauras.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Bakalauras.Persistence.Repositories.EF
{
    public class NodeConnectionRepository : INodeConnectionRepository
    {
        private readonly AppDbContext _context;

        public NodeConnectionRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<NodeConnection?> AddAsync(NodeConnection nodeConnection)
        {
            bool exists = await _context.NodeConnections.AnyAsync(nc => nc.FromNodeId == nodeConnection.FromNodeId && nc.ToNodeId == nodeConnection.ToNodeId);
            if (exists)
            {
                return null;
            }

            var fromNode = await _context.Nodes.Include(n => n.Parent).FirstOrDefaultAsync(n => n.Id == nodeConnection.FromNodeId);
            var toNode = await _context.Nodes.Include(n => n.Parent).FirstOrDefaultAsync(n => n.Id == nodeConnection.ToNodeId);

            if (fromNode != null && toNode != null)
            {
                var fromParentName = fromNode.ParentName ?? "NoParent";
                var toParentName = toNode.ParentName ?? "NoParent";

                nodeConnection.Name = $"{fromParentName}_{fromNode.Name}_{toParentName}_{toNode.Name}";
            }
            else
            {
                return null;
            }

            await _context.NodeConnections.AddAsync(nodeConnection);
            await _context.SaveChangesAsync();
            return nodeConnection;
        }
        public async Task DeleteAsync(Guid id)
        {
            NodeConnection? nodeConnection = await _context.NodeConnections.FindAsync(id);
            if (nodeConnection is not null)
            {
                _context.NodeConnections.Remove(nodeConnection);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<NodeConnection>> GetAllAsync()
        {
            return await _context.NodeConnections.ToListAsync();
        }

        public async Task<IEnumerable<NodeConnection>> GetAllConnectionsByNodeIdAsync(Guid id)
        {
            return await _context.NodeConnections.Where(nc => nc.FromNodeId == id || nc.ToNodeId == id).ToListAsync();
        }

        public async Task<NodeConnection?> GetByIdAsync(Guid id)
        {
            return await _context.NodeConnections.FindAsync(id);
        }

        public async Task<NodeConnection?> UpdateAsync(NodeConnection nodeConnection)
        {
            _context.NodeConnections.Update(nodeConnection);
            await _context.SaveChangesAsync();
            return nodeConnection;
        }
    }
}
