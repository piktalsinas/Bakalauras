using Microsoft.AspNetCore.Mvc;
using Bakalauras.API.Controllers;
using Moq;

namespace Bakalauras.UnitTests
{
    public class ImageControllerTests
    {
        private ImageController _controller;

        [SetUp]
        public void Setup()
        {
            _controller = new ImageController();
        }

        [Test]
        public void GetImage_ShouldReturnCorrectContentType_ForJpgFile()
        {
            var fileName = "S1.jpg";
            var filePath = Path.Combine(@"C:\Users\picom\Documents\BAKALAURAS\BaseNodes", fileName);

            File.WriteAllText(filePath, "mock content");

            var result = _controller.GetImage(fileName);

            var contentResult = result as FileContentResult;
            Assert.IsNotNull(contentResult);
            Assert.AreEqual("image/jpeg", contentResult.ContentType); // Assert correct MIME type for JPG
        }

        [Test]
        public void GetImage_InvalidFileName_ReturnsBadRequest()
        {
            // Arrange
            string invalidFileName = null;

            // Act
            var result = _controller.GetImage(invalidFileName) as BadRequestObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(400, result.StatusCode);
            Assert.AreEqual("Filename is required.", result.Value);
        }

        [Test]
        public void GetImage_FileNotFound_ReturnsNotFound()
        {
            // Arrange
            string nonExistentFileName = "nonexistent.jpg";

            // Act
            var result = _controller.GetImage(nonExistentFileName) as NotFoundObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(404, result.StatusCode);
            Assert.AreEqual("Image not found.", result.Value);
        }

    }
}
