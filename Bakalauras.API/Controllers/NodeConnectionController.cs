using Bakalauras.App.Services;
using Bakalauras.Domain.Models;
using Bakalauras.Persistence.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bakalauras.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class NodeConnectionController : ControllerBase
    {
        private readonly ILogger<NodeConnectionController> _logger;
        private readonly INodeConnectionRepository _nodeConnectionRepository;
        private readonly NodeConnectionService _nodeConnectionService;

        public NodeConnectionController(ILogger<NodeConnectionController> logger, INodeConnectionRepository nodeConnectionRepository, NodeConnectionService nodeConnectionService)
        {
            _logger = logger;
            _nodeConnectionRepository = nodeConnectionRepository;
            _nodeConnectionService = nodeConnectionService;
        }

        [HttpGet(Name = "GetAllNodeConnections")]
        public async Task<ActionResult<IEnumerable<NodeConnection>>> GetAll()
        {
            return Ok(await _nodeConnectionRepository.GetAllAsync());
        }

        [HttpPost(Name = "PostNodeConnection")]
        public async Task<ActionResult<NodeConnection>> Post([FromQuery] Guid fromNodeId, [FromQuery] Guid toNodeId, [FromQuery] float weight)
        {
            NodeConnection nodeConnection = new()
            {
                FromNodeId = fromNodeId,
                ToNodeId = toNodeId,
                Weight = weight
            };

            // The Name will be automatically generated in the repository
            NodeConnection? addedNodeConnection = await _nodeConnectionRepository.AddAsync(nodeConnection);
            if (addedNodeConnection is null)
            {
                return BadRequest("Node connection already exists or nodes not found.");
            }

            return Ok(addedNodeConnection);
        }

        [HttpPost("CopyImage/{id}")]
        public async Task<IActionResult> CopyImageToPathFolderAsync(Guid id)
        {
            try
            {
                bool success = await _nodeConnectionService.MoveImageToPathFolderAsync(id);
                if (success)
                {
                    return Ok("Image successfully copied to Path folder.");
                }

                return NotFound("Image not found or an error occurred.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while copying image file for NodeConnection {NodeConnectionId}.", id);
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }
    }
}
