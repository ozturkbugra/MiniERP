using MediatR;
using MiniERP.Domain.Common;
using MiniERP.Domain.Enums;

namespace MiniERP.Application.Features.Cashs.Commands.UpdateCash
{
    public sealed record UpdateCashCommand(Guid Id, string Name, CurrencyType CurrencyType) : IRequest<Result<string>>;
}
