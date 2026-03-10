using MediatR;
using MiniERP.Application.Interfaces;
using MiniERP.Domain.Common;
using MiniERP.Domain.Entities;

namespace MiniERP.Application.Features.Roles.Commands.AssignPermissions
{
    public sealed class AssignPermissionsToRoleHandler : IRequestHandler<AssignPermissionsToRoleCommand, Result<string>>
    {
        private readonly IRepository<RolePermission> _rolePermissionRepository;
        private readonly IRoleService _roleService;
        private readonly IUnitOfWork _unitOfWork; 

        public AssignPermissionsToRoleHandler(
            IRepository<RolePermission> rolePermissionRepository,
            IRoleService roleService,
            IUnitOfWork unitOfWork)
        {
            _rolePermissionRepository = rolePermissionRepository;
            _roleService = roleService;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<string>> Handle(AssignPermissionsToRoleCommand request, CancellationToken cancellationToken)
        {
            
            var roleResult = await _roleService.GetRoleByIdAsync(request.RoleId.ToString(), cancellationToken);

            if (!roleResult.IsSuccess)
            {
                return Result<string>.Failure("Rol bulunamadı.");
            }

            var existingPermissions = await _rolePermissionRepository.GetAllAsync(
                x => x.RoleId == request.RoleId,
                cancellationToken);

            if (existingPermissions.Any())
            {
                foreach (var permission in existingPermissions)
                {
                    _rolePermissionRepository.Delete(permission);
                }
            }

            if (request.Permissions != null && request.Permissions.Any())
            {
                foreach (var perm in request.Permissions)
                {
                    var newPermission = new RolePermission
                    {
                        RoleId = request.RoleId,
                        Permission = perm,
                        CreatedDate = DateTime.Now,
                        IsDeleted = false
                    };

                    await _rolePermissionRepository.AddAsync(newPermission, cancellationToken);
                }
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<string>.Success(request.RoleId.ToString(),"Yetkiler Başarıyla Atandı.");
        }
    }
}
