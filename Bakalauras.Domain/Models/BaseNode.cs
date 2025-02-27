namespace Bakalauras.Domain.Models;

public class BaseNode
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
   /* public Guid? ParentId { get; set; }
    public BaseNode? Parent { get; set; }*/


}