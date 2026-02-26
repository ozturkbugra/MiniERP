using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiniERP.Domain.Entities;

namespace MiniERP.Persistence.Configuration
{
    public abstract class FinancialTransactionConfiguration<T> : IEntityTypeConfiguration<T> where T : FinancialTransaction
    {
        public virtual void Configure(EntityTypeBuilder<T> builder)
        {
            builder.Property(x => x.Debit).HasColumnType("decimal(18,2)");
            builder.Property(x => x.Credit).HasColumnType("decimal(18,2)");
            builder.Property(x => x.Description).HasMaxLength(500);
        }
    }
}
