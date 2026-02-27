using MediatR;
using MiniERP.Domain.Common;

namespace MiniERP.Application.Features.Transactions.Queries.GetCashStatement
{
    public sealed record GetCashStatementQuery(Guid CashId) : IRequest<Result<List<CashStatementResponse>>>;
}
