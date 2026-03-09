using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MiniERP.Domain.Entities;
using MiniERP.Domain.Enums;
using MiniERP.Persistence.Context;
using MiniERP.Persistence.IdentityModels;

namespace MiniERP.Persistence;

public static class DbInitializer
{
    public static async Task SeedAsync(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager, AppDbContext context)
    {
        var adminRoleName = "Admin";

        // 1. Admin Rolü Kontrolü
        if (!await roleManager.RoleExistsAsync(adminRoleName))
        {
            await roleManager.CreateAsync(new ApplicationRole { Name = adminRoleName });
        }

        var adminRole = await roleManager.FindByNameAsync(adminRoleName);

        // 2. Admin Kullanıcı Kontrolü
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

            var result = await userManager.CreateAsync(user, "123456");

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, adminRoleName);
            }
        }

        // 3. YETKİLERİ (RolePermissions) DOLDURMA
        if (adminRole != null && !await context.RolePermissions.AnyAsync(x => x.RoleId == adminRole.Id))
        {
            var allPermissions = new List<string>
            {
                // Raporlar
                AppPermissions.Reports.View,

                // Stok Yönetimi (Senin AppPermissions'daki Stocks sınıfı)
                AppPermissions.Stocks.View,
                AppPermissions.Stocks.Create,
                AppPermissions.Stocks.Delete,

                // Finans & Kasa
                AppPermissions.Finance.View,
                AppPermissions.Finance.Transaction,

                // Kullanıcı Yönetimi
                AppPermissions.Users.View,
                AppPermissions.Users.Create,
                AppPermissions.Users.Update,
                AppPermissions.Users.Delete,

                // Rol & Yetki Yönetimi
                AppPermissions.Roles.View,
                AppPermissions.Roles.Create,
                AppPermissions.Roles.Update,
                AppPermissions.Roles.Delete,

                // Banka Yetkileri
                AppPermissions.Banks.View,
                AppPermissions.Banks.Create,
                AppPermissions.Banks.Update,
                AppPermissions.Banks.Delete,

                // Kasa Yetkileri
                AppPermissions.Cashes.View,
                AppPermissions.Cashes.Create,
                AppPermissions.Cashes.Update,
                AppPermissions.Cashes.Delete,

                // Marka Yetkileri
                AppPermissions.Brands.View,
                AppPermissions.Brands.Create,
                AppPermissions.Brands.Update,
                AppPermissions.Brands.Delete,

                // Kategori Yetkileri
                AppPermissions.Categories.View,
                AppPermissions.Categories.Create,
                AppPermissions.Categories.Update,
                AppPermissions.Categories.Delete,

                // Cari Yetkileri
                AppPermissions.Customers.View,
                AppPermissions.Customers.Create,
                AppPermissions.Customers.Update,
                AppPermissions.Customers.Delete,

                //Fatura Yetkileri
                AppPermissions.Invoices.View,
                AppPermissions.Invoices.Create,
                AppPermissions.Invoices.Approve,
                AppPermissions.Invoices.Cancel,
                AppPermissions.Invoices.Return,

                // Sipariş Yetkileri
                AppPermissions.Orders.View,
                AppPermissions.Orders.Create,
                AppPermissions.Orders.Approve,
                AppPermissions.Orders.Cancel,

                // Ürün Yetkileri
                AppPermissions.Products.View,
                AppPermissions.Products.Create,
                AppPermissions.Products.Update,
                AppPermissions.Products.Delete,
            };

            var rolePermissions = allPermissions.Select(permission => new RolePermission
            {
                RoleId = adminRole.Id,
                Permission = permission,
                CreatedDate = DateTime.Now,
                CreatedBy = "System",
                IsDeleted = false
            }).ToList();

            await context.RolePermissions.AddRangeAsync(rolePermissions);
            await context.SaveChangesAsync();
        }
    }
}