using MediatR;
using MiniERP.Domain.Common;

namespace MiniERP.Application.Features.Users.Commands.CreateUser;

public sealed record CreateUserCommand(
    string FirstName,
    string LastName,
    string Email,
    string Password) : IRequest<Result>;