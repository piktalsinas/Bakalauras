namespace Bakalauras.Domain.Models;

public class BaseNode : TrackingEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    // public Guid? ParentNodeId { get; set; }

}