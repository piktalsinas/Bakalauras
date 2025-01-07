using Bakalauras.App.Services;
using Bakalauras.Domain.Models;
using Bakalauras.Persistence.Repositories;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
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

        // Endpoint to add nodes from images in the folder
        [HttpGet("add-nodes-from-images")]
        public async Task<IActionResult> AddNodesFromImages()
        {
            string folderPath = @"C:\Users\picom\Documents\BAKALAURAS\photos"; // Your images folder path
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

        [HttpPost("MoveImage/{id}")]
        public async Task<ActionResult> MoveImageToAnswerPic(Guid id)
        {
            var node = await _nodeRepository.GetByIdAsync(id);

            if (node == null)
            {
                return NotFound("Node not found.");
            }

            // Call NodeService to move the image
            var success = await _nodeService.MoveImageToAnswerPicFolderAsync(id);

            if (success)
            {
                return Ok("Image successfully moved to AnswerPic folder.");
            }

            return BadRequest("Failed to move the image. Please check if the image exists.");
        }
    }
}

