using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MiniERP.Domain.Common;
using MiniERP.Domain.Entities;
using MiniERP.Persistence.IdentityModels;
using System.Reflection;
using System.Security.Claims;

namespace MiniERP.Persistence.Context;

public sealed class AppDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
{

    private readonly IHttpContextAccessor _httpContextAccessor;

    public AppDbContext(DbContextOptions<AppDbContext> options, IHttpContextAccessor httpContextAccessor) : base(options)
    {
        _httpContextAccessor = httpContextAccessor;
    }
   

    public DbSet<Customer> Customers { get; set; }
    public DbSet<Cash> Cashes { get; set; }
    public DbSet<Bank> Banks { get; set; }
    public DbSet<CashTransaction> CashTransactions { get; set; }
    public DbSet<BankTransaction> BankTransactions { get; set; }
    public DbSet<CustomerTransaction> CustomerTransactions { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Brand> Brands { get; set; }
    public DbSet<Unit> Units { get; set; }
    public DbSet<Warehouse> Warehouses { get; set; }
    public DbSet<StockTransaction> StockTransactions { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        // Identity tablolarının (AspNetUsers vb.) oluşması için
        base.OnModelCreating(builder);

        // Yazdığımız tüm IEntityTypeConfiguration sınıflarını otomatik uygula
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // JWT Token üzerinden sisteme giriş yapmış kullanıcının ID'sini yakalıyoruz
        // Eğer giriş yapmış biri yoksa (örn: seed data) "System" olarak atıyoruz
        var userId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? "System";

        // Sadece BaseEntity'den miras alan ve durumu Added veya Modified olanları bul
        var entries = ChangeTracker.Entries<BaseEntity>();

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedBy = userId;
                entry.Entity.CreatedDate = DateTime.Now;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedBy = userId;
                entry.Entity.UpdatedDate = DateTime.Now;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}