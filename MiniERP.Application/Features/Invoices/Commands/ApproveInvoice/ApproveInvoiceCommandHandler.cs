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
            // Önce temel kontrolleri yapalım
            if (request.PaymentType == PaymentType.Credit && (request.CashId is not null || request.BankId is not null))
                return Result<string>.Failure("Açık (veresiye) hesapta kasa veya banka seçilemez.");

            if (request.PaymentType == PaymentType.Cash && request.CashId is null)
                return Result<string>.Failure("Nakit ödeme için kasa seçimi zorunludur.");

            if (request.PaymentType == PaymentType.Bank && request.BankId is null)
                return Result<string>.Failure("Banka ödemesi için banka hesabı seçimi zorunludur.");

            // İşlemlerin yarım kalmaması için Serializable seviyesinde kilitliyoruz
            await using var transaction = await _unitOfWork.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);

            try
            {
                // Faturayı satırlarıyla beraber çekelim
                var invoice = await _invoiceRepository.GetByIdWithIncludesAsync(request.Id, cancellationToken, x => x.Details);

                if (invoice == null) return Result<string>.Failure("Fatura bulunamadı.");
                if (invoice.Status != InvoiceStatus.Draft) return Result<string>.Failure("Bu fatura zaten onaylanmış veya iptal edilmiş.");

                var sharedTransactionId = Guid.NewGuid(); // Tüm hareketleri bu ID ile birbirine bağlayacağız

                // 🚀 1. MANTIKSAL YÖN VE AÇIKLAMA (Eski hatalı isPurchase mantığı kaldırıldı, Switch yapısı eklendi)
                var stockType = invoice.Type switch
                {
                    InvoiceType.Sales => StockTransactionType.Out,
                    InvoiceType.Purchase => StockTransactionType.In,
                    InvoiceType.SalesReturn => StockTransactionType.In, // Satıştan dönen mal depoya girer
                    InvoiceType.PurchaseReturn => StockTransactionType.Out, // Alımdan dönen mal depodan çıkar
                    _ => throw new Exception("Geçersiz fatura tipi!")
                };

                var description = invoice.Type switch
                {
                    InvoiceType.Sales => "Satış Faturası",
                    InvoiceType.Purchase => "Alım Faturası",
                    InvoiceType.SalesReturn => "Satış İade Faturası",
                    InvoiceType.PurchaseReturn => "Alım İade Faturası",
                    _ => "Fatura İşlemi"
                };

                // Finansal olarak borç mu (Debit) alacak mı (Credit)? 
                bool isDebitEntry = invoice.Type == InvoiceType.Sales || invoice.Type == InvoiceType.PurchaseReturn;


                // 🚀 2. STOK HAREKETLERİ VE KONTROLÜ
                foreach (var detail in invoice.Details)
                {
                    // Her zaman mutlak (pozitif) değerle çalışıp, yönü stockType (In/Out) ile belirleyeceğiz
                    decimal absQuantity = Math.Abs(detail.Quantity);

                    // Sadece stoktan mal çıkışı yapılan durumlarda (Satış ve Alım İadesi) stok kontrolü yapılır
                    if (stockType == StockTransactionType.Out)
                    {
                        // Mevcut fiziksel stok hesabı (Girişler - Çıkışlar)
                        var stockMoves = await _stockRepository.ToListAsync(_stockRepository.Where(x => x.ProductId == detail.ProductId && x.WarehouseId == detail.WarehouseId && !x.IsDeleted), cancellationToken);

                        // 🚀 DÜZELTME BURADA: Eğer Type In(1) VEYA 3 (Açılış) ise Miktarı Topla, değilse Çıkar!
                        var physicalBalance = stockMoves.Sum(x =>
                            ((int)x.Type == (int)StockTransactionType.In || (int)x.Type == 3)
                                ? x.Quantity
                                : -x.Quantity);

                        // Bekleyen (Onaylı) diğer siparişlerin rezervasyonu (Bu faturanın ait olduğu sipariş hariç tutuluyor)
                        var reservations = await _orderDetailRepository.ToListAsync(_orderDetailRepository.Where(x =>
                            x.ProductId == detail.ProductId &&
                            x.WarehouseId == detail.WarehouseId &&
                            x.Order!.Status == OrderStatus.Approved &&
                            !x.IsDeleted &&
                            (!invoice.OrderId.HasValue || x.OrderId != invoice.OrderId.Value)), cancellationToken);

                        var reservedAmount = reservations.Sum(x => x.Quantity);
                        var availableStock = physicalBalance - reservedAmount;

                        if (availableStock < absQuantity)
                            throw new Exception($"{detail.ProductId} için yeterli stok yok. Müsait: {availableStock}, Gereken: {absQuantity}");
                    }

                    // Stok hareket kaydını atıyoruz
                    await _stockRepository.AddAsync(new StockTransaction
                    {
                        TransactionId = sharedTransactionId,
                        DocumentNo = invoice.InvoiceNumber,
                        TransactionDate = invoice.InvoiceDate,
                        ProductId = detail.ProductId,
                        WarehouseId = detail.WarehouseId,
                        Quantity = absQuantity, // Daima pozitif
                        UnitPrice = detail.UnitPrice,
                        Type = stockType, // 🚀 Yön (In/Out) dinamik olarak atandı
                        Description = description, // 🚀 Açıklama dinamik olarak atandı
                        CustomerId = invoice.CustomerId,
                        PaymentType = request.PaymentType
                    }, cancellationToken);
                }

                // 🚀 3. CARİ HAREKET (Fatura borç/alacak kaydı)
                var mainTransaction = new CustomerTransaction
                {
                    TransactionId = sharedTransactionId,
                    Date = invoice.InvoiceDate,
                    Description = $"{description}: {invoice.InvoiceNumber}", // 🚀 Artık SQL'de doğru açıklama yazacak
                    CustomerId = invoice.CustomerId,
                    Debit = isDebitEntry ? invoice.GrandTotal : 0,
                    Credit = !isDebitEntry ? invoice.GrandTotal : 0
                };
                await _customerRepository.AddAsync(mainTransaction, cancellationToken);


                // 🚀 4. PEŞİN ÖDEME VARSA (Kasa/Banka kayıtları)
                if (request.PaymentType != PaymentType.Credit)
                {
                    // Cariyi kapatan ters kayıt (Tahsilat/Ödeme)
                    var offsetTransaction = new CustomerTransaction
                    {
                        TransactionId = sharedTransactionId,
                        Date = invoice.InvoiceDate,
                        Description = $"Fatura Ödemesi: {invoice.InvoiceNumber}",
                        CustomerId = invoice.CustomerId,
                        Debit = !isDebitEntry ? invoice.GrandTotal : 0,
                        Credit = isDebitEntry ? invoice.GrandTotal : 0
                    };
                    await _customerRepository.AddAsync(offsetTransaction, cancellationToken);

                    // Kasa veya Banka girişi/çıkışı
                    if (request.PaymentType == PaymentType.Cash && request.CashId.HasValue)
                    {
                        await _cashRepository.AddAsync(new CashTransaction
                        {
                            TransactionId = sharedTransactionId,
                            CashId = request.CashId.Value,
                            Date = invoice.InvoiceDate,
                            Debit = isDebitEntry ? invoice.GrandTotal : 0, // Satış/Alımİadesi ise para girer
                            Credit = !isDebitEntry ? invoice.GrandTotal : 0, // Alım/Satışİadesi ise para çıkar
                            Description = $"Fatura No: {invoice.InvoiceNumber}"
                        }, cancellationToken);
                    }
                    else if (request.PaymentType == PaymentType.Bank && request.BankId.HasValue)
                    {
                        await _bankRepository.AddAsync(new BankTransaction
                        {
                            TransactionId = sharedTransactionId,
                            BankId = request.BankId.Value,
                            Date = invoice.InvoiceDate,
                            Debit = isDebitEntry ? invoice.GrandTotal : 0,
                            Credit = !isDebitEntry ? invoice.GrandTotal : 0,
                            Description = $"Fatura No: {invoice.InvoiceNumber}"
                        }, cancellationToken);
                    }
                }

                // 🚀 5. SİPARİŞİ KAPAT VE FATURAYI ONAYLA
                if (invoice.OrderId.HasValue)
                {
                    var order = await _orderRepository.GetByIdAsync(invoice.OrderId.Value, cancellationToken);
                    if (order != null)
                    {
                        order.MarkAsInvoiced(); // Statü artık Faturalandı
                        await _orderRepository.UpdateAsync(order, cancellationToken);
                    }
                }

                // Faturayı onayla ve veritabanını güncelle
                invoice.Approve(sharedTransactionId, request.PaymentType);
                await _invoiceRepository.UpdateAsync(invoice, cancellationToken);

                // Tüm işlemleri tek potada kaydet
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