using MediatR;
using MiniERP.Application.Interfaces;
using MiniERP.Domain.Common;
using MiniERP.Domain.Entities;
using MiniERP.Domain.Enums;

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

            // 2. İade faturasının tipini belirleme kısmı (Satışsa Satış İade, Alışsa Alış İade olur)
            InvoiceType returnType = parentInvoice.Type switch
            {
                InvoiceType.Sales => InvoiceType.SalesReturn,
                InvoiceType.Purchase => InvoiceType.PurchaseReturn,
                _ => throw new Exception("Bu fatura tipi iade edilemez.")
            };

            // 3. Fatura Numarası Üretimi (Dün yaptığımız otomatik artan mantığın aynısı)
            string year = DateTime.Now.Year.ToString();
            string prefix = $"RT-{year}-"; 

            var lastInvoices = await _invoiceRepository.ToListAsync(
                _invoiceRepository.Where(x => x.InvoiceNumber.StartsWith(prefix)).OrderByDescending(x => x.InvoiceNumber).Take(1),
                cancellationToken);

            var lastInvoice = lastInvoices.FirstOrDefault();
            string invoiceNumber = lastInvoice == null
                ? $"{prefix}00000001"
                : $"{prefix}{(long.Parse(lastInvoice.InvoiceNumber.Replace(prefix, "")) + 1):D8}";

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

            // 5. Miktar Kontrolü: Almadığı veya satmadığı malı iade etmesin
            foreach (var reqDetail in request.Details)
            {
                // Orijinal faturada bu ürün var mı ve miktarı ne kadar?
                var originalDetail = parentInvoice.Details.FirstOrDefault(x => x.ProductId == reqDetail.ProductId);

                if (originalDetail == null)
                    return Result<string>.Failure("İade edilmek istenen ürün orijinal faturada yok.");

                if (reqDetail.ReturnQuantity > originalDetail.Quantity)
                    return Result<string>.Failure($"Hata: {reqDetail.ProductId} ürününden en fazla {originalDetail.Quantity} adet iade edebilirsiniz.");

                if (reqDetail.ReturnQuantity <= 0)
                    return Result<string>.Failure("İade miktarı sıfır veya eksi olamaz.");

                // Kontrolden geçti, iade faturasına kalemi ekle
                returnInvoice.AddDetail(new InvoiceDetail(
                    reqDetail.ProductId,
                    reqDetail.WarehouseId,
                    reqDetail.ReturnQuantity, 
                    reqDetail.UnitPrice,     
                    reqDetail.DiscountRate,
                    reqDetail.VatRate
                ));
            }

            // 6. Kaydet ve bitir
            await _invoiceRepository.AddAsync(returnInvoice, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<string>.Success(returnInvoice.InvoiceNumber, $"İade faturası taslağı oluşturuldu. No: {invoiceNumber}");
        }
    }
}
