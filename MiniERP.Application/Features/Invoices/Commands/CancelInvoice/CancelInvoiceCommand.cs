using MediatR;
using MiniERP.Domain.Common;

namespace MiniERP.Application.Features.Invoices.Commands.CancelInvoice
{
    public sealed record CancelInvoiceCommand(Guid Id) : IRequest<Result<string>>;
}
