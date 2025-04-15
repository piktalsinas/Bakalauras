using Bakalauras.App.Services;
using Bakalauras.Domain.Models;
using Bakalauras.Persistence.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Bakalauras.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BaseNodeController : ControllerBase
    {
        private readonly ILogger<BaseNodeController> _logger;
        private readonly IBaseNodeRepository _baseNodeRepository;
        private readonly BaseNodeService _baseNodeService;

        public BaseNodeController(ILogger<BaseNodeController> logger, IBaseNodeRepository baseNodeRepository, BaseNodeService baseNodeService)
        {
            _logger = logger;
            _baseNodeRepository = baseNodeRepository;
            _baseNodeService = baseNodeService;
        }

        [HttpPost(Name = "PostBaseNode")]
        public async Task<ActionResult<BaseNode>> Post([FromQuery] string name)
        {
            var baseNode = await _baseNodeService.AddBaseNodeAsync(name);
            if (baseNode is null)
            {
                return BadRequest("BaseNode with this name already exists.");
            }
            return Ok(baseNode);
        }

        [HttpPost("add-base-nodes-from-images")]
        public async Task<IActionResult> AddBaseNodesFromImages()
        {
            string folderPath = @"C:\\Users\\picom\\Documents\\BAKALAURAS\\BaseNodes";
            try
            {
                await _baseNodeService.AddBaseNodesFromImagesAsync(folderPath);
                return Ok("BaseNodes added successfully from images.");
            }
            catch (Exception ex)
            {
                return BadRequest($"An error occurred: {ex.Message}");
            }
        }

        [HttpPost("CopyImage/{baseNodeName}")]
        public async Task<IActionResult> CopyBaseNodeImageToPathFolderAsync(string baseNodeName)
        {
            try
            {
                var baseNode = await _baseNodeRepository.GetByNameAsync(baseNodeName);


                if (baseNode == null || string.IsNullOrEmpty(baseNode.Name))
                {
                    _logger.LogError("BaseNode with name {BaseNodeName} not found.", baseNodeName);
                    return NotFound("BaseNode not found.");
                }

                string sourceFolder = @"C:\\Users\\picom\\Documents\\BAKALAURAS\\BaseNodes";
                string destinationFolder = @"C:\\Users\\picom\\Documents\\BAKALAURAS\\BaseNodesPath";

                string sourceFileName = $"{baseNode.Name}.jpg";
                string sourceFilePath = Path.Combine(sourceFolder, sourceFileName);
                string destinationFilePath = Path.Combine(destinationFolder, sourceFileName);

                if (!System.IO.File.Exists(sourceFilePath))
                {
                    _logger.LogError("Image file {SourceFilePath} not found for BaseNode {BaseNodeName}.", sourceFilePath, baseNode.Name);
                    return NotFound("Image file not found.");
                }

                if (!Directory.Exists(destinationFolder))
                {
                    Directory.CreateDirectory(destinationFolder);
                }

                System.IO.File.Copy(sourceFilePath, destinationFilePath, overwrite: true);

                _logger.LogInformation("Image file copied successfully from {Source} to {Destination}.", sourceFilePath, destinationFilePath);
                return Ok("Image successfully copied to Path folder.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while copying image file for BaseNode {BaseNodeName}.", baseNodeName);
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }
    }
}
