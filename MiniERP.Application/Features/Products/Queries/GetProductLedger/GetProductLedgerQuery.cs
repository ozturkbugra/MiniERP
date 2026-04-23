using MediatR;
using MiniERP.Domain.Common;

namespace MiniERP.Application.Features.Products.Queries.GetProductLedger
{
    public sealed record GetProductLedgerQuery(Guid ProductId) : IRequest<Result<List<ProductLedgerResponse>>>;
}
