using Bakalauras.App.Services.IServices;
using Bakalauras.Domain.Models;
using Bakalauras.Persistence.Repositories;
using Microsoft.Extensions.Logging;

namespace Bakalauras.App.Services
{
    public class DijkstraService : IDijkstraService
    {
        private readonly INodeRepository _nodeRepository;
        private readonly INodeConnectionRepository _nodeConnectionRepository;
        private readonly ILogger<DijkstraService> _logger;

        public DijkstraService(INodeRepository nodeRepository, INodeConnectionRepository nodeConnectionRepository, ILogger<DijkstraService> logger)
        {
            _nodeRepository = nodeRepository;
            _nodeConnectionRepository = nodeConnectionRepository;
            _logger = logger;
        }

        public async Task<List<Node>> FindShortestPathAsync(Guid startNodeId, Guid endNodeId)
        {
            var nodes = (await _nodeRepository.GetAllAsync()).ToList();
            var connections = (await _nodeConnectionRepository.GetAllAsync()).ToList();

            var nodeDict = nodes.ToDictionary(n => n.Id);
            var adjacencyList = new Dictionary<Guid, List<NodeConnection>>();

            foreach (var conn in connections)
            {
                if (!adjacencyList.ContainsKey(conn.FromNodeId))
                    adjacencyList[conn.FromNodeId] = new List<NodeConnection>();

                adjacencyList[conn.FromNodeId].Add(conn);
            }

            var distances = nodes.ToDictionary(n => n.Id, _ => float.MaxValue);
            var previousNodes = new Dictionary<Guid, Guid?>();
            var priorityQueue = new SortedSet<(float, Guid)>(Comparer<(float, Guid)>.Create((a, b) =>
            {
                int compare = a.Item1.CompareTo(b.Item1);
                return compare == 0 ? a.Item2.CompareTo(b.Item2) : compare;
            }));

            distances[startNodeId] = 0;
            priorityQueue.Add((0, startNodeId));

            // Dijkstra's Algorithm
            while (priorityQueue.Count > 0)
            {
                var (currentDistance, currentNodeId) = priorityQueue.First();
                priorityQueue.Remove(priorityQueue.First());

                if (currentNodeId == endNodeId) break;

                if (!adjacencyList.ContainsKey(currentNodeId)) continue;

                foreach (var connection in adjacencyList[currentNodeId])
                {
                    var neighborId = connection.ToNodeId;
                    float newDistance = currentDistance + connection.Weight;

                    if (newDistance < distances[neighborId])
                    {
                        priorityQueue.Remove((distances[neighborId], neighborId)); 
                        distances[neighborId] = newDistance;
                        previousNodes[neighborId] = currentNodeId;
                        priorityQueue.Add((newDistance, neighborId));
                    }
                }
            }

            var path = new List<Node>();
            Guid? at = endNodeId;
            while (at.HasValue && previousNodes.ContainsKey(at.Value))
            {
                path.Add(nodeDict[at.Value]);
                at = previousNodes[at.Value];
            }

            if (at == startNodeId)
            {
                path.Add(nodeDict[startNodeId]);
            }

            path.Reverse();
            return path;
        }
    }
}
