using System.Text.Json.Serialization;

namespace Bakalauras.Domain.Models;

public class Node
{
	public Guid Id { get; set; }
	public string Name { get; set; } = default!;
    [JsonIgnore]
    public virtual List<NodeConnection> OutgoingConnections { get; set; } = new();
    [JsonIgnore]
    public virtual List<NodeConnection> IncomingConnections { get; set; } = new();
    public Guid? ParentId { get; set; }
    [JsonIgnore]
    public BaseNode? Parent { get; set; }
    public string? ParentName { get; set; }
}
