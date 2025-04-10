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
        private readonly INodeConnectionService _nodeConnectionService;
        //private readonly NodeConnectionService _nodeConnectionService;
        //private readonly DijkstraService _dijkstraService;
        private readonly IDijkstraService _dijkstraService;
        private readonly INodeNameSevice _nodeNameService;

        public NodeConnectionController(ILogger<NodeConnectionController> logger, INodeConnectionRepository nodeConnectionRepository, INodeConnectionService nodeConnectionService, IDijkstraService dijkstraService, INodeNameSevice nodeNameService)
        {
            _logger = logger;
            _nodeConnectionRepository = nodeConnectionRepository;
            _nodeConnectionService = nodeConnectionService;
            _dijkstraService = dijkstraService;
            _nodeNameService = nodeNameService;
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

        [HttpGet("shortest-path")]
        public async Task<ActionResult<List<Node>>> GetShortestPath(
        [FromQuery] string fullName1, [FromQuery] string fullName2)
        {
            var startNodeId = await _nodeNameService.GetNodeIdByFullName(fullName1);
            var endNodeId = await _nodeNameService.GetNodeIdByFullName(fullName2);

            if (startNodeId == null || endNodeId == null)
            {
                return NotFound("One or both nodes not found.");
            }

            var path = await _dijkstraService.FindShortestPathAsync(startNodeId.Value, endNodeId.Value);

            if (path == null || path.Count == 0)
            {
                return NotFound("No path found between the specified nodes.");
            }

            bool copied = await _nodeConnectionService.CopyPathImagesAsync(path);
            if (!copied)
            {
                return StatusCode(500, "Path images could not be copied.");
            }

            return Ok(path);
        }

    }


}
