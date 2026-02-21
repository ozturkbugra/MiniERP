using MediatR;
using MiniERP.Domain.Common;

namespace MiniERP.Application.Features.Roles.Commands;

public sealed record CreateRoleCommand(
    string Name,
    string Description
   ) : IRequest<Result>;