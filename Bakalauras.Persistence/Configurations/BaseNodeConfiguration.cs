using Bakalauras.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bakalauras.Persistence.Configurations;

public class BaseNodeConfiguration : IEntityTypeConfiguration<BaseNode>
    {
        public void Configure(EntityTypeBuilder<BaseNode> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name).IsRequired();
            builder.Property(x => x.Name).HasMaxLength(100);
            builder.HasIndex(x => x.Name).IsUnique();
        }
    }


