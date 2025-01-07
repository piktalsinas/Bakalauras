using Bakalauras.Domain.Models;
using Bakalauras.Persistence.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Bakalauras.API.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class NodeConnectionController : ControllerBase
	{
		private readonly ILogger<NodeConnectionController> _logger;
		private readonly INodeConnectionRepository _nodeConnectionRepository;

		public NodeConnectionController(ILogger<NodeConnectionController> logger, INodeConnectionRepository nodeConnectionRepository)
		{
			_logger = logger;
			_nodeConnectionRepository = nodeConnectionRepository;
		}

		[HttpGet(Name = "GetAllNodeConnections")]
		public async Task<ActionResult<IEnumerable<NodeConnection>>> GetAll()
		{
			return Ok(await _nodeConnectionRepository.GetAllAsync());
		}

		[HttpPost(Name = "PostNodeConnection")]
		public async Task<ActionResult<NodeConnection>> Post([FromQuery] Guid fromNodeId, [FromQuery] Guid toNodeId, [FromQuery] float weight)
		{
			NodeConnection nodeConnection = new() { FromNodeId = fromNodeId, ToNodeId = toNodeId, Weight = weight };
			NodeConnection? addedNodeConnection = await _nodeConnectionRepository.AddAsync(nodeConnection);
			if (addedNodeConnection is null)
			{
				return BadRequest("Node connection already exists");
			}
			return Ok(addedNodeConnection);
		}

		[HttpPut("{id}", Name = "PutNodeConnection")]
		public async Task<ActionResult<NodeConnection>> Put(Guid id, [FromQuery] Guid fromNodeId, [FromQuery] Guid toNodeId, [FromQuery] float weight)
		{
			NodeConnection? nodeConnection = await _nodeConnectionRepository.GetByIdAsync(id);
			if (nodeConnection is null)
			{
				return NotFound();
			}
			nodeConnection.FromNodeId = fromNodeId;
			nodeConnection.ToNodeId = toNodeId;
			nodeConnection.Weight = weight;
			NodeConnection? updatedNodeConnection = await _nodeConnectionRepository.UpdateAsync(nodeConnection);
			if (updatedNodeConnection is null)
			{
				return BadRequest("Node connection already exists");
			}
			return Ok(updatedNodeConnection);
		}

		[HttpDelete("{id}", Name = "DeleteNodeConnection")]
		public async Task<ActionResult> Delete(Guid id)
		{
			await _nodeConnectionRepository.DeleteAsync(id);
			return NoContent();
		}

		[HttpGet("{id}", Name = "GetNodeConnection")]
		public async Task<ActionResult<NodeConnection>> Get(Guid id)
		{
			NodeConnection? nodeConnection = await _nodeConnectionRepository.GetByIdAsync(id);
			if (nodeConnection is null)
			{
				return NotFound();
			}

			return Ok(nodeConnection);
		}

		[HttpGet("{id}/connections", Name = "GetNodeConnections")]
		public async Task<ActionResult<IEnumerable<NodeConnection>>> GetConnections(Guid id)
		{
			return Ok(await _nodeConnectionRepository.GetAllConnectionsByNodeIdAsync(id));
		}
	}
}