using Bakalauras.Domain.Models;
using Bakalauras.Persistence.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Bakalauras.App.Services
{
    public interface INodeNameSevice
    {
        Task<string> GetFullNodeName(Guid nodeId);
        Task<Guid?> GetNodeIdByFullName(string fullName);
    }

    public class NodeNameSevice : INodeNameSevice
    {
        private readonly INodeRepository _nodeRepository;
        private readonly IBaseNodeRepository _baseNodeRepository;

        public NodeNameSevice(INodeRepository nodeRepository, IBaseNodeRepository baseNodeRepository)
        {
            _nodeRepository = nodeRepository ?? throw new ArgumentNullException(nameof(nodeRepository));
            _baseNodeRepository = baseNodeRepository ?? throw new ArgumentNullException(nameof(baseNodeRepository));
        }


        public async Task<string> GetFullNodeName(Guid nodeId)
        {
            Node? node = await _nodeRepository.GetByIdAsync(nodeId);

            if (node is null)
                return string.Empty;

            if (node.ParentId is null)
                return node.Name;

            LinkedList<string> names = new LinkedList<string>();
            names.AddFirst(node.Name);

        
            BaseNode? @base = await _baseNodeRepository.GetByIdAsync(node.ParentId!.Value);
            while (@base is not null)
            {
                names.AddFirst(@base.Name);
                @base = null;
                // @base = @base?.ParentNodeId is not null ? await _baseNodeRepository.GetByIdAsync(@base!.ParentNodeId) : null;
            }

            return string.Join("_", names);
        }

        public async Task<Guid?> GetNodeIdByFullName(string fullName)
        {
            string[] names = fullName.Split('_');
            List<Node> nodes = (await _nodeRepository.GetByNameAsync(names[^1])).ToList();
            
            if (nodes.Count < 2)
                return nodes.FirstOrDefault()?.Id;

            Index index = ^2;
            BaseNode? @base = await _baseNodeRepository.GetByNameAsync(names[index]);
            return nodes.FirstOrDefault(n => n?.ParentName == @base?.Name)?.Id ?? null;
        }
    }
}
