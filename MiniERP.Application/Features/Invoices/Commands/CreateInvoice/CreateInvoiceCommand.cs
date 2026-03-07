using MediatR;
using MiniERP.Domain.Common;
using MiniERP.Domain.Enums;

namespace MiniERP.Application.Features.Invoices.Commands.CreateInvoice
{
    public sealed record CreateInvoiceCommand(
    InvoiceType Type,
    DateTime InvoiceDate,
    Guid CustomerId,
    Guid? OrderId,
    List<CreateInvoiceDetailResponse> Details
) : IRequest<Result<string>>;
}
