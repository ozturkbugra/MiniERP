using MediatR;
using MiniERP.Domain.Common;

namespace MiniERP.Application.Features.Users.Commands.ChangePassword
{
    public sealed record ChangePasswordCommand(
    string UserId,
    string OldPassword,
    string NewPassword) : IRequest<Result<string>>;
}
