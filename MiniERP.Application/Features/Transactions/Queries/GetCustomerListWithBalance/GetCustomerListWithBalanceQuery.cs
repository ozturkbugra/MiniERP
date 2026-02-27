using MediatR;
using MiniERP.Domain.Common;

namespace MiniERP.Application.Features.Transactions.Queries.GetCustomerListWithBalance
{
    public sealed record GetCustomerListWithBalanceQuery() : IRequest<Result<List<CustomerListWithBalanceResponse>>>;
}
