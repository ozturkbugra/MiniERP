using MediatR;
using MiniERP.Domain.Common;

namespace MiniERP.Application.Features.Invoices.Queries.GetInvoiceById
{
    public sealed record GetInvoiceByIdQuery(Guid Id) : IRequest<Result<GetInvoiceByIdResponse>>;
}
