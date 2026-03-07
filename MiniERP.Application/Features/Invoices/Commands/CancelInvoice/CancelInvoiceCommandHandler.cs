using MediatR;
using MiniERP.Application.Interfaces;
using MiniERP.Domain.Common;
using MiniERP.Domain.Entities;
using MiniERP.Domain.Enums;
using System.Data;

namespace MiniERP.Application.Features.Invoices.Commands.CancelInvoice
{
    public sealed class CancelInvoiceCommandHandler : IRequestHandler<CancelInvoiceCommand, Result<string>>
    {
        private readonly IRepository<Invoice> _invoiceRepository;
        private readonly IRepository<Order> _orderRepository;
        private readonly IRepository<StockTransaction> _stockRepository;
        private readonly IRepository<CustomerTransaction> _customerRepository;
        private readonly IRepository<CashTransaction> _cashRepository;
        private readonly IRepository<BankTransaction> _bankRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CancelInvoiceCommandHandler(
            IRepository<Invoice> invoiceRepository,
            IRepository<Order> orderRepository,
            IRepository<StockTransaction> stockRepository,
            IRepository<CustomerTransaction> customerRepository,
            IRepository<CashTransaction> cashRepository,
            IRepository<BankTransaction> bankRepository,
            IUnitOfWork unitOfWork)
        {
            _invoiceRepository = invoiceRepository;
            _orderRepository = orderRepository;
            _stockRepository = stockRepository;
            _customerRepository = customerRepository;
            _cashRepository = cashRepository;
            _bankRepository = bankRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<string>> Handle(CancelInvoiceCommand request, CancellationToken cancellationToken)
        {
            await using var transaction = await _unitOfWork.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);

            try
            {
                // 1. Faturayı detaylarıyla çek
                var invoice = await _invoiceRepository.GetByIdWithIncludesAsync(request.Id, cancellationToken, x => x.Details);

                if (invoice == null) return Result<string>.Failure("Fatura bulunamadı.");

                var originalTid = invoice.TransactionId;
                if (originalTid == null) return Result<string>.Failure("Faturaya ait işlem takibi bulunamadığı için iptal edilemez.");

                // Statüyü 'Canceled' yap
                invoice.Cancel();

                var cancelTid = Guid.NewGuid();

                // 2. STOK TERS KAYITLARI (Nötrleme)
                foreach (var detail in invoice.Details)
                {
                    var reverseStock = new StockTransaction
                    {
                        DocumentNo = "İPT-" + invoice.InvoiceNumber,
                        TransactionDate = DateTime.Now,
                        Quantity = -detail.Quantity,
                        UnitPrice = detail.UnitPrice,
                        Description = $"{invoice.InvoiceNumber} Fatura İptal Kaydı",
                        Type = invoice.Type == InvoiceType.Purchase ? StockTransactionType.In : StockTransactionType.Out,
                        TransactionId = cancelTid,
                        ProductId = detail.ProductId,
                        WarehouseId = detail.WarehouseId,
                        CustomerId = invoice.CustomerId,
                        PaymentType = invoice.PaymentType ?? PaymentType.Credit
                    };
                    await _stockRepository.AddAsync(reverseStock, cancellationToken);
                }

                // --- 3. CARİ TERS KAYITLAR ---

                // A) Fatura Kaydının Tersi (Müşteri borç/alacak bakiyesini nötrler)
                var reverseInvoiceEntry = new CustomerTransaction
                {
                    TransactionId = cancelTid,
                    Date = DateTime.Now,
                    Description = $"{invoice.InvoiceNumber} nolu Fatura İptali (Fatura Ters Kaydı)",
                    CustomerId = invoice.CustomerId,
                    Debit = invoice.Type == InvoiceType.Purchase ? invoice.GrandTotal : 0,
                    Credit = invoice.Type == InvoiceType.Sales ? invoice.GrandTotal : 0
                };
                await _customerRepository.AddAsync(reverseInvoiceEntry, cancellationToken);

                // B) Ödeme/Tahsilat Kaydının Tersi (Sadece Peşin işlemlerde cariyi tam sıfırlamak için)
                if (invoice.PaymentType != PaymentType.Credit)
                {
                    var reversePaymentEntry = new CustomerTransaction
                    {
                        TransactionId = cancelTid,
                        Date = DateTime.Now,
                        Description = $"{invoice.InvoiceNumber} nolu Fatura İptali (Ödeme/Tahsilat İadesi)",
                        CustomerId = invoice.CustomerId,
                        Debit = invoice.Type == InvoiceType.Sales ? invoice.GrandTotal : 0,
                        Credit = invoice.Type == InvoiceType.Purchase ? invoice.GrandTotal : 0
                    };
                    await _customerRepository.AddAsync(reversePaymentEntry, cancellationToken);
                }

                // 4. KASA / BANKA TERS KAYIT
                if (invoice.PaymentType == PaymentType.Cash)
                {
                    var cashQuery = _cashRepository.Where(x => x.TransactionId == originalTid);
                    var cashLogs = await _cashRepository.ToListAsync(cashQuery, cancellationToken);
                    var originalCashLog = cashLogs.FirstOrDefault();

                    if (originalCashLog != null)
                    {
                        var reverseCash = new CashTransaction
                        {
                            TransactionId = cancelTid,
                            Date = DateTime.Now,
                            Description = $"{invoice.InvoiceNumber} Fatura İptal İadesi",
                            CashId = originalCashLog.CashId,
                            Debit = invoice.Type == InvoiceType.Purchase ? invoice.GrandTotal : 0,
                            Credit = invoice.Type == InvoiceType.Sales ? invoice.GrandTotal : 0
                        };
                        await _cashRepository.AddAsync(reverseCash, cancellationToken);
                    }
                }
                else if (invoice.PaymentType == PaymentType.Bank)
                {
                    var bankQuery = _bankRepository.Where(x => x.TransactionId == originalTid);
                    var bankLogs = await _bankRepository.ToListAsync(bankQuery, cancellationToken);
                    var originalBankLog = bankLogs.FirstOrDefault();

                    if (originalBankLog != null)
                    {
                        var reverseBank = new BankTransaction
                        {
                            TransactionId = cancelTid,
                            Date = DateTime.Now,
                            Description = $"{invoice.InvoiceNumber} Fatura İptal İadesi",
                            BankId = originalBankLog.BankId,
                            Debit = invoice.Type == InvoiceType.Purchase ? invoice.GrandTotal : 0,
                            Credit = invoice.Type == InvoiceType.Sales ? invoice.GrandTotal : 0
                        };
                        await _bankRepository.AddAsync(reverseBank, cancellationToken);
                    }
                }

                // 5. SİPARİŞİ GERİ DÖNDÜR
                if (invoice.OrderId.HasValue)
                {
                    var order = await _orderRepository.GetByIdAsync(invoice.OrderId.Value, cancellationToken);
                    if (order != null)
                    {
                        order.UndoInvoicedStatus();
                        await _orderRepository.UpdateAsync(order, cancellationToken);
                    }
                }

                
                await _invoiceRepository.UpdateAsync(invoice, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return Result<string>.Success(invoice.InvoiceNumber, "Fatura iptal edildi, cari terazi ve ters kayıtlar başarıyla işlendi.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Result<string>.Failure($"İptal operasyonu başarısız: {ex.Message}");
            }
        }
    }
}