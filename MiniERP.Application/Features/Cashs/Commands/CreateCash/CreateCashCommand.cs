using MediatR;
using MiniERP.Domain.Common;
using MiniERP.Domain.Enums;

namespace MiniERP.Application.Features.Cash.Commands.CreateCash
{
    public sealed record CreateCashCommand(string Name, CurrencyType CurrencyType) : IRequest<Result<string>>;
}
