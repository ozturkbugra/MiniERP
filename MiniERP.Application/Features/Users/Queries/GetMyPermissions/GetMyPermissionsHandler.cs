using MediatR;
using MiniERP.Application.Interfaces;
using MiniERP.Domain.Common;

namespace MiniERP.Application.Features.Users.Queries.GetMyPermissions
{
    public sealed class GetMyPermissionsHandler : IRequestHandler<GetMyPermissionsQuery, Result<List<string>>>
    {
        private readonly IAuthService _authService;

        public GetMyPermissionsHandler(IAuthService authService)
        {
            _authService = authService;
        }

        public async Task<Result<List<string>>> Handle(GetMyPermissionsQuery request, CancellationToken cancellationToken)
        {
            return await _authService.GetMyPermissionsAsync(request.UserId.ToString(), cancellationToken);
        }
    }
}
