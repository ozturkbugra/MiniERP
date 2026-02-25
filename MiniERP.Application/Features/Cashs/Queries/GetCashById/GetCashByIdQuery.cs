using MediatR;
using MiniERP.Domain.Common;

namespace MiniERP.Application.Features.Cashs.Queries.GetCashById
{
    public sealed record GetCashByIdQuery(Guid Id) : IRequest<Result<CashByIdResponse>>;
}
