using MiniERP.Domain.Common;
using MiniERP.Domain.Enums;

namespace MiniERP.Domain.Entities
{
    public sealed class StockTransaction : BaseEntity
    {
        public string DocumentNo { get; set; } // Fiş veya Fatura No (Örn: INV-20260001)
        public DateTime TransactionDate { get; set; }
        public decimal Quantity { get; set; } // Miktar
        public decimal UnitPrice { get; set; } // Hareket anındaki birim fiyat (Maliyet analizi için şart)
        public string? Description { get; set; }
        public StockTransactionType Type { get; set; } // Giriş mi? Çıkış mı?

        public Guid TransactionId { get; set; }
        public PaymentType PaymentType { get; set; }

        public Guid ProductId { get; set; }
        public Product? Product { get; set; }
        public Guid WarehouseId { get; set; }
        public Warehouse? Warehouse { get; set; }
        public Guid CustomerId { get; set; }
        public Customer? Customer { get; set; }
    }
}
