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
        [HttpGet("get-nodes-by-base-node")]
        public async Task<ActionResult<IEnumerable<Node>>> GetNodesByBaseNode([FromQuery] string baseNodeName)
        {
            if (string.IsNullOrEmpty(baseNodeName))
            {
                return BadRequest("BaseNode name is required.");
            }

            var nodes = await _nodeRepository.GetNodesByBaseNodeAsync(baseNodeName);

            if (nodes == null || !nodes.Any())
            {
                return NotFound($"No nodes found with BaseNode '{baseNodeName}' as their parent.");
            }

            return Ok(nodes);
        }

        [HttpPost(Name = "PostNode")]
        public async Task<ActionResult<Node>> Post([FromQuery] string name)
        {
            Node node = new() { Name = name };
            Node? addedNode = await _nodeRepository.AddAsync(node);

           /* if (addedNode is null)
            {
                return BadRequest("Node already exists");
            }*/

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
            string folderPath = @"C:\\Users\\picom\\Documents\\BAKALAURAS\\Nodes";
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

        [HttpPost("CopyImage/{parentNodeName}_{nodeName}")]
        public async Task<IActionResult> CopyImageToPathFolderAsync(string parentNodeName, string nodeName)
        {
            try
            {
                // Fetch the node by name
                //  Node? node = await _nodeRepository.GetByNameAsync(nodeName);
                Node? node = (await _nodeRepository.GetByNameAsync(nodeName)).FirstOrDefault();

            if (node == null || string.IsNullOrEmpty(node.Name))
            {
                _logger.LogError("Node with name {NodeName} not found.", nodeName);
                return NotFound("Node not found.");
            }

            if (node.ParentName == null || !node.ParentName.Equals(parentNodeName, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogError("Parent node name mismatch for node {NodeName}. Expected: {ExpectedParent}, Provided: {ProvidedParent}", nodeName, node.ParentName, parentNodeName);
                return BadRequest("Invalid parent node name.");
            }

            string sourceFolder = @"C:\\Users\\picom\\Documents\\BAKALAURAS\\Nodes";
                string destinationFolder = @"C:\\Users\\picom\\Documents\\BAKALAURAS\\NodesPath";

                string sourceFileName = $"{parentNodeName}_{node.Name}.jpg";
                string sourceFilePath = Path.Combine(sourceFolder, sourceFileName);
                string destinationFilePath = Path.Combine(destinationFolder, sourceFileName);

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
                _logger.LogError(ex, "Error occurred while copying image file for node {NodeName}.", nodeName);
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }



        [HttpPut("UpdateNodeParent/{id}")]
        public async Task<ActionResult<Node>> UpdateNodeParent(Guid id, [FromQuery] Guid? parentId)
        {
            try
            {
                Node? node = await _nodeRepository.GetByIdAsync(id);
                if (node == null)
                {
                    return NotFound($"Node with ID {id} not found.");
                }

                if (parentId.HasValue)
                {
                    var parentNode = await _nodeRepository.GetParentByIdAsync(parentId.Value);
                    if (parentNode == null)
                    {
                        return NotFound($"Parent with ID {parentId.Value} not found.");
                    }

                    node.ParentId = parentNode.Id; 
                    node.ParentName = parentNode.Name;  
                }
                else
                {
                    node.ParentId = null;
                    node.ParentName = null;
                }

                Node? updatedNode = await _nodeRepository.UpdateAsync(node);
                if (updatedNode == null)
                {
                    return BadRequest("Failed to update the node.");
                }

                return Ok(updatedNode);
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred while updating the node: {ex.Message}");
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

    }

    }
