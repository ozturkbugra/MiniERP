using MiniERP.Domain.Common;

namespace MiniERP.Application.Interfaces;

public interface IRoleService
{
    Task<Result> CreateRoleAsync(string name, string description);
}