using MediatR;
using MiniERP.Application.Interfaces;
using MiniERP.Domain.Common;

namespace MiniERP.Application.Features.Users.Queries.GetAllUsers
{
    public sealed class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, Result<List<GetAllUsersQueryResponse>>>
    {
        private readonly IAuthService _authService;

        public GetAllUsersQueryHandler(IAuthService authService)
        {
            _authService = authService;
        }

        public async Task<Result<List<GetAllUsersQueryResponse>>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
        {
            return await _authService.GetAllUsersAsync(cancellationToken);
        }
    }
}
