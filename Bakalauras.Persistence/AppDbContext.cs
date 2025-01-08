using Bakalauras.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Bakalauras.Persistence;

public class AppDbContext : DbContext
{
	public DbSet<Node> Nodes { get; set; } = default!;
	public DbSet<NodeConnection> NodeConnections { get; set; } = default!;

	public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
	{
		Database.EnsureCreated();
	}

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // If you want to configure Name column or any other custom behavior
        modelBuilder.Entity<NodeConnection>()
            .Property(nc => nc.Name)
            .HasMaxLength(100); // Example: limit the length of the Name

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
