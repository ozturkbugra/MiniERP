using MediatR;
using MiniERP.Application.Interfaces;
using MiniERP.Domain.Common;
using MiniERP.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniERP.Application.Features.Roles.Queries.GetPermissionsByRoleId
{
    public sealed class GetPermissionsByRoleIdHandler : IRequestHandler<GetPermissionsByRoleIdQuery, Result<List<string>>>
    {
        private readonly IRepository<RolePermission> _rolePermissionRepository;

        public GetPermissionsByRoleIdHandler(IRepository<RolePermission> rolePermissionRepository)
        {
            _rolePermissionRepository = rolePermissionRepository;
        }

        public async Task<Result<List<string>>> Handle(GetPermissionsByRoleIdQuery request, CancellationToken cancellationToken)
        {
            // Repo üzerinden o role ait tüm yetkileri filtreleyerek çekiyoruz
            var permissions = await _rolePermissionRepository.GetAllAsync(
                x => x.RoleId == request.RoleId,
                cancellationToken);

            // Sadece yetki isimlerini (string listesi) seçip dönüyoruz
            var permissionNames = permissions
                .Select(x => x.Permission)
                .ToList();

            return Result<List<string>>.Success(permissionNames,"Rol yetkileri başarıyla çekildi.");
        }
    }
}
