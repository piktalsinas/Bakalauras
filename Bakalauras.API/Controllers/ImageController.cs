using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Linq;

namespace Bakalauras.API.Controllers
{
    [ApiController]
    [Route("images")]
    public class ImageController : ControllerBase
    {
        private readonly string _nodeImagesDirectory = @"C:\Users\picom\Documents\BAKALAURAS\Nodes";
        private readonly string _connectionImagesDirectory = @"C:\Users\picom\Documents\BAKALAURAS\ConnectionPhotos";
        private readonly string _baseNodeImagesDirectory = @"C:\Users\picom\Documents\BAKALAURAS\BaseNodes";

        [HttpGet("{fileName}")]
        public IActionResult GetImage(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return BadRequest("Filename is required.");

            
            string selectedDirectory = fileName.Count(c => c == '_') >= 3
                ? _connectionImagesDirectory
                : fileName.Length > 2 && fileName.All(char.IsLetterOrDigit) 
                    ? _baseNodeImagesDirectory
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
