using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiniERP.Domain.Entities;

namespace MiniERP.Persistence.Configuration
{
    public class BankConfiguration : IEntityTypeConfiguration<Bank>
    {
        public void Configure(EntityTypeBuilder<Bank> builder)
        {
            builder.HasKey(b => b.Id);

            builder.Property(b => b.BankName)
                .IsRequired()
                .HasMaxLength(100);

            // Hesap Adı (Mevduat, Maaş Hesabı vb.)
            builder.Property(b => b.AccountName)
                .IsRequired()
                .HasMaxLength(150);

        
            builder.Property(b => b.IBAN)
                .IsRequired()
                .HasMaxLength(34); // Uluslararası standartlarda max 34

            // Şube Adı
            builder.Property(b => b.BranchName)
                .HasMaxLength(100);

            // Para Birimi
            builder.Property(b => b.CurrencyType)
                .IsRequired();

            // Soft Delete: Silinen banka hesapları sorgularda gelmesin
            builder.HasQueryFilter(b => !b.IsDeleted);

            // Veritabanı seviyesinde IBAN benzersiz olacak
            builder.HasIndex(b => b.IBAN).IsUnique();
        }
    }
}
