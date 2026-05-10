using MediatR;
using MiniERP.Application.Interfaces;
using MiniERP.Domain.Common;
using MiniERP.Domain.Entities;
using MiniERP.Domain.Enums;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MiniERP.Application.Features.Invoices.Commands.CreateInvoice
{
    public sealed class CreateInvoiceCommandHandler : IRequestHandler<CreateInvoiceCommand, Result<string>>
    {
        private readonly IRepository<Invoice> _invoiceRepository;
        private readonly IRepository<Order> _orderRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateInvoiceCommandHandler(
            IRepository<Invoice> invoiceRepository,
            IRepository<Order> orderRepository,
            IUnitOfWork unitOfWork)
        {
            _invoiceRepository = invoiceRepository;
            _orderRepository = orderRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<string>> Handle(CreateInvoiceCommand request, CancellationToken cancellationToken)
        {
            // 1. Doğrulama
            if (request.OrderId.HasValue)
            {
                var order = await _orderRepository.GetByIdAsync(request.OrderId.Value, cancellationToken);
                if (order == null) return Result<string>.Failure("Kaynak sipariş bulunamadı.");

                if (order.CustomerId != request.CustomerId)
                    return Result<string>.Failure("Sipariş müşterisi ile fatura müşterisi uyuşmuyor!");

                if (order.Status == OrderStatus.Invoiced)
                    return Result<string>.Failure("Bu sipariş zaten faturalandırılmış.");
            }

            // 2. ASENKRON FATURA NUMARASI ÜRETİMİ
            string year = DateTime.Now.Year.ToString();
            string prefix = $"FT-{year}-";

            var query = _invoiceRepository.Where(x => x.InvoiceNumber.StartsWith(prefix))
                                          .OrderByDescending(x => x.InvoiceNumber)
                                          .Take(1);

            var lastInvoices = await _invoiceRepository.ToListAsync(query, cancellationToken);
            var lastInvoice = lastInvoices.FirstOrDefault();

            string invoiceNumber;
            if (lastInvoice == null)
            {
                invoiceNumber = $"{prefix}00000001";
            }
            else
            {
                string lastSequenceStr = lastInvoice.InvoiceNumber.Replace(prefix, "");
                if (long.TryParse(lastSequenceStr, out long lastSequence))
                {
                    invoiceNumber = $"{prefix}{(lastSequence + 1):D8}";
                }
                else
                {
                    invoiceNumber = $"{prefix}{new Random().Next(1, 99999999):D8}";
                }
            }

            // 3. Fatura Taslağını Oluştur
            var invoice = new Invoice(
                invoiceNumber,
                request.InvoiceDate,
                request.Type,
                request.CustomerId,
                request.OrderId,
                request.WarehouseId
            );

            // 4. Detayları Ekleme ve Depo Kontrolü
            foreach (var d in request.Details)
            {
                var finalWarehouseId = d.WarehouseId == Guid.Empty ? request.WarehouseId : d.WarehouseId;
                if (finalWarehouseId == null || finalWarehouseId == Guid.Empty)
                    return Result<string>.Failure($"{d.ProductId} ID'li ürün için depo seçilmemiş.");

                // 🚀 KRİTİK DÜZELTME: Miktarı daima mutlak (pozitif) kaydediyoruz.
                // Kullanıcı arayüzde eksi yazsa bile veritabanına pozitif girecek,
                // Onay aşamasında zaten In/Out ile doğru işlemi yapacağız.
                decimal absQuantity = Math.Abs(d.Quantity);

                invoice.AddDetail(new InvoiceDetail(
                    d.ProductId,
                    finalWarehouseId.Value,
                    absQuantity, // 🚀 Koruma burada
                    d.UnitPrice,
                    d.DiscountRate,
                    d.VatRate));
            }

            // 5. Kaydet 
            try
            {
                await _invoiceRepository.AddAsync(invoice, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
            catch (Exception)
            {
                return Result<string>.Failure("Fatura numarası çakışması veya veritabanı hatası oluştu. Lütfen tekrar deneyiniz.");
            }

            return Result<string>.Success(invoice.InvoiceNumber, $"Fatura taslağı hazırlandı. No: {invoiceNumber}");
        }
    }
}