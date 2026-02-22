using Microsoft.AspNetCore.Identity;
using MiniERP.Persistence.IdentityModels; // Kendi User/Role modellerini buraya ekle

namespace MiniERP.Persistence;

public static class DbInitializer
{
    public static async Task SeedAsync(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager)
    {
        // 1. Admin Rolü var mı? Yoksa oluştur.
        if (!await roleManager.RoleExistsAsync("Admin"))
        {
            await roleManager.CreateAsync(new ApplicationRole { Name = "Admin" });
        }

        // 2. Admin Kullanıcısı var mı? Yoksa oluştur.
        var adminEmail = "admin@minierp.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser == null)
        {
            var user = new ApplicationUser
            {
                UserName = "admin",
                Email = adminEmail,
                FirstName = "Sistem",
                LastName = "Admini",
                EmailConfirmed = true
            };

            // Şifreyi "123456" yapıyoruz
            var result = await userManager.CreateAsync(user, "123456");

            if (result.Succeeded)
            {
                // Kullanıcıyı Admin rolüne ata
                await userManager.AddToRoleAsync(user, "Admin");
            }
        }
    }
}