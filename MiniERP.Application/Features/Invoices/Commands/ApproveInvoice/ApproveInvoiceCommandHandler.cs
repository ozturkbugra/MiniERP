using MediatR;
using MiniERP.Application.Interfaces;
using MiniERP.Domain.Common;
using MiniERP.Domain.Entities;
using MiniERP.Domain.Enums;
using System.Data;

namespace MiniERP.Application.Features.Invoices.Commands.ApproveInvoice
{
    public sealed class ApproveInvoiceCommandHandler : IRequestHandler<ApproveInvoiceCommand, Result<string>>
    {
        private readonly IRepository<Invoice> _invoiceRepository;
        private readonly IRepository<Order> _orderRepository;
        private readonly IRepository<StockTransaction> _stockRepository;
        private readonly IRepository<CustomerTransaction> _customerRepository;
        private readonly IRepository<OrderDetail> _orderDetailRepository;
        private readonly IRepository<CashTransaction> _cashRepository;
        private readonly IRepository<BankTransaction> _bankRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ApproveInvoiceCommandHandler(
            IRepository<Invoice> invoiceRepository,
            IRepository<Order> orderRepository,
            IRepository<StockTransaction> stockRepository,
            IRepository<CustomerTransaction> customerRepository,
            IRepository<OrderDetail> orderDetailRepository,
            IRepository<CashTransaction> cashRepository,
            IRepository<BankTransaction> bankRepository,
            IUnitOfWork unitOfWork) 
        {
            _invoiceRepository = invoiceRepository;
            _orderRepository = orderRepository;
            _stockRepository = stockRepository;
            _customerRepository = customerRepository;
            _orderDetailRepository = orderDetailRepository;
            _cashRepository = cashRepository;
            _bankRepository = bankRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<string>> Handle(ApproveInvoiceCommand request, CancellationToken cancellationToken)
        {
            // Önce temel kontrolleri yapalım, boşuna transaction başlatmayalım
            if (request.PaymentType == PaymentType.Credit && (request.CashId is not null || request.BankId is not null))
                return Result<string>.Failure("Açık (veresiye) hesapta kasa veya banka seçemezsin.");

            if (request.PaymentType == PaymentType.Cash && request.CashId is null)
                return Result<string>.Failure("Nakit ödeme için kasa seçmen şart.");

            if (request.PaymentType == PaymentType.Bank && request.BankId is null)
                return Result<string>.Failure("Banka ödemesi için banka hesabı seçmelisin.");

            // İşlemlerin yarım kalmaması için Serializable seviyesinde kilitliyoruz
            await using var transaction = await _unitOfWork.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);

            try
            {
                // Faturayı satırlarıyla beraber çekelim
                var invoice = await _invoiceRepository.GetByIdWithIncludesAsync(request.Id, cancellationToken, x => x.Details);

                if (invoice == null) return Result<string>.Failure("Fatura bulunamadı.");
                if (invoice.Status != InvoiceStatus.Draft) return Result<string>.Failure("Bu fatura zaten onaylanmış veya iptal edilmiş.");

                var isPurchase = invoice.Type == InvoiceType.Purchase;
                var sharedTransactionId = Guid.NewGuid(); // Tüm hareketleri bu ID ile birbirine bağlayacağız

                // 1. Stok Hareketleri ve Kontrolü
                foreach (var detail in invoice.Details)
                {
                    if (!isPurchase) // Satış yapıyorsak stok yeterli mi bakalım
                    {
                        // Mevcut fiziksel stok hesabı (Girişler - Çıkışlar)
                        var stockMoves = await _stockRepository.ToListAsync(_stockRepository.Where(x => x.ProductId == detail.ProductId && x.WarehouseId == detail.WarehouseId && !x.IsDeleted), cancellationToken);
                        var physicalBalance = stockMoves.Sum(x => x.Type == StockTransactionType.In ? x.Quantity : -x.Quantity);

                        // Bekleyen (Onaylı) diğer siparişlerin rezervasyonu
                        var reservations = await _orderDetailRepository.ToListAsync(_orderDetailRepository.Where(x =>
                            x.ProductId == detail.ProductId &&
                            x.WarehouseId == detail.WarehouseId &&
                            x.Order!.Status == OrderStatus.Approved &&
                            !x.IsDeleted &&
                            (!invoice.OrderId.HasValue || x.OrderId != invoice.OrderId.Value)), cancellationToken);

                        var reservedAmount = reservations.Sum(x => x.Quantity);
                        var availableStock = physicalBalance - reservedAmount;

                        if (availableStock < detail.Quantity)
                            throw new Exception($"{detail.ProductId} için yeterli stok yok. Müsait: {availableStock}, Gereken: {detail.Quantity}");
                    }

                    // Stok hareket kaydını atıyoruz
                    await _stockRepository.AddAsync(new StockTransaction
                    {
                        TransactionId = sharedTransactionId,
                        DocumentNo = invoice.InvoiceNumber,
                        TransactionDate = invoice.InvoiceDate,
                        ProductId = detail.ProductId,
                        WarehouseId = detail.WarehouseId,
                        Quantity = detail.Quantity,
                        UnitPrice = detail.UnitPrice,
                        Type = isPurchase ? StockTransactionType.In : StockTransactionType.Out,
                        Description = isPurchase ? "Alım Faturası" : "Satış Faturası",
                        CustomerId = invoice.CustomerId,
                        PaymentType = request.PaymentType
                    }, cancellationToken);
                }

                // 2. Cari Hareket (Fatura borç/alacak kaydı)
                var mainTransaction = new CustomerTransaction
                {
                    TransactionId = sharedTransactionId,
                    Date = invoice.InvoiceDate,
                    Description = $"Fatura: {invoice.InvoiceNumber}",
                    CustomerId = invoice.CustomerId,
                    Debit = isPurchase ? 0 : invoice.GrandTotal, // Satışsa borçlandır, alışsa alacaklandır
                    Credit = isPurchase ? invoice.GrandTotal : 0
                };
                await _customerRepository.AddAsync(mainTransaction, cancellationToken);

                // 3. Peşin Ödeme Varsa (Kasa/Banka kayıtları)
                if (request.PaymentType != PaymentType.Credit)
                {
                    // Cariyi kapatan ters kayıt (Tahsilat/Ödeme)
                    var offsetTransaction = new CustomerTransaction
                    {
                        TransactionId = sharedTransactionId,
                        Date = invoice.InvoiceDate,
                        Description = $"Fatura Ödemesi: {invoice.InvoiceNumber}",
                        CustomerId = invoice.CustomerId,
                        Debit = isPurchase ? invoice.GrandTotal : 0,
                        Credit = isPurchase ? 0 : invoice.GrandTotal
                    };
                    await _customerRepository.AddAsync(offsetTransaction, cancellationToken);

                    // Kasa veya Banka girişi/çıkışı
                    if (request.PaymentType == PaymentType.Cash)
                    {
                        await _cashRepository.AddAsync(new CashTransaction
                        {
                            TransactionId = sharedTransactionId,
                            CashId = request.CashId!.Value,
                            Date = invoice.InvoiceDate,
                            Debit = isPurchase ? 0 : invoice.GrandTotal, // Satışsa kasaya para girer
                            Credit = isPurchase ? invoice.GrandTotal : 0,
                            Description = $"Fatura No: {invoice.InvoiceNumber}"
                        }, cancellationToken);
                    }
                    else if (request.PaymentType == PaymentType.Bank)
                    {
                        await _bankRepository.AddAsync(new BankTransaction
                        {
                            TransactionId = sharedTransactionId,
                            BankId = request.BankId!.Value,
                            Date = invoice.InvoiceDate,
                            Debit = isPurchase ? 0 : invoice.GrandTotal,
                            Credit = isPurchase ? invoice.GrandTotal : 0,
                            Description = $"Fatura No: {invoice.InvoiceNumber}"
                        }, cancellationToken);
                    }
                }

                // 4. Eğer bir siparişten geldiysek o siparişi kapatalım
                if (invoice.OrderId.HasValue)
                {
                    var order = await _orderRepository.GetByIdAsync(invoice.OrderId.Value, cancellationToken);
                    if (order != null)
                    {
                        order.MarkAsInvoiced(); // Statü artık Faturalandı
                        await _orderRepository.UpdateAsync(order, cancellationToken);
                    }
                }

                // 5. Faturayı onayla ve bitir
                invoice.Approve(sharedTransactionId, request.PaymentType);
                await _invoiceRepository.UpdateAsync(invoice, cancellationToken);

                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return Result<string>.Success(invoice.InvoiceNumber, "Fatura başarıyla onaylandı ve tüm kayıtlar işlendi.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken); // Bir hata olursa her şeyi geri al
                return Result<string>.Failure($"Hata oluştu, hiçbir işlem yapılmadı: {ex.Message}");
            }
        }
    }
}