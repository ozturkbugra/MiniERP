using MiniERP.Domain.Common;

namespace MiniERP.Domain.Entities
{
    public sealed class Warehouse : BaseEntity
    {
        public string Code { get; set; } // Örn: DP-01
        public string Name { get; set; } // Örn: Merkez Depo
        public string Location { get; set; }
    }
}