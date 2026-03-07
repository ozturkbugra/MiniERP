using MediatR;
using MiniERP.Application.Interfaces;
using MiniERP.Domain.Common;
using MiniERP.Domain.Entities;
using CustomerEntity = MiniERP.Domain.Entities.Customer;
namespace MiniERP.Application.Features.Invoices.Queries.GetInvoiceById
{
    public sealed class GetInvoiceByIdQueryHandler : IRequestHandler<GetInvoiceByIdQuery, Result<GetInvoiceByIdResponse>>
    {
        private readonly IRepository<Invoice> _invoiceRepository;
        private readonly IRepository<CustomerEntity> _customerRepository;
        private readonly IRepository<Warehouse> _warehouseRepository;
        private readonly IRepository<Product> _productRepository;
        private readonly IAppUserService _appUserService;

        public GetInvoiceByIdQueryHandler(IRepository<Invoice> invoiceRepository, IRepository<CustomerEntity> customerRepository, IRepository<Warehouse> warehouseRepository, IRepository<Product> productRepository, IAppUserService appUserService)
        {
            _invoiceRepository = invoiceRepository;
            _customerRepository = customerRepository;
            _warehouseRepository = warehouseRepository;
            _productRepository = productRepository;
            _appUserService = appUserService;
        }

        public async Task<Result<GetInvoiceByIdResponse>> Handle(GetInvoiceByIdQuery request, CancellationToken cancellationToken)
        {
            // 1. Faturayı detaylarıyla beraber çekiyoruz
            var invoice = await _invoiceRepository.GetByIdWithIncludesAsync(request.Id, cancellationToken, x => x.Details);
            if (invoice == null) return Result<GetInvoiceByIdResponse>.Failure("Fatura bulunamadı.");

            // 2. ID'leri Toployıruz
            var productIds = invoice.Details.Select(x => x.ProductId).Distinct().ToList();
            var warehouseIds = invoice.Details.Select(x => x.WarehouseId).Distinct().ToList();
            if (invoice.WarehouseId.HasValue) warehouseIds.Add(invoice.WarehouseId.Value);

            // 3. Dictionaryler
            var productDict = (await _productRepository.ToListAsync(_productRepository.Where(x => productIds.Contains(x.Id)), cancellationToken))
                               .ToDictionary(x => x.Id, x => x.Name);

            var warehouseDict = (await _warehouseRepository.ToListAsync(_warehouseRepository.Where(x => warehouseIds.Contains(x.Id)), cancellationToken))
                                .ToDictionary(x => x.Id, x => x.Name);

            var customerName = (await _customerRepository.GetByIdAsync(invoice.CustomerId, cancellationToken))?.Name ?? "Tanımsız";

            var userNames = await _appUserService.GetUserNamesByIdsAsync(new List<string?> { invoice.CreatedBy }, cancellationToken);

            // 4. Mapping
            var response = new GetInvoiceByIdResponse(
                invoice.Id,
                invoice.InvoiceNumber,
                invoice.InvoiceDate,
                customerName,
                invoice.WarehouseId.HasValue && warehouseDict.TryGetValue(invoice.WarehouseId.Value, out var wName) ? wName : "Genel Depo",
                invoice.TotalGross,
                invoice.TotalDiscount,
                invoice.TotalVat,
                invoice.GrandTotal,
                invoice.Status.ToString(),
                userNames.TryGetValue(invoice.CreatedBy ?? "", out var uName) ? uName : "Sistem",
                invoice.Details.Select(d => new InvoiceLineResponse(
                    d.ProductId,
                    productDict.TryGetValue(d.ProductId, out var pName) ? pName : "Bilinmeyen Ürün",
                    d.Quantity,
                    d.UnitPrice,
                    d.DiscountRate,
                    d.VatRate,
                    d.LineTotal,
                    warehouseDict.TryGetValue(d.WarehouseId, out var lineWName) ? lineWName : "Bilinmeyen Depo"
                )).ToList()
            );

            return Result<GetInvoiceByIdResponse>.Success(response,"Fatura başarıyla getirildi.");
        }
    }
}
