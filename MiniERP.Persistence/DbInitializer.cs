using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MiniERP.Domain.Entities;
using MiniERP.Persistence.Context;
using MiniERP.Persistence.IdentityModels;

namespace MiniERP.Persistence;

public static class DbInitializer
{
    public static async Task SeedAsync(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager, AppDbContext context)
    {
        var adminRoleName = "Admin";

        // 1. Admin Rolü var mı? Yoksa oluştur.
        if (!await roleManager.RoleExistsAsync(adminRoleName))
        {
            await roleManager.CreateAsync(new ApplicationRole { Name = adminRoleName });
        }

        // Rolün ID'sini alabilmek için veritabanından çekiyoruz (Birazdan yetki atarken lazım olacak)
        var adminRole = await roleManager.FindByNameAsync(adminRoleName);

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

            var result = await userManager.CreateAsync(user, "123456");

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, adminRoleName);
            }
        }

        // 3. YETKİLERİ (RolePermissions) DOLDURMA ZAMANI!
        // Eğer bu rolün üzerine atanmış hiçbir yetki yoksa, tüm anahtarları ver.
        if (adminRole != null && !await context.RolePermissions.AnyAsync(x => x.RoleId == adminRole.Id))
        {
            // Controller listene göre tüm yetki matrisini oluşturuyoruz
            var allPermissions = new List<string>
            {
                // Raporlar (Sadece Görme)
                "Permissions.Reports.View",

                // Stok & Ürün Yönetimi
                "Permissions.Products.View", "Permissions.Products.Create", "Permissions.Products.Update", "Permissions.Products.Delete",
                "Permissions.Categories.View", "Permissions.Categories.Create", "Permissions.Categories.Update", "Permissions.Categories.Delete",
                "Permissions.Brands.View", "Permissions.Brands.Create", "Permissions.Brands.Update", "Permissions.Brands.Delete",
                "Permissions.Units.View", "Permissions.Units.Create", "Permissions.Units.Update", "Permissions.Units.Delete",
                "Permissions.Warehouses.View", "Permissions.Warehouses.Create", "Permissions.Warehouses.Update", "Permissions.Warehouses.Delete",
                "Permissions.StockTransactions.View", "Permissions.StockTransactions.Create", "Permissions.StockTransactions.Update", "Permissions.StockTransactions.Delete",

                // Finans Yönetimi
                "Permissions.Banks.View", "Permissions.Banks.Create", "Permissions.Banks.Update", "Permissions.Banks.Delete",
                "Permissions.Cashes.View", "Permissions.Cashes.Create", "Permissions.Cashes.Update", "Permissions.Cashes.Delete",
                "Permissions.Transactions.View", "Permissions.Transactions.Create", "Permissions.Transactions.Update", "Permissions.Transactions.Delete",

                // Müşteri & Satış
                "Permissions.Customers.View", "Permissions.Customers.Create", "Permissions.Customers.Update", "Permissions.Customers.Delete",
                "Permissions.Orders.View", "Permissions.Orders.Create", "Permissions.Orders.Update", "Permissions.Orders.Delete",
                "Permissions.Invoices.View", "Permissions.Invoices.Create", "Permissions.Invoices.Update", "Permissions.Invoices.Delete",

                // Sistem Yönetimi
                "Permissions.Roles.View", "Permissions.Roles.Create", "Permissions.Roles.Update", "Permissions.Roles.Delete",
                "Permissions.Auths.View" // Gerekirse vs.
            };

            // String listesini bizim 'RolePermission' entity'mize dönüştürüyoruz
            var rolePermissions = allPermissions.Select(permission => new RolePermission
            {
                RoleId = adminRole.Id,
                Permission = permission,
                CreatedDate = DateTime.Now,
                CreatedBy = "System", 
                IsDeleted = false
            }).ToList();

            // Veritabanına topluca basıyoruz
            await context.RolePermissions.AddRangeAsync(rolePermissions);
            await context.SaveChangesAsync();
        }
    }
}