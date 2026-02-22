using MediatR;
using MiniERP.Application.Interfaces;
using MiniERP.Domain.Common;

namespace MiniERP.Application.Features.Auth.Commands.LoginCommands;

public sealed class LoginCommandHandler : IRequestHandler<LoginCommand, Result<string>>
{
    private readonly IAuthService _authService;
    public LoginCommandHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<Result<string>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        return await _authService.LoginAsync(request.Email, request.Password);
    }
}