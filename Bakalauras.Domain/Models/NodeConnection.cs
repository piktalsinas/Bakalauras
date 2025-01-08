namespace Bakalauras.Domain.Models;

public class NodeConnection
{
	public Guid Id { get; set; }

	public Guid FromNodeId { get; set; }
	public virtual Node FromNode { get; set; } = default!;

	public Guid ToNodeId { get; set; }
	public virtual Node ToNode { get; set; } = default!;

	public float Weight { get; set; }

}