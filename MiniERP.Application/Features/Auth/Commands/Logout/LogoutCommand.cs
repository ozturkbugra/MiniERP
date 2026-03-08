using MediatR;

namespace MiniERP.Application.Features.Auth.Commands.Logout
{
    public sealed record LogoutCommand() : IRequest<string>;
}
