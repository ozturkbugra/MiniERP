using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MiniERP.Application.Interfaces;
using MiniERP.Persistence.IdentityModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniERP.Persistence.Services
{
    public sealed class AppUserService : IAppUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public AppUserService(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<Dictionary<string, string>> GetUserNamesByIdsAsync(List<string> userIds, CancellationToken cancellationToken = default)
        {
            if (userIds == null || !userIds.Any())
                return new Dictionary<string, string>();

            return await _userManager.Users
                .Where(u => userIds.Contains(u.Id.ToString()))
                .ToDictionaryAsync(
                    u => u.Id.ToString(),
                    u => u.FirstName + " " + u.LastName,
                    cancellationToken);
        }
    }
}
