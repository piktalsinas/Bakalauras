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
    }
}
