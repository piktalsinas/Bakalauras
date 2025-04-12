using Bakalauras.Domain.Models;

namespace Bakalauras.App.Services.IServices;
public interface INodeConnectionService
{
    Task<bool> MoveImageToPathFolderAsync(Guid nodeConnectionId);
    Task<bool> CopyPathImagesAsync(List<Node> path);
}
