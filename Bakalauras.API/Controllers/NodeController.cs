using Bakalauras.App.Services;
using Bakalauras.Domain.Models;
using Bakalauras.Persistence.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Bakalauras.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class NodeController : ControllerBase
    {
        private readonly ILogger<NodeController> _logger;
        private readonly INodeRepository _nodeRepository;
        private readonly NodeService _nodeService;

        public NodeController(ILogger<NodeController> logger, INodeRepository nodeRepository, NodeService nodeService)
        {
            _logger = logger;
            _nodeRepository = nodeRepository;
            _nodeService = nodeService;
        }

        [HttpGet(Name = "GetAllNodes")]
        public async Task<ActionResult<IEnumerable<Node>>> GetAll()
        {
            return Ok(await _nodeRepository.GetAllAsync());
        }

        [HttpPost(Name = "PostNode")]
        public async Task<ActionResult<Node>> Post([FromQuery] string name)
        {
            Node node = new() { Name = name };
            Node? addedNode = await _nodeRepository.AddAsync(node);

            if (addedNode is null)
            {
                return BadRequest("Node already exists");
            }

            return Ok(addedNode);
        }

        [HttpPut("{id}", Name = "PutNode")]
        public async Task<ActionResult<Node>> Put(Guid id, [FromQuery] string name)
        {
            Node? node = await _nodeRepository.GetByIdAsync(id);
            if (node is null)
            {
                return NotFound();
            }
            node.Name = name;
            Node? updatedNode = await _nodeRepository.UpdateAsync(node);
            if (updatedNode is null)
            {
                return BadRequest("Node already exists");
            }
            return Ok(updatedNode);
        }

        [HttpDelete("{id}", Name = "DeleteNode")]
        public async Task<ActionResult> Delete(Guid id)
        {
            await _nodeRepository.DeleteAsync(id);
            return NoContent();
        }

        [HttpGet("{id}", Name = "GetNode")]
        public async Task<ActionResult<Node>> Get(Guid id)
        {
            Node? node = await _nodeRepository.GetByIdAsync(id);
            if (node is null)
            {
                return NotFound();
            }
            return Ok(node);
        }

        [HttpPost("add-nodes-from-images")]
        public async Task<IActionResult> AddNodesFromImages()
        {
            string folderPath = @"C:\Users\picom\Documents\BAKALAURAS\photos";
            try
            {
                await _nodeService.AddNodesFromImagesAsync(folderPath);
                return Ok("Nodes added successfully from images.");
            }
            catch (Exception ex)
            {
                return BadRequest($"An error occurred: {ex.Message}");
            }
        }

        [HttpPost("CopyImage/{id}")]
        public async Task<IActionResult> CopyImageToPathFolderAsync(Guid id)
        {
            try
            {
                Node? node = await _nodeRepository.GetByIdAsync(id);
                if (node == null || string.IsNullOrEmpty(node.Name))
                {
                    _logger.LogError("Node with ID {NodeId} not found or has no associated name.", id);
                    return NotFound("Node not found or has no associated name.");
                }

                string sourceFolder = @"C:\Users\picom\Documents\BAKALAURAS\photos";
                string destinationFolder = @"C:\Users\picom\Documents\BAKALAURAS\Path";
                string sourceFilePath = Path.Combine(sourceFolder, $"{node.Name}.jpg");
                string destinationFilePath = Path.Combine(destinationFolder, $"{node.Name}.jpg");

                if (!System.IO.File.Exists(sourceFilePath))
                {
                    _logger.LogError("Image file {SourceFilePath} not found for node {NodeName}.", sourceFilePath, node.Name);
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
                _logger.LogError(ex, "Error occurred while copying image file for node {NodeId}.", id);
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }
    }
}
