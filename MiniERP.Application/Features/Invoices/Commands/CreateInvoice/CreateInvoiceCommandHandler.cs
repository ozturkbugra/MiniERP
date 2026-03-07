using MediatR;
using MiniERP.Application.Interfaces;
using MiniERP.Domain.Common;
using MiniERP.Domain.Entities;

namespace MiniERP.Application.Features.Invoices.Commands.CreateInvoice
{
    public sealed class CreateInvoiceCommandHandler : IRequestHandler<CreateInvoiceCommand, Result<string>>
    {
        private readonly IRepository<Invoice> _invoiceRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateInvoiceCommandHandler(IRepository<Invoice> invoiceRepository, IUnitOfWork unitOfWork)
        {
            _invoiceRepository = invoiceRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<string>> Handle(CreateInvoiceCommand request, CancellationToken cancellationToken)
        {
            string invoiceNumber = $"FT-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 5).ToUpper()}";

            var invoice = new Invoice(
                invoiceNumber,
                request.InvoiceDate,
                request.Type,
                request.CustomerId,
                request.OrderId
            );

            foreach (var detailDto in request.Details)
            {
                var detail = new InvoiceDetail(
                    detailDto.ProductId,
                    detailDto.WarehouseId,
                    detailDto.Quantity,
                    detailDto.UnitPrice,
                    detailDto.DiscountRate,
                    detailDto.VatRate
                );
                invoice.AddDetail(detail);
                
            }

            await _invoiceRepository.AddAsync(invoice);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<string>.Success(request.InvoiceDate.ToString("dd/MM/yyyy HH:mm"),$"Fatura taslağı başarıyla oluşturuldu. Fatura No: {invoiceNumber}");
        }
    }
}
