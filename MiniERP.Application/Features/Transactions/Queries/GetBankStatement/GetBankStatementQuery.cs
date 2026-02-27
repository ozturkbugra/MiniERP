using MediatR;
using MiniERP.Domain.Common;

namespace MiniERP.Application.Features.Transactions.Queries.GetBankStatement
{
    public sealed record GetBankStatementQuery(Guid BankId) : IRequest<Result<List<BankStatementResponse>>>;
}
