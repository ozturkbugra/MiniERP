namespace MiniERP.Application.Features.Roles.Queries.GetAllPermissions
{
    public sealed record PermissionGroupResponse(
    string ModuleName,
    List<string> Permissions);
}
