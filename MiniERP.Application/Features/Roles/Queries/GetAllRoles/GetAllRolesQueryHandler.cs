using MediatR;
using MiniERP.Application.Interfaces;
using MiniERP.Domain.Common;

namespace MiniERP.Application.Features.Roles.Queries.GetAllRoles
{
    public sealed class GetAllRolesQueryHandler : IRequestHandler<GetAllRolesQuery, Result<List<GetAllRolesQueryResponse>>>
    {
        private readonly IRoleService _roleService; // Yeni servisimiz

        public GetAllRolesQueryHandler(IRoleService roleService)
        {
            _roleService = roleService;
        }

        public async Task<Result<List<GetAllRolesQueryResponse>>> Handle(GetAllRolesQuery request, CancellationToken cancellationToken)
        {
            return await _roleService.GetAllRolesAsync(cancellationToken);
        }
    }
}
