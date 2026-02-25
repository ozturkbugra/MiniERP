using MiniERP.Domain.Common;
using MiniERP.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniERP.Domain.Entities
{
    public sealed class Transaction : BaseEntity
    {
        public DateTime Date { get; set; } = DateTime.Now;
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public TransactionType TransactionType { get; set; }


        // İlişkiler - Nullable olmalarının sebebi her hareketin hem cari hem banka içermeyebilmesidir
        public Guid? CustomerId { get; set; }
        public Customer? Customer { get; set; }

        public Guid? CashId { get; set; }
        public Cash? Cash { get; set; }

        public Guid? BankId { get; set; }
        public Bank? Bank { get; set; }
    }
}
