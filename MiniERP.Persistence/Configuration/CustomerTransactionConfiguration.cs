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
    public sealed class CustomerTransactionConfiguration : FinancialTransactionConfiguration<CustomerTransaction>
    {
        public override void Configure(EntityTypeBuilder<CustomerTransaction> builder)
        {
            base.Configure(builder);

            builder.ToTable("CustomerTransactions");

            builder.HasOne(x => x.Customer)
                   .WithMany()
                   .HasForeignKey(x => x.CustomerId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
