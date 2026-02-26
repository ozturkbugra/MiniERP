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
    public sealed class BankTransactionConfiguration : FinancialTransactionConfiguration<BankTransaction>
    {
        public override void Configure(EntityTypeBuilder<BankTransaction> builder)
        {
            base.Configure(builder);

            builder.ToTable("BankTransactions");

            builder.HasOne(x => x.Bank)
                   .WithMany()
                   .HasForeignKey(x => x.BankId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
