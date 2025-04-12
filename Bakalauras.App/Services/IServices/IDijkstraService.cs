using Bakalauras.Domain.Models;

namespace Bakalauras.App.Services.IServices;

public interface IDijkstraService
{
    Task<List<Node>> FindShortestPathAsync(Guid startNodeId, Guid endNodeId);
}
