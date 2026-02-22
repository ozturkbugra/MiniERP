using MiniERP.Application.Features.Roles.Queries.GetAllRoles;
using MiniERP.Domain.Common;

namespace MiniERP.Application.Interfaces;

public interface IRoleService
{
    Task<Result> CreateRoleAsync(string name, string description);

    Task<Result<List<GetAllRolesQueryResponse>>> GetAllRolesAsync(CancellationToken cancellationToken);
}
