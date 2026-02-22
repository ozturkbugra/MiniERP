using MediatR;
using MiniERP.Domain.Common;

namespace MiniERP.Application.Features.Users.Commands.UpdateUser;

public sealed record UpdateUserCommand(
    string Id,
    string FirstName,
    string LastName,
    string Email,
    string UserName) : IRequest<Result<string>>;