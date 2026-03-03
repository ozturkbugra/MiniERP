using MiniERP.Domain.Common;

namespace MiniERP.Domain.Entities
{
    public sealed class OrderDetail : BaseEntity
    {
        public Guid OrderId { get; set; }
        public Order? Order { get; set; }

        public Guid ProductId { get; set; }
        public Product? Product { get; set; }

        // Multi-Warehouse Anayasası: Depo satır bazında seçilir!
        public Guid WarehouseId { get; set; }
        public Warehouse? Warehouse { get; set; }

        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }

        // Veritabanında yer kaplamayan, anlık hesaplanan (Sanal) kolon
        public decimal LineTotal => Quantity * UnitPrice;
    }
}
