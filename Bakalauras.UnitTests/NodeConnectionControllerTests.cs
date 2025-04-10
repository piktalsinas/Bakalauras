using Bakalauras.App.Services;
using Bakalauras.Domain.Models;
using Bakalauras.Persistence.Repositories;
using Microsoft.Extensions.Logging;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Bakalauras.API.Controllers;

namespace Bakalauras.UnitTests
{
    public class NodeConnectionControllerTests
    {
        private Mock<ILogger<NodeConnectionController>> _loggerMockForController;
        private Mock<ILogger<NodeConnectionService>> _loggerMockForService;
        private Mock<ILogger<IDijkstraService>> _loggerMockForDijkstraService;
        private Mock<INodeConnectionRepository> _nodeConnectionRepoMock;
        private Mock<INodeRepository> _nodeRepoMock;
        private Mock<IDijkstraService> _dijkstraServiceMock;
        private Mock<INodeNameSevice> _nodeNameServiceMock;
        private Mock<INodeConnectionService> _nodeConnectionServiceMock;
        private NodeConnectionController _controller;

        [SetUp]
        public void SetUp()
        {
            _loggerMockForController = new Mock<ILogger<NodeConnectionController>>();
            _loggerMockForService = new Mock<ILogger<NodeConnectionService>>();
            _loggerMockForDijkstraService = new Mock<ILogger<IDijkstraService>>();
            _nodeConnectionRepoMock = new Mock<INodeConnectionRepository>();
            _nodeRepoMock = new Mock<INodeRepository>();
            _dijkstraServiceMock = new Mock<IDijkstraService>();  // Mocking the interface
            _nodeNameServiceMock = new Mock<INodeNameSevice>();
            _nodeConnectionServiceMock = new Mock<INodeConnectionService>(); // Mocking the interface

            _controller = new NodeConnectionController(
                _loggerMockForController.Object,
                _nodeConnectionRepoMock.Object,
                _nodeConnectionServiceMock.Object,
                _dijkstraServiceMock.Object,
                _nodeNameServiceMock.Object
            );
        }

        [Test]
        public async Task GetAll_NodeConnectionsExists()
        {
            var nodeConnections = new List<NodeConnection>
            {
                new NodeConnection { FromNodeId = Guid.NewGuid(), ToNodeId = Guid.NewGuid(), Weight = 1.0f },
                new NodeConnection { FromNodeId = Guid.NewGuid(), ToNodeId = Guid.NewGuid(), Weight = 2.0f }
            };

            _nodeConnectionRepoMock.Setup(repo => repo.GetAllAsync()).ReturnsAsync(nodeConnections);

            var result = await _controller.GetAll();

            var okResult = result.Result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            Assert.AreEqual(nodeConnections, okResult.Value);
        }

        [Test]
        public async Task Post_NodeConnectionCreated()
        {
            var fromNodeId = Guid.NewGuid();
            var toNodeId = Guid.NewGuid();
            var weight = 10f;

            var newNodeConnection = new NodeConnection
            {
                FromNodeId = fromNodeId,
                ToNodeId = toNodeId,
                Weight = weight
            };

            _nodeConnectionRepoMock.Setup(repo => repo.AddAsync(It.IsAny<NodeConnection>())).ReturnsAsync(newNodeConnection);

            var result = await _controller.Post(fromNodeId, toNodeId, weight);

            var okResult = result.Result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            Assert.AreEqual(newNodeConnection, okResult.Value);
        }

        [Test]
        public async Task Post_NodeConnectionAlreadyExists()
        {
            var fromNodeId = Guid.NewGuid();
            var toNodeId = Guid.NewGuid();
            var weight = 10f;

            _nodeConnectionRepoMock.Setup(repo => repo.AddAsync(It.IsAny<NodeConnection>())).ReturnsAsync((NodeConnection?)null);

            var result = await _controller.Post(fromNodeId, toNodeId, weight);

            var badRequestResult = result.Result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual(400, badRequestResult.StatusCode);
            Assert.AreEqual("Node connection already exists or nodes not found.", badRequestResult.Value);
        }

        [Test]
        public async Task GetShortestPath_ReturnNotFound_NodesNotFound()
        {
            var fullName1 = "S1_Exit";
            var fullName2 = "S1_Room3";

            _nodeNameServiceMock.Setup(service => service.GetNodeIdByFullName(fullName1)).ReturnsAsync((Guid?)null);
            _nodeNameServiceMock.Setup(service => service.GetNodeIdByFullName(fullName2)).ReturnsAsync((Guid?)null);

            var result = await _controller.GetShortestPath(fullName1, fullName2);

            var notFoundResult = result.Result as NotFoundObjectResult;
            Assert.IsNotNull(notFoundResult);
            Assert.AreEqual(404, notFoundResult.StatusCode);
            Assert.AreEqual("One or both nodes not found.", notFoundResult.Value);
        }

        [Test]
        public async Task CopyImageToPathFolderAsync_ImageIsCopiedSuccessfully()
        {
            var nodeConnectionId = Guid.NewGuid();
            _nodeConnectionRepoMock.Setup(repo => repo.GetByIdAsync(nodeConnectionId))
                .ReturnsAsync(new NodeConnection { FromNodeId = Guid.NewGuid(), ToNodeId = Guid.NewGuid() });

            _nodeRepoMock.Setup(repo => repo.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(new Node { Id = Guid.NewGuid() });

            _nodeConnectionServiceMock.Setup(service => service.MoveImageToPathFolderAsync(nodeConnectionId)).ReturnsAsync(true);

            var result = await _controller.CopyImageToPathFolderAsync(nodeConnectionId);

            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            Assert.AreEqual("Image successfully copied to Path folder.", okResult.Value);
        }

        [Test]
        public async Task CopyImageToPathFolderAsync_ImageNotFound()
        {
            var nodeConnectionId = Guid.NewGuid();
            _nodeConnectionRepoMock.Setup(repo => repo.GetByIdAsync(nodeConnectionId))
                .ReturnsAsync(new NodeConnection { FromNodeId = Guid.NewGuid(), ToNodeId = Guid.NewGuid() });

            _nodeRepoMock.Setup(repo => repo.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(new Node { Id = Guid.NewGuid() });

            _nodeConnectionServiceMock.Setup(service => service.MoveImageToPathFolderAsync(nodeConnectionId)).ReturnsAsync(false);

            var result = await _controller.CopyImageToPathFolderAsync(nodeConnectionId);

            var notFoundResult = result as NotFoundObjectResult;
            Assert.IsNotNull(notFoundResult);
            Assert.AreEqual(404, notFoundResult.StatusCode);
            Assert.AreEqual("Image not found or an error occurred.", notFoundResult.Value);
        }

        [Test]
        public async Task GetShortestPath_WhenPathFound()
        {
            var fullName1 = "S1_Exit";
            var fullName2 = "S1_Room3";

            var node1Id = Guid.NewGuid();
            var node2Id = Guid.NewGuid();

            _nodeNameServiceMock.Setup(service => service.GetNodeIdByFullName(fullName1)).ReturnsAsync(node1Id);
            _nodeNameServiceMock.Setup(service => service.GetNodeIdByFullName(fullName2)).ReturnsAsync(node2Id);

            var pathNodes = new List<Node> { new Node { Id = node1Id }, new Node { Id = node2Id } };
            _dijkstraServiceMock.Setup(service => service.FindShortestPathAsync(node1Id, node2Id)).ReturnsAsync(pathNodes);

            _nodeConnectionServiceMock.Setup(service => service.CopyPathImagesAsync(It.IsAny<List<Node>>())).ReturnsAsync(true);

            var result = await _controller.GetShortestPath(fullName1, fullName2);

            var okResult = result.Result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
        }

        [Test]
        public async Task GetShortestPath_NodesNotFound()
        {
            var fullName1 = "S1_Exit";
            var fullName2 = "S1_Room3";

            _nodeNameServiceMock.Setup(service => service.GetNodeIdByFullName(fullName1)).ReturnsAsync((Guid?)null);
            _nodeNameServiceMock.Setup(service => service.GetNodeIdByFullName(fullName2)).ReturnsAsync((Guid?)null);

            var result = await _controller.GetShortestPath(fullName1, fullName2);

            var notFoundResult = result.Result as NotFoundObjectResult;
            Assert.IsNotNull(notFoundResult);
            Assert.AreEqual(404, notFoundResult.StatusCode);
            Assert.AreEqual("One or both nodes not found.", notFoundResult.Value);
        }

        [Test]
        public async Task GetShortestPath_NoPathFound()
        {
            var fullName1 = "S1_Exit";
            var fullName2 = "S1_Room3";

            var node1Id = Guid.NewGuid();
            var node2Id = Guid.NewGuid();

            _nodeNameServiceMock.Setup(service => service.GetNodeIdByFullName(fullName1)).ReturnsAsync(node1Id);
            _nodeNameServiceMock.Setup(service => service.GetNodeIdByFullName(fullName2)).ReturnsAsync(node2Id);
            _dijkstraServiceMock.Setup(service => service.FindShortestPathAsync(node1Id, node2Id)).ReturnsAsync(new List<Node>());

            var result = await _controller.GetShortestPath(fullName1, fullName2);

            var notFoundResult = result.Result as NotFoundObjectResult;
            Assert.IsNotNull(notFoundResult);
            Assert.AreEqual(404, notFoundResult.StatusCode);
            Assert.AreEqual("No path found between the specified nodes.", notFoundResult.Value);
        }

       
    }
}
