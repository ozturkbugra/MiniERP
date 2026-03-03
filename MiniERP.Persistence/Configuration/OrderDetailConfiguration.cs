using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiniERP.Domain.Entities;

namespace MiniERP.Persistence.Configuration
{
    public sealed class OrderDetailConfiguration : IEntityTypeConfiguration<OrderDetail>
    {
        public void Configure(EntityTypeBuilder<OrderDetail> builder)
        {
            builder.ToTable("OrderDetails");

            builder.HasKey(x => x.Id);

            // Dinamik rezervasyon hesaplamasında Product ve Warehouse üzerinden 
            // sürekli sorgu atacağımız için bu ikiliyi indeksliyoruz.
            builder.HasIndex(x => new { x.ProductId, x.WarehouseId });

            // Ürün ve Depo bağlantıları
            // NoAction: Ürün veya Depo yanlışlıkla silinmesin, sipariş varsa engellesin!
            builder.HasOne(x => x.Product)
                .WithMany()
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.Warehouse)
                .WithMany()
                .HasForeignKey(x => x.WarehouseId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Property(x => x.Quantity).HasPrecision(18, 4);
            builder.Property(x => x.UnitPrice).HasPrecision(18, 4);
        }
    }
}
