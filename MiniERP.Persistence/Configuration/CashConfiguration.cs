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
    public class CashConfiguration : IEntityTypeConfiguration<Cash>
    {
        public void Configure(EntityTypeBuilder<Cash> builder)
        {
            builder.HasKey(c => c.Id);

            builder.Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(c => c.CurrencyType)
                .IsRequired();

            builder.HasQueryFilter(c => !c.IsDeleted);
        }
    }
}
