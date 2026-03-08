using MediatR;
using Microsoft.IdentityModel.JsonWebTokens;
using MiniERP.Application.Interfaces;
using MiniERP.Domain.Entities;

namespace MiniERP.Application.Features.Auth.Commands.Logout
{
    public sealed class LogoutCommandHandler : IRequestHandler<LogoutCommand, string>
    {
        private readonly IAuthService _authService;

        public LogoutCommandHandler(IAuthService authService)
        {
            _authService = authService;
        }

        public async Task<string> Handle(LogoutCommand request, CancellationToken cancellationToken)
        {
            return await _authService.LogoutAsync();
        }
    }
}
