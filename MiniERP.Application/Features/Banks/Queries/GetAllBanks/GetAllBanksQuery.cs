using MediatR;
using MiniERP.Domain.Common;

namespace MiniERP.Application.Features.Banks.Queries.GetAllBanks
{
    public sealed record GetAllBanksQuery() : IRequest<Result<List<BankResponse>>>;
}
