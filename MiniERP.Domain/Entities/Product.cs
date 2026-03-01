using MiniERP.Domain.Common;

namespace MiniERP.Domain.Entities
{
    public sealed class Product : BaseEntity
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string Barcode { get; set; }

        public decimal DefaultPrice { get; set; }
        public decimal CriticalStockLevel { get; set; }
        public decimal VatRate { get; set; } // KDV Oranı (Örn: 20)

        // Foreign Keys & Navigation Properties
        public Guid CategoryId { get; set; }
        public Category Category { get; set; }

        public Guid BrandId { get; set; }
        public Brand Brand { get; set; }

        public Guid UnitId { get; set; }
        public Unit Unit { get; set; }

        public Guid WarehouseId { get; set; }
        public Warehouse Warehouse { get; set; }
    }
}