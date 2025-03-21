using Bakalauras.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Bakalauras.Persistence
{
    public class AppDbContext : DbContext
    {
        public DbSet<Node> Nodes { get; set; } = default!;
        public DbSet<NodeConnection> NodeConnections { get; set; } = default!;
        public DbSet<BaseNode> BaseNodes { get; set; } = default!;

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<NodeConnection>()
                .Property(nc => nc.Name)
                .HasMaxLength(100); // Example: limit the length of the Name

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }
    }
}
