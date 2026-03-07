using MiniERP.Domain.Common;
using MiniERP.Domain.Enums;

namespace MiniERP.Domain.Entities
{
    public sealed class Order : BaseEntity
    {
        public string OrderNumber { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public string? Description { get; set; }

        public OrderType Type { get; set; }

        // Status dışarıdan atamaya kapalı (Encapsulation).
        public OrderStatus Status { get; private set; }

        public Guid CustomerId { get; set; }
        public Customer? Customer { get; set; }

        public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

        public Order()
        {
            // Bir sipariş doğduğunda her zaman 'Beklemede' başlar.
            Status = OrderStatus.Pending;
        }

        // İş Kuralları

        // Siparişi onaylar ve stokların dinamik rezerve edilmesinin önünü açar.
        public void Approve()
        {
            if (Status != OrderStatus.Pending)
                throw new InvalidOperationException("Sadece 'Beklemede' olan siparişler onaylanabilir.");

            Status = OrderStatus.Approved;
        }

        // Siparişi iptal eder. Rezerve edilen stoklar boşa çıkar.
        public void Cancel()
        {
            if (Status == OrderStatus.Invoiced)
                throw new InvalidOperationException("Faturalanmış sipariş iptal edilemez, iade süreci başlatılmalıdır.");

            Status = OrderStatus.Canceled;
        }

        // 5. Modülde faturası kesildiğinde çağrılacak metot.
        public void MarkAsInvoiced()
        {
            if (Status != OrderStatus.Approved)
                throw new InvalidOperationException("Sadece 'Onaylanmış' siparişler faturalandırılabilir.");

            Status = OrderStatus.Invoiced;
        }

        public void UndoInvoicedStatus()
        {
            if (Status != OrderStatus.Invoiced)
                throw new InvalidOperationException("Sadece faturalanmış siparişler geri alınabilir.");

            Status = OrderStatus.Approved;
        }
    }
}
