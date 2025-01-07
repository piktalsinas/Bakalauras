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

		modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
	}
}
