using Microsoft.VisualBasic;

namespace Bakalauras.Domain.Models;

public class NodeConnection : TrackingEntity
{
	public Guid Id { get; set; }

	public Guid FromNodeId { get; set; }
	public virtual Node FromNode { get; set; } = default!;

	public Guid ToNodeId { get; set; }
	public virtual Node ToNode { get; set; } = default!;

	public float Weight { get; set; }
    public string Name { get; set; } = string.Empty;


}
