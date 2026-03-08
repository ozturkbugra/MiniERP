using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using MiniERP.Domain.Entities;
using System.Security.Claims;
using System.Text.Json;

namespace MiniERP.Persistence.Interceptors
{
    public sealed class AuditInterceptor : SaveChangesInterceptor
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuditInterceptor(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync( DbContextEventData eventData,InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            var dbContext = eventData.Context;
            if (dbContext == null) return base.SavingChangesAsync(eventData, result, cancellationToken);

            // 1. AuditLog tablosunun kendisini dinleme (Yoksa sonsuz döngüye girer)
            var entries = dbContext.ChangeTracker.Entries()
                .Where(e => e.Entity is not AuditLog &&
                           (e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted))
                .ToList();

            var auditLogs = new List<AuditLog>();
            var userId = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "System";

            foreach (var entry in entries)
            {
                var auditLog = new AuditLog
                {
                    TableName = entry.Entity.GetType().Name,
                    ActionType = entry.State.ToString(),
                    UserId = userId,
                    Timestamp = DateTime.UtcNow
                };

                // 2. Primary Key Bulma
                var primaryKey = entry.Properties.FirstOrDefault(p => p.Metadata.IsPrimaryKey());
                auditLog.EntityId = primaryKey?.CurrentValue?.ToString() ?? "Unknown";

                // 3. Eski ve Yeni Değerler
                if (entry.State == EntityState.Modified || entry.State == EntityState.Deleted)
                {
                    var oldValues = entry.Properties.ToDictionary(p => p.Metadata.Name, p => p.OriginalValue);
                    auditLog.OldValues = JsonSerializer.Serialize(oldValues);
                }

                if (entry.State == EntityState.Added || entry.State == EntityState.Modified)
                {
                    var newValues = entry.Properties.ToDictionary(p => p.Metadata.Name, p => p.CurrentValue);
                    auditLog.NewValues = JsonSerializer.Serialize(newValues);
                }

                auditLogs.Add(auditLog);
            }

            if (auditLogs.Any())
            {
                dbContext.Set<AuditLog>().AddRange(auditLogs);
            }

            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }
    }
}
