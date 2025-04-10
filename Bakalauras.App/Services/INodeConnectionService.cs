using Bakalauras.Domain.Models;

namespace Bakalauras.App.Services;

public interface INodeConnectionService
{
    Task<bool> MoveImageToPathFolderAsync(Guid nodeConnectionId);
    Task<bool> CopyPathImagesAsync(List<Node> path);
}
