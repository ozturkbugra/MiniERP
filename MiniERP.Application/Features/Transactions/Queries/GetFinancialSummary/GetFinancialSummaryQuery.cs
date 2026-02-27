using MediatR;
using MiniERP.Domain.Common;

namespace MiniERP.Application.Features.Transactions.Queries.GetFinancialSummary
{
    public sealed record GetFinancialSummaryQuery() : IRequest<Result<FinancialSummaryResponse>>;
}
