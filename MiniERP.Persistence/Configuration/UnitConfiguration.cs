using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiniERP.Domain.Entities;

namespace MiniERP.Persistence.Configuration
{
    public class UnitConfiguration : IEntityTypeConfiguration<Unit>
    {
        public void Configure(EntityTypeBuilder<Unit> builder)
        {
            builder.Property(u => u.Code).HasMaxLength(10).IsRequired();
            builder.Property(u => u.Name).HasMaxLength(50).IsRequired();
            builder.HasIndex(u => u.Code).IsUnique();
        }
    }
}
