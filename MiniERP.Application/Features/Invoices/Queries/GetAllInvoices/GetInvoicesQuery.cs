using MediatR;
using MiniERP.Domain.Common;

namespace MiniERP.Application.Features.Invoices.Queries.GetAllInvoices
{
    public sealed record GetInvoicesQuery() : IRequest<Result<List<GetInvoicesResponse>>>;
}
