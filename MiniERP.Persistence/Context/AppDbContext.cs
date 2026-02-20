using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MiniERP.Persistence.IdentityModels;

namespace MiniERP.Persistence.Context;

public sealed class AppDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        // Identity tablolarının (AspNetUsers vb.) oluşması için
        base.OnModelCreating(builder);

        // İleride kendi Domain sınıflarımızı (Örn: Products) buraya DbSet olarak eklediğimizde,
        // Fluent API ayarlarını burada yapacağız.
    }
}