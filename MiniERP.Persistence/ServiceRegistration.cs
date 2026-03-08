using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MiniERP.Application.Interfaces;
using MiniERP.Persistence.Context;
using MiniERP.Persistence.IdentityModels;
using MiniERP.Persistence.Interceptors;
using MiniERP.Persistence.Services;
using System.Reflection;

namespace MiniERP.Persistence;

public static class ServiceRegistration
{
    public static void AddPersistenceServices(this IServiceCollection services, IConfiguration configuration)
    {
        // 1. Önce Interceptor'ı Singleton olarak kaydediyoruz
        services.AddSingleton<AuditInterceptor>();

        // 2. DbContext Kaydı ve Interceptor Bağlantısı
        services.AddDbContext<AppDbContext>((sp, options) =>
        {
            var auditInterceptor = sp.GetRequiredService<AuditInterceptor>();

            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"))
                   .AddInterceptors(auditInterceptor);
        });



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

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<IJwtProvider, JwtProvider>();
        services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IAppUserService, AppUserService>();
    }
}