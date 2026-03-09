using MiniERP.Domain.Common;

namespace MiniERP.Domain.Entities
{
    public sealed class RolePermission : BaseEntity
    {
        public Guid RoleId { get; set; }

        // "Permissions.Reports.View" gibi yetki metinleri
        public string Permission { get; set; } = string.Empty;
    }
}
