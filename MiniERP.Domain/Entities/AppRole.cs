using MiniERP.Domain.Common;

namespace MiniERP.Domain.Entities
{
    public sealed class AppRole : BaseEntity
    {
        public string Name { get; set; } = string.Empty; // Örn: "Admin", "User", "Manager"
        public string? Description { get; set; }
    }
}
