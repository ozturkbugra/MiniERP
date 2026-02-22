using MediatR;
using MiniERP.Application.Interfaces;
using MiniERP.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniERP.Application.Features.Users.Commands.UpdateUser
{
    public sealed class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, Result<string>>
    {
        private readonly IAuthService _authService;

        public UpdateUserCommandHandler(IAuthService authService)
        {
            _authService = authService;
        }

        public async Task<Result<string>> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
        {
            return await _authService.UpdateUserAsync(request, cancellationToken);
        }
    }
}
