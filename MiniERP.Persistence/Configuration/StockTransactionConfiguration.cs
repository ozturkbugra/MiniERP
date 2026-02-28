using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiniERP.Domain.Entities;

public class StockTransactionConfiguration : IEntityTypeConfiguration<StockTransaction>
{
    public void Configure(EntityTypeBuilder<StockTransaction> builder)
    {
        builder.Property(s => s.DocumentNo).HasMaxLength(50).IsRequired();
        builder.Property(s => s.Description).HasMaxLength(500);

        builder.Property(s => s.Quantity).HasPrecision(18, 2);
        builder.Property(s => s.UnitPrice).HasPrecision(18, 2);

        // Cari (Customer) İlişkisi
        builder.HasOne(s => s.Customer)
               .WithMany() // Customer tarafında ICollection<StockTransaction> açmadıysak boş bırakılır
               .HasForeignKey(s => s.CustomerId)
               .OnDelete(DeleteBehavior.Restrict); // Cari silinirse hareketler silinmesin!

        // Depo İlişkisi
        builder.HasOne(s => s.Warehouse)
               .WithMany()
               .HasForeignKey(s => s.WarehouseId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.Product)
                   .WithMany()
                   .HasForeignKey(s => s.ProductId)
                   .OnDelete(DeleteBehavior.Restrict);
    }
}