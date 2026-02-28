using MediatR;
using MiniERP.Domain.Common;

namespace MiniERP.Application.Features.Units.Commands.CreateUnit
{
    public sealed record CreateUnitCommand(string Code, string Name) : IRequest<Result<string>>;
}
