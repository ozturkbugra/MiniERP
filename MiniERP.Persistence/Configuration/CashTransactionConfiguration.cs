using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiniERP.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniERP.Persistence.Configuration
{
    public sealed class CashTransactionConfiguration : FinancialTransactionConfiguration<CashTransaction>
    {
        public override void Configure(EntityTypeBuilder<CashTransaction> builder)
        {
            base.Configure(builder); // Ata sınıfın decimal ayarlarını çağır

            builder.ToTable("CashTransactions"); // Tablo adını netleştir

            // İlişki: Kasa silinirse hareketleri SİLİNMESİN (Veri Güvenliği)
            builder.HasOne(x => x.Cash)
                   .WithMany()
                   .HasForeignKey(x => x.CashId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
