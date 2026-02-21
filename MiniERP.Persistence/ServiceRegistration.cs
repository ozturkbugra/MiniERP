using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MiniERP.Persistence.Context;
using MiniERP.Persistence.IdentityModels;
using System.Reflection;

namespace MiniERP.Persistence;

public static class ServiceRegistration
{
    public static void AddPersistenceServices(this IServiceCollection services, IConfiguration configuration)
    {
        // 1. DbContext Kaydı
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
        {
            // Şifre kurallarını esnettik geliştirme yaparken zorlamasın diye
            options.Password.RequiredLength = 3;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireDigit = false;
            options.Password.RequireLowercase = false;
            options.Password.RequireUppercase = false;
        })
        .AddEntityFrameworkStores<AppDbContext>();
    }

    public static void AddApplicationServices(this IServiceCollection services)
    {
        // Bu katmandaki tüm MediatR Handler'larını otomatik bul ve sisteme kaydet!
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
    }
}