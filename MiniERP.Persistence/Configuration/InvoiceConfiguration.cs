using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiniERP.Domain.Entities;

namespace MiniERP.Persistence.Configuration
{
    public sealed class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
    {
        public void Configure(EntityTypeBuilder<Invoice> builder)
        {
            builder.ToTable("Invoices");
            builder.HasKey(x => x.Id);

            // Fatura Numarası asla tekrar edemez (Concurrency & Integrity)
            builder.HasIndex(x => x.InvoiceNumber).IsUnique();
            builder.Property(x => x.InvoiceNumber).IsRequired().HasMaxLength(50);

            // Parasal alanların veritabanı tipleri kesin olarak decimal(18,2) ayarlanıyor
            builder.Property(x => x.TotalGross).HasColumnType("decimal(18,2)");
            builder.Property(x => x.TotalDiscount).HasColumnType("decimal(18,2)");
            builder.Property(x => x.TotalVat).HasColumnType("decimal(18,2)");
            builder.Property(x => x.GrandTotal).HasColumnType("decimal(18,2)");

            // Bire-Çok İlişki (Bir faturanın birden fazla satırı olur)
            builder.HasMany(x => x.Details)
                   .WithOne(x => x.Invoice)
                   .HasForeignKey(x => x.InvoiceId)
                   .OnDelete(DeleteBehavior.Cascade);

            // BaseEntity'den gelen IsDeleted için global filtre (Silinenleri getirme)
            builder.HasQueryFilter(x => !x.IsDeleted);
        }
    }
}
