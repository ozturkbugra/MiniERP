using MediatR;
using MiniERP.Domain.Common;

namespace MiniERP.Application.Features.Users.Commands.DeleteUser
{
    public sealed record DeleteUserCommand(string Id) : IRequest<Result<string>>;
}
