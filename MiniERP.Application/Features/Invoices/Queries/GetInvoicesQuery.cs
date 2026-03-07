using MediatR;
using MiniERP.Domain.Common;

namespace MiniERP.Application.Features.Invoices.Queries
{
    public sealed record GetInvoicesQuery() : IRequest<Result<List<GetInvoicesResponse>>>;
}
