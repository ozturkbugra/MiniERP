namespace MiniERP.Application.Features.Users.Queries.GetUserById;

public sealed record GetUserByIdQueryResponse(
    string Id,
    string FirstName,
    string LastName,
    string Email,
    string UserName,
    bool IsDeleted,
    List<string> Roles);