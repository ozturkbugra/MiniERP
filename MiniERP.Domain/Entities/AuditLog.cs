using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniERP.Domain.Entities
{
    public sealed class AuditLog
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string TableName { get; set; } = string.Empty;

        // İşlem yapılan kaydın ID'si
        public string EntityId { get; set; } = string.Empty;

        // İşlem Tipi (Create, Update, Delete)
        public string ActionType { get; set; } = string.Empty;

        // Güncellemeden ÖNCEKİ değerler (JSON formatında saklayacağız)
        public string? OldValues { get; set; }

        // Güncellemeden SONRAKİ değerler (JSON formatında saklayacağız)
        public string? NewValues { get; set; }

        // Hangi kullanıcı yaptı? (Şimdilik sistem veya ileride token'dan gelen user)
        public string UserId { get; set; } = string.Empty;

        // İşlem ne zaman yapıldı?
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
