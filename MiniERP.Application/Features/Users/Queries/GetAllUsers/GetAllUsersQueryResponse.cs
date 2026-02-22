namespace MiniERP.Application.Features.Users.Queries.GetAllUsers
{
    public sealed record GetAllUsersQueryResponse(
     string Id,
     string FirstName,
     string LastName,
     string Email,
     string UserName,
     bool IsDeleted,
     List<string> Roles);
}
