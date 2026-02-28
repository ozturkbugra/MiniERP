using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiniERP.Domain.Entities;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.Property(p => p.Code).HasMaxLength(30).IsRequired();
        builder.Property(p => p.Name).HasMaxLength(150).IsRequired();
        builder.Property(p => p.Barcode).HasMaxLength(50);

        // Sayısal Alanlar (Decimal 18,2)
        builder.Property(p => p.DefaultPrice).HasPrecision(18, 2);
        builder.Property(p => p.CriticalStockLevel).HasPrecision(18, 2);
        builder.Property(p => p.VatRate).HasPrecision(18, 2);

        // Benzersizlik Kuralları
        builder.HasIndex(p => p.Code).IsUnique();
        builder.HasIndex(p => p.Barcode).IsUnique().HasFilter("[Barcode] IS NOT NULL");

        // İlişkiler
        builder.HasOne(p => p.Category).WithMany(c => c.Products).HasForeignKey(p => p.CategoryId);
        builder.HasOne(p => p.Brand).WithMany(b => b.Products).HasForeignKey(p => p.BrandId);
        builder.HasOne(p => p.Unit).WithMany(u => u.Products).HasForeignKey(p => p.UnitId);
    }
}