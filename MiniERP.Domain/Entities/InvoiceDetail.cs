using MiniERP.Domain.Common;

namespace MiniERP.Domain.Entities
{
    public sealed class InvoiceDetail : BaseEntity
    {
        public Guid InvoiceId { get; private set; }
        public Guid ProductId { get; private set; }
        public Guid WarehouseId { get; private set; }

        public decimal Quantity { get; private set; }
        public decimal UnitPrice { get; private set; }
        public decimal DiscountRate { get; private set; }
        public decimal VatRate { get; private set; }
        public decimal LineTotal { get; private set; }

        public Invoice? Invoice { get; private set; }

        private InvoiceDetail() { } // EF Core için

        public InvoiceDetail(Guid productId, Guid warehouseId, decimal quantity, decimal unitPrice, decimal discountRate, decimal vatRate)
        {
            ProductId = productId;
            WarehouseId = warehouseId;
            Quantity = quantity;
            UnitPrice = unitPrice;
            DiscountRate = discountRate;
            VatRate = vatRate;

            CalculateLineTotal();
        }

        public void CalculateLineTotal()
        {
            decimal gross = Quantity * UnitPrice;
            decimal discountAmount = gross * DiscountRate;
            decimal net = gross - discountAmount;
            decimal vatAmount = net * VatRate;

            // Finansal tutarlılık için satır bazlı yuvarlama (ERP Standardı)
            LineTotal = Math.Round(net + vatAmount, 2, MidpointRounding.AwayFromZero);
        }
    }
}
