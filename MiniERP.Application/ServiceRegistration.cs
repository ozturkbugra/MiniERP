using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using MiniERP.Application.Behaviors;
using System.Reflection;

namespace MiniERP.Application;

public static class ServiceRegistration
{
    public static void AddApplicationServices(this IServiceCollection services)
    {
        // 1. MediatR ve Behavior Kaydı
        services.AddMediatR(cfg =>
        {
            // Mevcut Handler'ları bulur
            cfg.RegisterServicesFromAssembly(typeof(ServiceRegistration).Assembly);

            // Pipeline Behavioru devreye sokar
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        // Bu satır, projedeki tüm AbstractValidator'ları (CreateUserCommandValidator vb.) otomatik bulup sisteme tanıtır.
        services.AddValidatorsFromAssembly(typeof(ServiceRegistration).Assembly);

        services.AddAutoMapper(Assembly.GetExecutingAssembly());
    }
}