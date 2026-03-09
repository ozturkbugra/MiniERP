using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace MiniERP.Persistence.Security
{
    public class PermissionPolicyProvider : DefaultAuthorizationPolicyProvider
    {
        public PermissionPolicyProvider(IOptions<AuthorizationOptions> options) : base(options)
        {
        }

        public override async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
        {
            // Önce ASP.NET'in kendi standart policy'si var mı diye bak
            var policy = await base.GetPolicyAsync(policyName);
            if (policy != null)
            {
                return policy;
            }

            // Yoksa bizim dinamik PermissionRequirement'ı oluştur ve sisteme tanıt
            return new AuthorizationPolicyBuilder()
                .AddRequirements(new PermissionRequirement(policyName))
                .Build();
        }
    }
}
