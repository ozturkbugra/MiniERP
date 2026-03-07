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
        private readonly IRepository<CashTransaction> _cashRepository; // YENİ
        private readonly IRepository<BankTransaction> _bankRepository; // YENİ
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
            // 1. Validasyonlar (Kapıdaki güvenlik)
            if (request.PaymentType == PaymentType.Credit && (request.CashId is not null || request.BankId is not null))
                return Result<string>.Failure("Veresiye işlemde kasa veya banka seçilemez.");

            if (request.PaymentType == PaymentType.Cash && request.CashId is null)
                return Result<string>.Failure("Nakit işlem için kasa seçilmelidir.");

            if (request.PaymentType == PaymentType.Bank && request.BankId is null)
                return Result<string>.Failure("Banka işlem için banka seçilmelidir.");

            await using var transaction = await _unitOfWork.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);

            try
            {
                // Faturayı satırlarıyla beraber çek
                var invoice = await _invoiceRepository.GetByIdWithIncludesAsync(request.Id, cancellationToken, x => x.Details);

                if (invoice == null) return Result<string>.Failure("Fatura bulunamadı.");
                if (invoice.IsDeleted) return Result<string>.Failure("Silinmiş fatura onaylanamaz.");
                if (invoice.Status != InvoiceStatus.Draft) return Result<string>.Failure("Sadece taslak faturalar onaylanabilir.");

                var isPurchase = invoice.Type == InvoiceType.Purchase;
                var sharedTransactionId = Guid.NewGuid();

                // 2. STOK KONTROLÜ VE HAREKETLERİ
                foreach (var detail in invoice.Details)
                {
                    if (!isPurchase)
                    {
                        var physicalBalance = _stockRepository
                            .Where(x => x.ProductId == detail.ProductId && x.WarehouseId == detail.WarehouseId && !x.IsDeleted)
                            .AsEnumerable()
                            .Sum(x => x.Type == StockTransactionType.In ? x.Quantity : -x.Quantity);

                        var reservedAmount = _orderDetailRepository
                            .Where(x => x.ProductId == detail.ProductId &&
                                        x.WarehouseId == detail.WarehouseId &&
                                        x.Order!.Status == OrderStatus.Approved &&
                                        !x.IsDeleted &&
                                        (!invoice.OrderId.HasValue || x.OrderId != invoice.OrderId.Value))
                            .AsEnumerable()
                            .Sum(x => x.Quantity);

                        var availableStock = physicalBalance - reservedAmount;

                        if (availableStock < detail.Quantity)
                            return Result<string>.Failure($"Yetersiz stok! Ürün ID: {detail.ProductId}, Müsait: {availableStock}, Talep: {detail.Quantity}");
                    }

                    var stockTransaction = new StockTransaction
                    {
                        DocumentNo = invoice.InvoiceNumber,
                        TransactionDate = invoice.InvoiceDate,
                        Quantity = detail.Quantity,
                        UnitPrice = detail.UnitPrice,
                        Description = isPurchase ? "Mal Alım Faturası" : "Mal Satış Faturası",
                        Type = isPurchase ? StockTransactionType.In : StockTransactionType.Out,
                        TransactionId = sharedTransactionId,
                        PaymentType = request.PaymentType, 
                        ProductId = detail.ProductId,
                        WarehouseId = detail.WarehouseId,
                        CustomerId = invoice.CustomerId
                    };
                    await _stockRepository.AddAsync(stockTransaction, cancellationToken);
                }

                // 3. CARİ HAREKETİ - FATURA KAYDI (Açık Fatura - Borç/Alacak Oluşumu)
                var customerInvoiceEntry = new CustomerTransaction
                {
                    TransactionId = sharedTransactionId,
                    Date = invoice.InvoiceDate,
                    Description = $"Fatura No: {invoice.InvoiceNumber} - " + (isPurchase ? "Alım Faturası" : "Satış Faturası"),
                    CustomerId = invoice.CustomerId,
                    Debit = isPurchase ? 0 : invoice.GrandTotal,
                    Credit = isPurchase ? invoice.GrandTotal : 0
                };
                customerInvoiceEntry.Validate();
                await _customerRepository.AddAsync(customerInvoiceEntry, cancellationToken);

                // 4. EĞER PEŞİN ÖDEME VARSA (Kasa veya Banka - Kapalı Fatura İşlemi)
                if (request.PaymentType != PaymentType.Credit)
                {
                    // A) Cariyeyi Kapatan Ters Kayıt (Ödeme / Tahsilat)
                    var paymentOffset = new CustomerTransaction
                    {
                        TransactionId = sharedTransactionId,
                        Date = invoice.InvoiceDate,
                        Description = $"Fatura No: {invoice.InvoiceNumber} - " + (isPurchase ? "Peşin Ödeme" : "Peşin Tahsilat"),
                        CustomerId = invoice.CustomerId,
                        Debit = isPurchase ? invoice.GrandTotal : 0, // Alışta borcumuz azalır (Ödeme yaptık)
                        Credit = isPurchase ? 0 : invoice.GrandTotal // Satışta alacağımız azalır (Tahsilat yaptık)
                    };
                    paymentOffset.Validate();
                    await _customerRepository.AddAsync(paymentOffset, cancellationToken);

                    // B) Kasaya veya Bankaya Parayı Gir/Çık
                    if (request.PaymentType == PaymentType.Cash)
                    {
                        var cashEntry = new CashTransaction
                        {
                            TransactionId = sharedTransactionId,
                            Date = invoice.InvoiceDate,
                            Description = $"Fatura No: {invoice.InvoiceNumber} - " + (isPurchase ? "Fatura Ödemesi" : "Fatura Tahsilatı"),
                            CashId = request.CashId!.Value,
                            Debit = isPurchase ? 0 : invoice.GrandTotal, // Satışsa kasaya para GİRER (Borç/Debit artar)
                            Credit = isPurchase ? invoice.GrandTotal : 0 // Alışsa kasadan para ÇIKAR (Alacak/Credit)
                        };
                        cashEntry.Validate();
                        await _cashRepository.AddAsync(cashEntry, cancellationToken);
                    }
                    else if (request.PaymentType == PaymentType.Bank)
                    {
                        var bankEntry = new BankTransaction
                        {
                            TransactionId = sharedTransactionId,
                            Date = invoice.InvoiceDate,
                            Description = $"Fatura No: {invoice.InvoiceNumber} - " + (isPurchase ? "Fatura Ödemesi (Banka)" : "Fatura Tahsilatı (Banka)"),
                            BankId = request.BankId!.Value,
                            Debit = isPurchase ? 0 : invoice.GrandTotal,
                            Credit = isPurchase ? invoice.GrandTotal : 0
                        };
                        bankEntry.Validate();
                        await _bankRepository.AddAsync(bankEntry, cancellationToken);
                    }
                }

                // 5. SİPARİŞ DURUM GÜNCELLEMESİ 
                if (invoice.OrderId.HasValue)
                {
                    var order = await _orderRepository.GetByIdAsync(invoice.OrderId.Value, cancellationToken);
                    if (order != null)
                    {
                        order.MarkAsInvoiced(); 
                        await _orderRepository.UpdateAsync(order, cancellationToken);
                    }
                }

                // 6. Faturayı Onayla ve Kaydet
                invoice.Approve(sharedTransactionId, request.PaymentType);
                await _invoiceRepository.UpdateAsync(invoice, cancellationToken);

                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return Result<string>.Success(invoice.InvoiceNumber, $"Fatura onaylandı. ({request.PaymentType} olarak işlendi)");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Result<string>.Failure($"Kritik hata: {ex.Message}");
            }
        }
    }
}