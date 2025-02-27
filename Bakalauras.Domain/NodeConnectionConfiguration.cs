using Bakalauras.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bakalauras.Persistence.Configurations;

class NodeConnectionConfiguration : IEntityTypeConfiguration<NodeConnection>
{
	public void Configure(EntityTypeBuilder<NodeConnection> builder)
	{
		builder.HasKey(x => x.Id);

		builder.HasOne(x => x.FromNode)
			.WithMany(x => x.OutgoingConnections)
			.HasForeignKey(x => x.FromNodeId)
			.OnDelete(DeleteBehavior.Cascade);

		builder.HasOne(x => x.ToNode)
			.WithMany(x => x.IncomingConnections)
			.HasForeignKey(x => x.ToNodeId)
			.OnDelete(DeleteBehavior.Restrict);

		builder.Property(x => x.Weight).IsRequired();
	}
}
