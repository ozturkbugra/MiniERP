using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiniERP.Domain.Entities;

namespace MiniERP.Persistence.Configuration
{
    public sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.ToTable("Orders");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.OrderNumber)
                .IsRequired()
                .HasMaxLength(50);

            // Sipariş numarası benzersiz
            builder.HasIndex(x => x.OrderNumber).IsUnique();

            // Header -> Line İlişkisi (Bire-Çok)
            // Sipariş silinirse tüm satırları da beraberinde silinsin (Cascade)
            builder.HasMany(x => x.OrderDetails)
                .WithOne(x => x.Order)
                .HasForeignKey(x => x.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Property(x => x.Description).HasMaxLength(500);
        }
    }
}
