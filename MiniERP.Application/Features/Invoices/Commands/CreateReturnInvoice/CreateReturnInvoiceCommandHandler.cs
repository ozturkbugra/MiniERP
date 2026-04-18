using MediatR;
using MiniERP.Application.Interfaces;
using MiniERP.Domain.Common;
using MiniERP.Domain.Entities;
using MiniERP.Domain.Enums;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MiniERP.Application.Features.Invoices.Commands.CreateReturnInvoice
{
    public sealed class CreateReturnInvoiceCommandHandler : IRequestHandler<CreateReturnInvoiceCommand, Result<string>>
    {
        private readonly IRepository<Invoice> _invoiceRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateReturnInvoiceCommandHandler(IRepository<Invoice> invoiceRepository, IUnitOfWork unitOfWork)
        {
            _invoiceRepository = invoiceRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<string>> Handle(CreateReturnInvoiceCommand request, CancellationToken cancellationToken)
        {
            // 1. Faturayı detaylarıyla bulalım
            var parentInvoice = await _invoiceRepository.GetByIdWithIncludesAsync(
                request.ParentInvoiceId,
                cancellationToken,
                x => x.Details);

            if (parentInvoice == null)
                return Result<string>.Failure("İade edilmek istenen orijinal fatura bulunamadı.");

            if (parentInvoice.Status != InvoiceStatus.Approved)
                return Result<string>.Failure("Sadece onaylanmış faturaların iadesi yapılabilir.");

            // 2. İade faturasının tipini belirleme kısmı
            InvoiceType returnType = parentInvoice.Type switch
            {
                InvoiceType.Sales => InvoiceType.SalesReturn,
                InvoiceType.Purchase => InvoiceType.PurchaseReturn,
                _ => throw new Exception("Bu fatura tipi iade edilemez.")
            };

            // 🚀 3. Fatura Numarası Üretimi (TryParse ile patlamaya karşı korumalı)
            string year = DateTime.Now.Year.ToString();
            string prefix = $"RT-{year}-";

            var query = _invoiceRepository.Where(x => x.InvoiceNumber.StartsWith(prefix)).OrderByDescending(x => x.InvoiceNumber).Take(1);
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
                    // DB'de formatı bozuk bir RT serisi varsa rastgele atayıp kurtarsın
                    invoiceNumber = $"{prefix}{new Random().Next(1, 99999999):D8}";
                }
            }

            // 4. Yeni İade Faturasını (Taslak olarak) yaratıyoruz
            var returnInvoice = new Invoice(
                invoiceNumber,
                request.ReturnDate,
                returnType,
                parentInvoice.CustomerId,
                parentInvoice.OrderId,
                parentInvoice.WarehouseId,
                parentInvoice.Id
            );

            // 🚀 5. Miktar Kontrolü ve Negatif Zırhı
            foreach (var reqDetail in request.Details)
            {
                // Frontend eksi gönderse bile pozitife çevirip öyle işlem yapıyoruz
                decimal absReturnQuantity = Math.Abs(reqDetail.ReturnQuantity);

                if (absReturnQuantity == 0)
                    return Result<string>.Failure("İade miktarı sıfır olamaz.");

                // Orijinal faturada bu ürün var mı?
                var originalDetail = parentInvoice.Details.FirstOrDefault(x => x.ProductId == reqDetail.ProductId);

                if (originalDetail == null)
                    return Result<string>.Failure("İade edilmek istenen ürün orijinal faturada yok.");

                // Orijinal miktar da DB'de pozitif olduğu için kıyaslama kusursuz çalışır
                if (absReturnQuantity > originalDetail.Quantity)
                    return Result<string>.Failure($"Hata: Bu üründen en fazla {originalDetail.Quantity} adet iade edebilirsiniz.");

                // Kontrolden geçti, iade faturasına kalemi ekle
                returnInvoice.AddDetail(new InvoiceDetail(
                    reqDetail.ProductId,
                    reqDetail.WarehouseId,
                    absReturnQuantity, // 🚀 Daima pozitif
                    reqDetail.UnitPrice,
                    reqDetail.DiscountRate,
                    reqDetail.VatRate
                ));
            }

            // 6. Kaydet ve bitir
            try
            {
                await _invoiceRepository.AddAsync(returnInvoice, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                return Result<string>.Failure($"İade faturası oluşturulurken hata: {ex.Message}");
            }

            return Result<string>.Success(returnInvoice.InvoiceNumber, $"İade faturası taslağı oluşturuldu. No: {invoiceNumber}");
        }
    }
}