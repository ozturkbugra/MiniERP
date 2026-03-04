using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiniERP.Domain.Entities;

namespace MiniERP.Persistence.Configuration
{
    public sealed class InvoiceDetailConfiguration : IEntityTypeConfiguration<InvoiceDetail>
    {
        public void Configure(EntityTypeBuilder<InvoiceDetail> builder)
        {
            builder.ToTable("InvoiceDetails");
            builder.HasKey(x => x.Id);

            // Parasal ve oransal alanların veri tipleri
            builder.Property(x => x.Quantity).HasColumnType("decimal(18,2)");
            builder.Property(x => x.UnitPrice).HasColumnType("decimal(18,2)");
            builder.Property(x => x.DiscountRate).HasColumnType("decimal(18,2)");
            builder.Property(x => x.VatRate).HasColumnType("decimal(18,2)");
            builder.Property(x => x.LineTotal).HasColumnType("decimal(18,2)");

            // BaseEntity filtresi
            builder.HasQueryFilter(x => !x.IsDeleted);
        }
    }
}
