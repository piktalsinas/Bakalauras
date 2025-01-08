namespace Bakalauras.Domain.Models;

public class Node
{
	public Guid Id { get; set; }
	public string Name { get; set; } = default!;
	
    public virtual List<NodeConnection> OutgoingConnections { get; set; } = new();
	public virtual List<NodeConnection> IncomingConnections { get; set; } = new();
}
