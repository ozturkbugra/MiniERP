using MediatR;
using MiniERP.Domain.Common;

namespace MiniERP.Application.Features.Banks.Queries.GetBankById
{
    public sealed record GetBankByIdQuery(Guid Id) : IRequest<Result<BankResponse>>;
}
