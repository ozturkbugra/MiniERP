using MediatR;
using MiniERP.Domain.Common;

namespace MiniERP.Application.Features.Units.Commands.DeleteUnit
{
    public sealed record DeleteUnitCommand(Guid Id) : IRequest<Result<string>>;
}
