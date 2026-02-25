using MediatR;
using MiniERP.Domain.Common;

namespace MiniERP.Application.Features.Cashs.Queries.GetAllCashes
{
    public sealed record GetAllCashesQuery() : IRequest<Result<List<CashResponse>>>;
}
