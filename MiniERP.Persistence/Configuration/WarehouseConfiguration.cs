using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiniERP.Domain.Entities;

namespace MiniERP.Persistence.Configuration
{
    public class WarehouseConfiguration : IEntityTypeConfiguration<Warehouse>
    {
        public void Configure(EntityTypeBuilder<Warehouse> builder)
        {
            builder.Property(w => w.Code).HasMaxLength(20).IsRequired();
            builder.Property(w => w.Name).HasMaxLength(100).IsRequired();
            builder.Property(w => w.Location).HasMaxLength(250);
            builder.HasIndex(w => w.Code).IsUnique();
            builder.HasQueryFilter(w => !w.IsDeleted);
        }
    }
}
