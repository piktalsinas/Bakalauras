using Bakalauras.Domain.Models;
using Bakalauras.Persistence.Repositories;
using Bakalauras.API.Controllers;
using Bakalauras.App.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace Bakalauras.UnitTests
{
    public class NodeControllerTests
    {
        private Mock<ILogger<NodeController>> _loggerControllerMock;
        private Mock<ILogger<NodeService>> _loggerServiceMock;
        private Mock<INodeRepository> _nodeRepositoryMock;
        private NodeService _nodeService;
        private NodeController _nodeController;

        [SetUp]
        public void Setup()
        {
            // Mock the dependencies
            _loggerControllerMock = new Mock<ILogger<NodeController>>();
            _loggerServiceMock = new Mock<ILogger<NodeService>>();
            _nodeRepositoryMock = new Mock<INodeRepository>();

            // Create real instance of NodeService with mocked dependencies
            _nodeService = new NodeService(_nodeRepositoryMock.Object, _loggerServiceMock.Object);

            // Initialize NodeController with mocked dependencies
            _nodeController = new NodeController(_loggerControllerMock.Object, _nodeRepositoryMock.Object, _nodeService);
        }

        [Test]
        public async Task GetAll_ReturnsOkResult_WithNodes()
        {
            // Arrange
            var nodes = new List<Node>
            {
                new Node { Id = Guid.NewGuid(), Name = "Node1" },
                new Node { Id = Guid.NewGuid(), Name = "Node2" }
            };

            _nodeRepositoryMock.Setup(repo => repo.GetAllAsync()).ReturnsAsync(nodes);

            // Act
            var result = await _nodeController.GetAll();

            // Assert
            Assert.IsInstanceOf<ActionResult<IEnumerable<Node>>>(result);
            var okResult = result.Result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);

            var returnValue = okResult.Value as IEnumerable<Node>;
            Assert.IsNotNull(returnValue);
            Assert.AreEqual(2, returnValue.Count());
        }

        [Test]
        public async Task GetNodesByBaseNode_ReturnsBadRequest_WhenBaseNodeNameIsNullOrEmpty()
        {
            // Act
            var result = await _nodeController.GetNodesByBaseNode(null);

            // Assert
            Assert.IsInstanceOf<ActionResult<IEnumerable<Node>>>(result);
            var badRequestResult = result.Result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual(400, badRequestResult.StatusCode);
            Assert.AreEqual("BaseNode name is required.", badRequestResult.Value);
        }

        [Test]
        public async Task GetNodesByBaseNode_ReturnsNotFound_WhenNoNodesFound()
        {
            // Arrange
            string baseNodeName = "S1";
            _nodeRepositoryMock.Setup(repo => repo.GetNodesByBaseNodeAsync(baseNodeName)).ReturnsAsync(new List<Node>());

            // Act
            var result = await _nodeController.GetNodesByBaseNode(baseNodeName);

            // Assert
            Assert.IsInstanceOf<ActionResult<IEnumerable<Node>>>(result);
            var notFoundResult = result.Result as NotFoundObjectResult;
            Assert.IsNotNull(notFoundResult);
            Assert.AreEqual(404, notFoundResult.StatusCode);
            Assert.AreEqual($"No nodes found with BaseNode '{baseNodeName}' as their parent.", notFoundResult.Value);
        }

        [Test]
        public async Task AddNodesFromImages_ReturnsBadRequest_WhenFolderDoesNotExist()
        {
            // Arrange
            var folderPath = @"C:\NonExistentFolder";  // Simulate a non-existent folder path

            // Act
            var result = await _nodeController.AddNodesFromImages();

            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result); 
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual(400, badRequestResult.StatusCode); 
            Assert.IsTrue(badRequestResult.Value.ToString().Contains("An error occurred"));
        }


    }
}
