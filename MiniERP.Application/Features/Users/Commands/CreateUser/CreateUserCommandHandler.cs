using MediatR;
using MiniERP.Application.Interfaces;
using MiniERP.Domain.Common;

namespace MiniERP.Application.Features.Users.Commands.CreateUser;
public sealed class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Result>
{
    private readonly IAuthService _authService;

    public CreateUserCommandHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<Result> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var response = await _authService.RegisterAsync(
            request.FirstName,
            request.LastName,
            request.Email,
            request.Password);

        return response;
    }
}