using MiniERP.Domain.Common;
using MiniERP.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniERP.Domain.Entities
{
    public sealed class Cash : BaseEntity
    {
        public string Name { get; set; } = string.Empty; // Örn: Merkez Kasa, TL Kasası
        public CurrencyType CurrencyType { get; set; } // Hangi para birimi?

    }
}
