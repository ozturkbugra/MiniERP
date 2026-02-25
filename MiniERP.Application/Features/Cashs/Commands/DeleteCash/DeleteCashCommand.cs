using MediatR;
using MiniERP.Domain.Common;

namespace MiniERP.Application.Features.Cashs.Commands.DeleteCash
{
    public sealed record DeleteCashCommand(Guid Id) : IRequest<Result<string>>;
}
