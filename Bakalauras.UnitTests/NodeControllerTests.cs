using Bakalauras.Domain.Models;
using Bakalauras.Persistence.Repositories;
using Bakalauras.API.Controllers;
using Bakalauras.App.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
            _loggerControllerMock = new Mock<ILogger<NodeController>>();
            _loggerServiceMock = new Mock<ILogger<NodeService>>();
            _nodeRepositoryMock = new Mock<INodeRepository>();

            _nodeService = new NodeService(_nodeRepositoryMock.Object, _loggerServiceMock.Object);
            _nodeController = new NodeController(_loggerControllerMock.Object, _nodeRepositoryMock.Object, _nodeService);
        }

        [Test]
        public async Task GetAll_ReturnsOk()
        {
            var nodes = new List<Node>
            {
                new Node { Id = Guid.NewGuid(), Name = "Node1" },
                new Node { Id = Guid.NewGuid(), Name = "Node2" }
            };

            _nodeRepositoryMock.Setup(repo => repo.GetAllAsync()).ReturnsAsync(nodes);

            var result = await _nodeController.GetAll();

            Assert.IsInstanceOf<ActionResult<IEnumerable<Node>>>(result);
            var okResult = result.Result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);

            var returnValue = okResult.Value as IEnumerable<Node>;
            Assert.IsNotNull(returnValue);
            Assert.AreEqual(2, returnValue.Count());
        }

        [Test]
        public async Task GetNodesByBaseNode_ReturnsBadRequest()
        {
            var result = await _nodeController.GetNodesByBaseNode(null);

            Assert.IsInstanceOf<ActionResult<IEnumerable<Node>>>(result);
            var badRequestResult = result.Result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual(400, badRequestResult.StatusCode);
            Assert.AreEqual("BaseNode name is required.", badRequestResult.Value);
        }

        [Test]
        public async Task GetNodesByBaseNode_ReturnsNotFound()
        {
            string baseNodeName = "S1";
            _nodeRepositoryMock.Setup(repo => repo.GetNodesByBaseNodeAsync(baseNodeName)).ReturnsAsync(new List<Node>());

            var result = await _nodeController.GetNodesByBaseNode(baseNodeName);

            Assert.IsInstanceOf<ActionResult<IEnumerable<Node>>>(result);
            var notFoundResult = result.Result as NotFoundObjectResult;
            Assert.IsNotNull(notFoundResult);
            Assert.AreEqual(404, notFoundResult.StatusCode);
            Assert.AreEqual($"No nodes found with BaseNode '{baseNodeName}' as their parent.", notFoundResult.Value);
        }

        [Test]
        public async Task Post_ReturnsOk()
        {
            string nodeName = "NewNode";
            var newNode = new Node { Id = Guid.NewGuid(), Name = nodeName };

            _nodeRepositoryMock.Setup(repo => repo.AddAsync(It.IsAny<Node>())).ReturnsAsync(newNode);

            var result = await _nodeController.Post(nodeName);

            Assert.IsInstanceOf<ActionResult<Node>>(result);
            var okResult = result.Result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);

            var returnValue = okResult.Value as Node;
            Assert.IsNotNull(returnValue);
            Assert.AreEqual(nodeName, returnValue.Name);
        }

        [Test]
        public async Task Put_ReturnsOk()
        {
            Guid nodeId = Guid.NewGuid();
            string updatedName = "UpdatedNode";
            var existingNode = new Node { Id = nodeId, Name = "OriginalNode" };
            var updatedNode = new Node { Id = nodeId, Name = updatedName };

            _nodeRepositoryMock.Setup(repo => repo.GetByIdAsync(nodeId)).ReturnsAsync(existingNode);
            _nodeRepositoryMock.Setup(repo => repo.UpdateAsync(It.IsAny<Node>())).ReturnsAsync(updatedNode);

            var result = await _nodeController.Put(nodeId, updatedName);

            Assert.IsInstanceOf<ActionResult<Node>>(result);
            var okResult = result.Result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);

            var returnValue = okResult.Value as Node;
            Assert.IsNotNull(returnValue);
            Assert.AreEqual(updatedName, returnValue.Name);
        }

        [Test]
        public async Task Delete_ReturnsNoContent()
        {
            Guid nodeId = Guid.NewGuid();
            _nodeRepositoryMock.Setup(repo => repo.DeleteAsync(nodeId)).Returns(Task.CompletedTask);

            var result = await _nodeController.Delete(nodeId);

            Assert.IsInstanceOf<NoContentResult>(result);
            var noContentResult = result as NoContentResult;
            Assert.IsNotNull(noContentResult);
            Assert.AreEqual(204, noContentResult.StatusCode);
        }

        [Test]
        public async Task Get_ReturnsOk()
        {
            Guid nodeId = Guid.NewGuid();
            var node = new Node { Id = nodeId, Name = "TestNode" };

            _nodeRepositoryMock.Setup(repo => repo.GetByIdAsync(nodeId)).ReturnsAsync(node);

            var result = await _nodeController.Get(nodeId);

            Assert.IsInstanceOf<ActionResult<Node>>(result);
            var okResult = result.Result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);

            var returnValue = okResult.Value as Node;
            Assert.IsNotNull(returnValue);
            Assert.AreEqual(nodeId, returnValue.Id);
        }

        [Test]
        public async Task AddNodesFromImages_ReturnsOk()
        {
            _nodeService = new NodeService(_nodeRepositoryMock.Object, _loggerServiceMock.Object);
            _nodeController = new NodeController(_loggerControllerMock.Object, _nodeRepositoryMock.Object, _nodeService);

            var result = await _nodeController.AddNodesFromImages();

            Assert.IsInstanceOf<OkObjectResult>(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            Assert.AreEqual("Nodes added successfully from images.", okResult.Value);
        }

        [Test]
        public async Task CopyImageToPathFolder_ReturnsOk()
        {
            string parentNodeName = "S1";
            string nodeName = "Elevator4";

            var node = new Node { Name = nodeName, ParentName = parentNodeName };
            _nodeRepositoryMock.Setup(repo => repo.GetByNameAsync(nodeName)).ReturnsAsync(new List<Node> { node });

            var result = await _nodeController.CopyImageToPathFolderAsync(parentNodeName, nodeName);

            Assert.IsInstanceOf<OkObjectResult>(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            Assert.AreEqual("Image successfully copied to Path folder.", okResult.Value);
        }


        [Test]
        public async Task UpdateNodeParent_ReturnsOk()
        {
            Guid nodeId = Guid.NewGuid();
            Guid parentId = Guid.NewGuid();

            var parentNode = new BaseNode { Id = parentId, Name = "ParentNode" };
            var node = new Node { Id = nodeId, Name = "ChildNode", ParentId = null, ParentName = null };

            _nodeRepositoryMock.Setup(repo => repo.GetByIdAsync(nodeId)).ReturnsAsync(node);
            _nodeRepositoryMock.Setup(repo => repo.GetParentByIdAsync(parentId)).ReturnsAsync(parentNode);
            _nodeRepositoryMock.Setup(repo => repo.UpdateAsync(It.IsAny<Node>())).ReturnsAsync(node);

            var result = await _nodeController.UpdateNodeParent(nodeId, parentId);

            Assert.IsInstanceOf<ActionResult<Node>>(result);

            var okResult = result.Result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);

            var returnValue = okResult.Value as Node;
            Assert.IsNotNull(returnValue);
            Assert.AreEqual(parentId, returnValue.ParentId);
            Assert.AreEqual("ParentNode", returnValue.ParentName);
        }
    }
}
