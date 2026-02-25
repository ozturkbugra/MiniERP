using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MiniERP.Domain.Entities;
using MiniERP.Persistence.IdentityModels;
using System.Reflection;

namespace MiniERP.Persistence.Context;

public sealed class AppDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Customer> Customers { get; set; }
    public DbSet<Cash> Cashes { get; set; }
    public DbSet<Bank> Banks { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        // Identity tablolarının (AspNetUsers vb.) oluşması için
        base.OnModelCreating(builder);

        // Yazdığımız tüm IEntityTypeConfiguration sınıflarını otomatik uygula
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}