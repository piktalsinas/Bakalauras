using Bakalauras.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bakalauras.Persistence.Configurations;

class NodeConfiguration : IEntityTypeConfiguration<Node>
{
	public void Configure(EntityTypeBuilder<Node> builder)
	{
		builder.HasKey(x => x.Id);

		builder.Property(x => x.Name).IsRequired();
		builder.Property(x => x.Name).HasMaxLength(100);
		builder.HasIndex(x => x.Name).IsUnique();
	}
}
