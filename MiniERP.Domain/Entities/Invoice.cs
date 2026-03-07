using MiniERP.Domain.Common;
using MiniERP.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace MiniERP.Domain.Entities
{
    public sealed class Invoice : BaseEntity
    {
        public string InvoiceNumber { get; private set; }
        public DateTime InvoiceDate { get; private set; }
        public InvoiceType Type { get; private set; }
        public InvoiceStatus Status { get; private set; }
        public Guid CustomerId { get; private set; }
        public Guid? OrderId { get; private set; }

        public decimal TotalGross { get; private set; }
        public decimal TotalDiscount { get; private set; }
        public decimal TotalVat { get; private set; }
        public decimal GrandTotal { get; private set; }

        public Guid? TransactionId { get; private set; }
        public PaymentType? PaymentType { get; private set; }

        public Guid? WarehouseId { get; private set; }


        private readonly List<InvoiceDetail> _details = new();
        public IReadOnlyCollection<InvoiceDetail> Details => _details.AsReadOnly();

        private Invoice() { } // EF Core için

        public Invoice(string invoiceNumber, DateTime invoiceDate, InvoiceType type, Guid customerId, Guid? orderId = null, Guid? warehouseId = null)
        {
            InvoiceNumber = invoiceNumber;
            InvoiceDate = invoiceDate;
            Type = type;
            Status = InvoiceStatus.Draft; // Doğduğu an taslaktır
            CustomerId = customerId;
            OrderId = orderId;
            WarehouseId = warehouseId; // Atama yapıldı
        }

        public void AddDetail(InvoiceDetail detail)
        {
            if (Status != InvoiceStatus.Draft)
                throw new InvalidOperationException("Sadece taslak faturaya ürün eklenebilir.");

            _details.Add(detail);
            CalculateTotals();
        }

        public void CalculateTotals()
        {
            TotalGross = 0;
            TotalDiscount = 0;
            TotalVat = 0;
            GrandTotal = 0;

            foreach (var detail in _details)
            {
                detail.CalculateLineTotal();

                decimal lineGross = detail.Quantity * detail.UnitPrice;
                decimal lineDiscount = lineGross * detail.DiscountRate;
                decimal lineNet = lineGross - lineDiscount;
                decimal lineVat = lineNet * detail.VatRate;

                TotalGross += Math.Round(lineGross, 2, MidpointRounding.AwayFromZero);
                TotalDiscount += Math.Round(lineDiscount, 2, MidpointRounding.AwayFromZero);
                TotalVat += Math.Round(lineVat, 2, MidpointRounding.AwayFromZero);
                GrandTotal += detail.LineTotal;
            }
        }

        public void Approve(Guid transactionId, PaymentType paymentType)
        {
            // Guard Clauses
            if (Status != InvoiceStatus.Draft)
                throw new InvalidOperationException("Sadece taslak faturada işlem yapılabilir.");

            if (!_details.Any())
                throw new InvalidOperationException("Fatura satırı boş olamaz.");

            CalculateTotals();
            Status = InvoiceStatus.Approved;

            TransactionId = transactionId;
            PaymentType = paymentType;
        }

        public void Cancel()
        {
            if (Status != InvoiceStatus.Approved)
                throw new InvalidOperationException("Sadece onaylanmış faturalar iptal edilebilir.");

            Status = InvoiceStatus.Canceled;
        }
    }
}
