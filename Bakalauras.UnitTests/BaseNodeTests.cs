using Bakalauras.App.Services;
using Bakalauras.Domain.Models;
using Bakalauras.Persistence.Repositories;
using Microsoft.Extensions.Logging;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Bakalauras.API.Controllers;
using NUnit.Framework;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Bakalauras.UnitTests
{
    public class BaseNodeTests
    {
        private Mock<ILogger<BaseNodeController>> _mockLogger;
        private Mock<IBaseNodeRepository> _mockBaseNodeRepository;
        private Mock<BaseNodeService> _mockBaseNodeService; 
        private BaseNodeController _controller;
        private Mock<ILogger<BaseNodeService>> _mockLoggerForService;

        [SetUp]
        public void SetUp()
        {
            _mockLogger = new Mock<ILogger<BaseNodeController>>();
            _mockBaseNodeRepository = new Mock<IBaseNodeRepository>();

            _mockBaseNodeService = new Mock<BaseNodeService>(_mockBaseNodeRepository.Object, new Mock<ILogger<BaseNodeService>>().Object);

            _controller = new BaseNodeController(_mockLogger.Object, _mockBaseNodeRepository.Object, _mockBaseNodeService.Object);
        }

        [Test]
        public async Task Post_BaseNodeAlreadyExists()
        {
            var name = "S1";
            _mockBaseNodeService
                .Setup(service => service.AddBaseNodeAsync(name))
                .ReturnsAsync((BaseNode)null);

            var result = await _controller.Post(name);

            var badRequestResult = result.Result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual("BaseNode with this name already exists.", badRequestResult.Value);
        }
       
        [Test]
        public async Task Post_BaseNodeCreatedSuccessfully()
        {
            var name = "NewBaseNode";
            var newBaseNode = new BaseNode { Id = Guid.NewGuid(), Name = name };
            _mockBaseNodeService
                .Setup(service => service.AddBaseNodeAsync(name))
                .ReturnsAsync(newBaseNode);

            var result = await _controller.Post(name);

            var okResult = result.Result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(newBaseNode, okResult.Value);
        }

        [Test]
        public async Task AddBaseNodesFromImages_Success()
        {
            var folderPath = @"C:\Users\picom\Documents\BAKALAURAS\BaseNodes";
            _mockBaseNodeService
                .Setup(service => service.AddBaseNodesFromImagesAsync(folderPath))
                .Returns(Task.CompletedTask);

            var result = await _controller.AddBaseNodesFromImages();

            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual("BaseNodes added successfully from images.", okResult.Value);
        }

       

        [Test]
        public async Task CopyBaseNodeImageToPathFolder_BaseNodeNotFound()
        {
            var baseNodeName = "NonExistentBaseNode";
            _mockBaseNodeRepository
                .Setup(repo => repo.GetByNameAsync(baseNodeName))
                .ReturnsAsync((BaseNode)null);

            var result = await _controller.CopyBaseNodeImageToPathFolderAsync(baseNodeName);

            var notFoundResult = result as NotFoundObjectResult;
            Assert.IsNotNull(notFoundResult);
            Assert.AreEqual("BaseNode not found.", notFoundResult.Value);
        }

        [Test]
        public async Task AddBaseNodesFromImages_FileExistsAndBaseNodeAlreadyExists()
        {
            var folderPath = @"C:\Users\picom\Documents\BAKALAURAS\BaseNodes";
            var file = @"C:\Users\picom\Documents\BAKALAURAS\BaseNodes\S1.jpg";
            var baseNodeName = "S1";

            _mockBaseNodeRepository
                .Setup(repo => repo.GetByNameAsync(baseNodeName))
                .ReturnsAsync(new BaseNode { Name = baseNodeName });

            var result = await _controller.AddBaseNodesFromImages();
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual("BaseNodes added successfully from images.", okResult.Value);
        }

     

        [Test]
        public void AddBaseNodesFromImagesAsync_DirectoryDoesNotExist_ThrowsDirectoryNotFoundException()
        {
            string folderPath = @"C:\\InvalidPath";

            var mockLogger = new Mock<ILogger<BaseNodeService>>();
            var mockBaseNodeRepository = new Mock<IBaseNodeRepository>();

            var service = new BaseNodeService(mockBaseNodeRepository.Object, mockLogger.Object);

            Assert.ThrowsAsync<DirectoryNotFoundException>(async () => await service.AddBaseNodesFromImagesAsync(folderPath));
        }


        [Test]
        public async Task AddBaseNodeAlreadyExists()
        {
            var name = "S1";
            _mockBaseNodeRepository
                .Setup(repo => repo.GetByNameAsync(name))
                .ReturnsAsync(new BaseNode { Name = name });  

            var result = await _controller.Post(name);

            var badRequestResult = result.Result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual("BaseNode with this name already exists.", badRequestResult.Value);
        }


        [Test]
        public async Task CopyBaseNodeImageToPathFolder_Success()
        {
            var baseNodeName = "S1";
            var baseNode = new BaseNode { Id = Guid.NewGuid(), Name = baseNodeName };
            _mockBaseNodeRepository
                .Setup(repo => repo.GetByNameAsync(baseNodeName))
                .ReturnsAsync(baseNode);

            var result = await _controller.CopyBaseNodeImageToPathFolderAsync(baseNodeName);

            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual("Image successfully copied to Path folder.", okResult.Value);
        }
    }
}
