using Microsoft.AspNetCore.Authorization;

namespace MiniERP.Persistence.Security
{
    public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
        {
            // Kullanıcının Token'ının içinde istenen yetki var mı kontrol et
            if (context.User.HasClaim(c => c.Type == "Permission" && c.Value == requirement.Permission))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask; // Yetki yoksa 403 Forbidden'a düşer
        }
    }
}
