using Bakalauras.App.Services;
using Bakalauras.Domain.Models;
using Bakalauras.Persistence.Repositories;
using Microsoft.Extensions.Logging;
using Moq;

namespace Bakalauras.UnitTests
{
    public class DijkstraTests
    {
        private Mock<INodeRepository> _mockNodeRepository;
        private Mock<INodeConnectionRepository> _mockNodeConnectionRepository;
        private Mock<ILogger<DijkstraService>> _mockLogger;
        private DijkstraService _dijkstraService;

        [SetUp]
        public void SetUp()
        {
            _mockNodeRepository = new Mock<INodeRepository>();
            _mockNodeConnectionRepository = new Mock<INodeConnectionRepository>();
            _mockLogger = new Mock<ILogger<DijkstraService>>();

            _dijkstraService = new DijkstraService(
                _mockNodeRepository.Object,
                _mockNodeConnectionRepository.Object,
                _mockLogger.Object);
        }

        [Test]
        public async Task FindShortestPath_ReturnsCorrectPath()
        {
            var node1 = new Node { Id = Guid.NewGuid(), Name = "A" };
            var node2 = new Node { Id = Guid.NewGuid(), Name = "B" };
            var node3 = new Node { Id = Guid.NewGuid(), Name = "C" };

            var nodes = new List<Node> { node1, node2, node3 };

            var connection1 = new NodeConnection { FromNodeId = node1.Id, ToNodeId = node2.Id, Weight = 1 };
            var connection2 = new NodeConnection { FromNodeId = node2.Id, ToNodeId = node3.Id, Weight = 1 };

            var connections = new List<NodeConnection> { connection1, connection2 };

            _mockNodeRepository.Setup(repo => repo.GetAllAsync()).ReturnsAsync(nodes);
            _mockNodeConnectionRepository.Setup(repo => repo.GetAllAsync()).ReturnsAsync(connections);

            var path = await _dijkstraService.FindShortestPathAsync(node1.Id, node3.Id);

            Assert.IsNotNull(path);
            Assert.AreEqual(3, path.Count);
            Assert.AreEqual("A", path[0].Name);
            Assert.AreEqual("B", path[1].Name);
            Assert.AreEqual("C", path[2].Name);
        }

        [Test]
        public async Task FindShortestPath_NoPathExists()
        {
            var node1 = new Node { Id = Guid.NewGuid(), Name = "A" };
            var node2 = new Node { Id = Guid.NewGuid(), Name = "B" };

            var nodes = new List<Node> { node1, node2 };
            var connections = new List<NodeConnection>();  

            _mockNodeRepository.Setup(repo => repo.GetAllAsync()).ReturnsAsync(nodes);
            _mockNodeConnectionRepository.Setup(repo => repo.GetAllAsync()).ReturnsAsync(connections);

            var path = await _dijkstraService.FindShortestPathAsync(node1.Id, node2.Id);

            Assert.IsEmpty(path);
        }

        [Test]
        public async Task FindShortestPath_StartAndEndAreTheSame_ReturnsSingleNodePath()
        {
            var node1 = new Node { Id = Guid.NewGuid(), Name = "A" };

            var nodes = new List<Node> { node1 };
            var connections = new List<NodeConnection>();  

            _mockNodeRepository.Setup(repo => repo.GetAllAsync()).ReturnsAsync(nodes);
            _mockNodeConnectionRepository.Setup(repo => repo.GetAllAsync()).ReturnsAsync(connections);

            var path = await _dijkstraService.FindShortestPathAsync(node1.Id, node1.Id);

            Assert.IsNotNull(path);
            Assert.AreEqual(1, path.Count);
            Assert.AreEqual("A", path[0].Name);
        }

        [Test]
        public async Task FindShortestPath_NodeNotFound()
        {
            var node1 = new Node { Id = Guid.NewGuid(), Name = "A" };
            var node2 = new Node { Id = Guid.NewGuid(), Name = "B" };

            var nodes = new List<Node> { node1 };
            var connections = new List<NodeConnection>(); 

            _mockNodeRepository.Setup(repo => repo.GetAllAsync()).ReturnsAsync(nodes);
            _mockNodeConnectionRepository.Setup(repo => repo.GetAllAsync()).ReturnsAsync(connections);

            var path = await _dijkstraService.FindShortestPathAsync(node1.Id, node2.Id);

            Assert.IsEmpty(path);
        }

        [Test]
        public async Task FindShortestPath_HandlesMultiplePaths_ReturnsShortest()
        {
            var node1 = new Node { Id = Guid.NewGuid(), Name = "A" };
            var node2 = new Node { Id = Guid.NewGuid(), Name = "B" };
            var node3 = new Node { Id = Guid.NewGuid(), Name = "C" };
            var node4 = new Node { Id = Guid.NewGuid(), Name = "D" };

            var nodes = new List<Node> { node1, node2, node3, node4 };

            var connection1 = new NodeConnection { FromNodeId = node1.Id, ToNodeId = node2.Id, Weight = 2 };
            var connection2 = new NodeConnection { FromNodeId = node1.Id, ToNodeId = node3.Id, Weight = 1 };
            var connection3 = new NodeConnection { FromNodeId = node3.Id, ToNodeId = node4.Id, Weight = 1 };
            var connection4 = new NodeConnection { FromNodeId = node2.Id, ToNodeId = node4.Id, Weight = 1 };

            var connections = new List<NodeConnection> { connection1, connection2, connection3, connection4 };

            _mockNodeRepository.Setup(repo => repo.GetAllAsync()).ReturnsAsync(nodes);
            _mockNodeConnectionRepository.Setup(repo => repo.GetAllAsync()).ReturnsAsync(connections);

            var path = await _dijkstraService.FindShortestPathAsync(node1.Id, node4.Id);

            Assert.IsNotNull(path);
            Assert.AreEqual(3, path.Count);
            Assert.AreEqual("A", path[0].Name);
            Assert.AreEqual("C", path[1].Name);
            Assert.AreEqual("D", path[2].Name);
        }
    }
}
