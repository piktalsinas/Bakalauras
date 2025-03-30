using Microsoft.AspNetCore.Mvc;
using System.IO;

namespace Bakalauras.API.Controllers
{
    [ApiController]
    [Route("api/images")]
    public class ImageController : ControllerBase
    {
        private readonly string _nodeImagesDirectory = @"C:\Users\picom\Documents\BAKALAURAS\Nodes";
        private readonly string _connectionImagesDirectory = @"C:\Users\picom\Documents\BAKALAURAS\ConnectionPhotos";

        [HttpGet("{fileName}")]
        public IActionResult GetImage(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return BadRequest("Filename is required.");

            // Logic: if image name contains 3 or more underscores, it's a connection image
            string selectedDirectory = fileName.Count(c => c == '_') >= 3
                ? _connectionImagesDirectory
                : _nodeImagesDirectory;

            var filePath = Path.Combine(selectedDirectory, fileName);

            if (!System.IO.File.Exists(filePath))
                return NotFound("Image not found.");

            var contentType = GetContentType(filePath);
            var fileBytes = System.IO.File.ReadAllBytes(filePath);

            return File(fileBytes, contentType, fileName);
        }

        private string GetContentType(string path)
        {
            var ext = Path.GetExtension(path).ToLowerInvariant();
            return ext switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                _ => "application/octet-stream",
            };
        }
    }
}
