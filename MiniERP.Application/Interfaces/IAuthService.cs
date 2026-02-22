using MiniERP.Application.Features.Users.Queries.GetAllUsers;
using MiniERP.Application.Features.Users.Queries.GetUserById;
using MiniERP.Domain.Common;

namespace MiniERP.Application.Interfaces;

public interface IAuthService
{
    Task<Result> RegisterAsync(string firstName, string lastName, string email, string password);

    Task<Result> AssignRoleAsync(string userId, string roleName);

    Task<Result<string>> LoginAsync(string email, string password);

    Task<Result<List<GetAllUsersQueryResponse>>> GetAllUsersAsync(CancellationToken cancellationToken);

    Task<Result<GetUserByIdQueryResponse>> GetUserByIdAsync(string id, CancellationToken cancellationToken);
}
