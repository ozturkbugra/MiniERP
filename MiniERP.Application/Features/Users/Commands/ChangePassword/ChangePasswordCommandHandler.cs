using MediatR;
using MiniERP.Application.Interfaces;
using MiniERP.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniERP.Application.Features.Users.Commands.ChangePassword
{
    public sealed class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, Result<string>>
    {
        private readonly IAuthService _authService; 

        public ChangePasswordCommandHandler(IAuthService authService) => _authService = authService;

        public async Task<Result<string>> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
        {
            return await _authService.ChangePasswordAsync(request, cancellationToken);
        }
    }
}
